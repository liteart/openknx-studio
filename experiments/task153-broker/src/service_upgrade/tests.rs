// SPDX-License-Identifier: Apache-2.0
use super::*;
use std::fs;
use std::os::unix::fs::{FileTypeExt, PermissionsExt, symlink};
use std::os::unix::net::UnixListener;

struct Fixture {
    directory: tempfile::TempDir,
    helpers: File,
    db: File,
    store: File,
}

fn fixture() -> Fixture {
    let directory = tempfile::Builder::new()
        .prefix("t153-upgrade-")
        .tempdir_in("/private/tmp")
        .unwrap();
    let helpers = directory.path().join("helpers");
    let db = directory.path().join("db");
    fs::create_dir(&helpers).unwrap();
    fs::create_dir(&db).unwrap();
    let store = db.join(OLD);
    fs::create_dir(&store).unwrap();
    fs::create_dir(store.join("execution")).unwrap();
    fs::write(store.join(MANIFEST), b"old manifest").unwrap();
    fs::write(store.join(PLAN), b"unchanged plan").unwrap();
    fs::write(helpers.join(OLD), b"old service").unwrap();
    fs::write(helpers.join(NEXT), b"new service").unwrap();
    Fixture {
        helpers: File::open(helpers).unwrap(),
        db: File::open(db).unwrap(),
        store: File::open(store).unwrap(),
        directory,
    }
}

fn publish_fixture(f: &Fixture) -> Result<(), Error> {
    publish(
        &f.helpers,
        &f.db,
        &f.store,
        b"new manifest",
        b"approved receipt",
        false,
        |_| Ok(()),
    )
}

#[test]
fn successful_upgrade_preserves_old_data_and_does_not_touch_plan_or_execution() {
    let f = fixture();
    assert_eq!(publish_fixture(&f), Ok(()));
    let root = f.directory.path();
    assert_eq!(
        fs::read(root.join("helpers").join(OLD)).unwrap(),
        b"new service"
    );
    assert_eq!(
        fs::read(root.join("helpers").join(SAVED)).unwrap(),
        b"old service"
    );
    assert_eq!(
        fs::read(root.join("db").join(FENCE).join(MANIFEST)).unwrap(),
        b"old manifest"
    );
    assert_eq!(
        fs::read(root.join("db").join(FENCE).join("receipt.json")).unwrap(),
        b"approved receipt"
    );
    let store = root.join("db").join(OLD);
    assert_eq!(fs::read(store.join(MANIFEST)).unwrap(), b"new manifest");
    assert_eq!(fs::read(store.join(PLAN)).unwrap(), b"unchanged plan");
    assert_eq!(fs::read_dir(store.join("execution")).unwrap().count(), 0);
    assert_eq!(publish_fixture(&f), Err(Error::ExistingOrUnavailable));
}

#[test]
fn every_publication_interruption_preserves_bytes_fences_retry_and_reports_uncertain() {
    for stage in [
        Stage::Prepared,
        Stage::SocketPreserved,
        Stage::OldBinaryPreserved,
        Stage::NewBinaryPublished,
        Stage::OldManifestPreserved,
        Stage::Published,
    ] {
        let f = fixture();
        assert_eq!(
            publish(
                &f.helpers,
                &f.db,
                &f.store,
                b"new manifest",
                b"receipt",
                false,
                |observed| if observed == stage {
                    Err(Error::Io)
                } else {
                    Ok(())
                }
            ),
            Err(Error::UpgradeUncertain)
        );
        assert_eq!(publish_fixture(&f), Err(Error::ExistingOrUnavailable));
        let root = f.directory.path();
        let helpers = root.join("helpers");
        let db = root.join("db");
        let old_binary = if helpers.join(SAVED).exists() {
            helpers.join(SAVED)
        } else {
            helpers.join(OLD)
        };
        assert_eq!(fs::read(old_binary).unwrap(), b"old service");
        let old_manifest = if db.join(FENCE).join(MANIFEST).exists() {
            db.join(FENCE).join(MANIFEST)
        } else {
            db.join(OLD).join(MANIFEST)
        };
        assert_eq!(fs::read(old_manifest).unwrap(), b"old manifest");
        assert_eq!(
            fs::read(db.join(OLD).join(PLAN)).unwrap(),
            b"unchanged plan"
        );
    }
}

#[test]
fn existing_backup_and_symlink_fence_are_never_overwritten() {
    let f = fixture();
    let saved = f.directory.path().join("helpers").join(SAVED);
    fs::write(&saved, b"preserve backup").unwrap();
    assert_eq!(publish_fixture(&f), Err(Error::ExistingOrUnavailable));
    assert_eq!(fs::read(saved).unwrap(), b"preserve backup");
    let f = fixture();
    let sentinel = f.directory.path().join("sentinel");
    fs::write(&sentinel, b"preserve").unwrap();
    symlink(&sentinel, f.directory.path().join("db").join(FENCE)).unwrap();
    assert_eq!(publish_fixture(&f), Err(Error::ExistingOrUnavailable));
    assert_eq!(fs::read(sentinel).unwrap(), b"preserve");
}

#[test]
fn replacement_changes_only_service_code_pins_and_rejects_identity_mutation() {
    let receipt = Receipt {
        schema: 1,
        upgrader: Pins {
            executable_sha256: [1; 32],
            executable_cdhash: "1".repeat(40),
        },
        old_service: Pins {
            executable_sha256: [2; 32],
            executable_cdhash: "2".repeat(40),
        },
        new_service: Pins {
            executable_sha256: [3; 32],
            executable_cdhash: "3".repeat(40),
        },
        manifest_sha256: [4; 32],
        plan_sha256: [5; 32],
        operator_uid: 501,
        socket_acl_uuid: "native-test-uuid".into(),
        expires_at_utc_seconds: 1000,
    };
    let old = serde_json::json!({"operator_uid":501,"agent_uid":501,"operator_acl_uuid":"native-test-uuid",
        "executable_sha256":receipt.old_service.executable_sha256,"executable_cdhash":"2".repeat(40),"plan_digest":vec![9;32],
        "enrollment_bootstrap":{"executable_sha256":vec![8;32]},"gateway":"synthetic"});
    let mut expected = old.clone();
    expected["executable_sha256"] = serde_json::json!(receipt.new_service.executable_sha256);
    expected["executable_cdhash"] = serde_json::json!("3".repeat(40));
    let updated = replacement_manifest(&serde_json::to_vec(&old).unwrap(), &receipt).unwrap();
    assert_eq!(
        serde_json::from_slice::<serde_json::Value>(&updated).unwrap(),
        expected
    );
    let mut changed = old;
    changed["operator_uid"] = serde_json::json!(502);
    assert_eq!(
        replacement_manifest(&serde_json::to_vec(&changed).unwrap(), &receipt),
        Err(Error::StateRejected)
    );
}

#[test]
fn stale_socket_is_preserved_and_active_listener_is_refused() {
    let f = fixture();
    let path = f
        .directory
        .path()
        .join("db")
        .join(OLD)
        .join("operator.sock");
    let listener = UnixListener::bind(&path).unwrap();
    nix::unistd::chown(&path, None, Some(getegid())).unwrap();
    fs::set_permissions(&path, fs::Permissions::from_mode(0o600)).unwrap();
    exacl::setfacl(
        &[&path],
        &[AclEntry::allow_user(
            &geteuid().to_string(),
            Perm::WRITE,
            None,
        )],
        Some(AclOption::SYMLINK_ACL),
    )
    .unwrap();
    let native =
        exacl::getfacl(&path, Some(AclOption::NATIVE_ID | AclOption::SYMLINK_ACL)).unwrap();
    assert_eq!(
        socket_stamp(&f.store, &path, geteuid().as_raw(), &native[0].name),
        Err(Error::ListenerNotProvenAbsent)
    );
    drop(listener);
    assert!(
        socket_stamp(&f.store, &path, geteuid().as_raw(), &native[0].name)
            .unwrap()
            .is_some()
    );
    assert_eq!(
        publish(
            &f.helpers,
            &f.db,
            &f.store,
            b"new manifest",
            b"receipt",
            true,
            |_| Ok(())
        ),
        Ok(())
    );
    assert!(
        fs::symlink_metadata(
            f.directory
                .path()
                .join("db")
                .join(FENCE)
                .join("operator.sock")
        )
        .unwrap()
        .file_type()
        .is_socket()
    );
    assert!(!path.exists());
}

#[test]
fn bounded_store_scan_rejects_unknown_and_nested_operation_evidence() {
    let f = fixture();
    let store = f.directory.path().join("db").join(OLD);
    for name in [
        "config.json",
        "enrollment.pending.json",
        "assertion.json",
        "unknown",
    ] {
        fs::write(store.join(name), b"keep").unwrap();
    }
    assert_eq!(entries(&f.store), Err(Error::StateRejected));
    fs::write(store.join("execution/intent.json"), b"uncertain").unwrap();
    let execution = File::open(store.join("execution")).unwrap();
    assert!(!entries(&execution).unwrap().is_empty());
    assert_eq!(
        fs::read(store.join("execution/intent.json")).unwrap(),
        b"uncertain"
    );
}

#[test]
fn entrypoint_rejects_ordinary_user_before_reading_receipt_or_mutating_anything() {
    if geteuid().as_raw() == 0 {
        return;
    }
    assert_eq!(
        run(vec![
            "--receipt".into(),
            "/does-not-exist".into(),
            "--receipt-sha256".into(),
            "00".repeat(32).into()
        ]),
        Err(Error::RootRequired)
    );
    assert_eq!(arguments(&["--command".into()]), Err(Error::Arguments));
}
