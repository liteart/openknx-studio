// SPDX-License-Identifier: Apache-2.0
use super::*;
use nix::unistd::fchown;
use std::fs;
use std::os::unix::fs::{FileTypeExt, PermissionsExt, symlink};
use std::os::unix::net::UnixListener;

struct Fixture {
    directory: tempfile::TempDir,
    parent: File,
    receipt: Receipt,
    listener: Option<UnixListener>,
}

fn mode(path: &Path, value: u32) {
    let file = File::open(path).unwrap();
    fchown(&file, None, Some(getegid())).unwrap();
    fs::set_permissions(path, fs::Permissions::from_mode(value)).unwrap();
}

fn make_fixture() -> Fixture {
    let directory = tempfile::Builder::new()
        .prefix("t153-recover-")
        .tempdir_in("/private/tmp")
        .unwrap();
    let parent = File::open(directory.path()).unwrap();
    let store = directory.path().join(STORE);
    fs::create_dir(&store).unwrap();
    mode(&store, 0o711);
    fs::create_dir(store.join("execution")).unwrap();
    mode(&store.join("execution"), 0o700);
    let uid = geteuid().as_raw();
    let manifest =
        serde_json::to_vec(&serde_json::json!({"operator_uid":uid,"agent_uid":uid})).unwrap();
    let plan = serde_json::to_vec(&serde_json::json!({"expires_at_utc_seconds":900})).unwrap();
    fs::write(store.join("service-manifest.json"), &manifest).unwrap();
    fs::write(store.join("write-plan.json"), &plan).unwrap();
    mode(&store.join("service-manifest.json"), 0o600);
    mode(&store.join("write-plan.json"), 0o600);
    let socket = store.join("operator.sock");
    let listener = UnixListener::bind(&socket).unwrap();
    nix::unistd::chown(&socket, None, Some(getegid())).unwrap();
    fs::set_permissions(&socket, fs::Permissions::from_mode(0o600)).unwrap();
    exacl::setfacl(
        &[&socket],
        &[AclEntry::allow_user(&uid.to_string(), Perm::WRITE, None)],
        Some(AclOption::SYMLINK_ACL),
    )
    .unwrap();
    let acl = getfacl(&socket, Some(AclOption::SYMLINK_ACL | AclOption::NATIVE_ID)).unwrap();
    Fixture {
        directory,
        parent,
        receipt: Receipt {
            schema: 1,
            recovery: Pins {
                executable_sha256: [1; 32],
                executable_cdhash: "synthetic".into(),
            },
            manifest_sha256: Sha256::digest(&manifest).into(),
            plan_sha256: Sha256::digest(&plan).into(),
            operator_uid: uid,
            socket_acl_uuid: acl[0].name.clone(),
            expires_at_utc_seconds: 1100,
        },
        listener: Some(listener),
    }
}

fn attempt(fixture: &Fixture) -> Result<(), Error> {
    quarantine(
        &fixture.parent,
        fixture.directory.path(),
        geteuid().as_raw(),
        &fixture.receipt,
        1000,
        |_| Ok(()),
    )
}

#[test]
fn stale_unused_store_is_preserved_as_one_atomic_quarantine() {
    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    let original = fs::read(fixture.directory.path().join(STORE).join("write-plan.json")).unwrap();
    assert_eq!(attempt(&fixture), Ok(()));
    assert!(!fixture.directory.path().join(STORE).exists());
    let preserved = fixture.directory.path().join(QUARANTINE);
    assert_eq!(
        fs::read(preserved.join("write-plan.json")).unwrap(),
        original
    );
    assert!(
        fs::symlink_metadata(preserved.join("operator.sock"))
            .unwrap()
            .file_type()
            .is_socket()
    );
    assert_eq!(fs::read_dir(preserved).unwrap().count(), 4);
}

#[test]
fn active_listener_refuses_recovery_without_sending_application_data() {
    let fixture = make_fixture();
    assert_eq!(attempt(&fixture), Err(Error::ListenerNotProvenAbsent));
    assert!(fixture.directory.path().join(STORE).exists());
    assert!(!fixture.directory.path().join(QUARANTINE).exists());
}

#[test]
fn enrollment_assertion_unknown_and_execution_state_are_preserved_and_refused() {
    for name in [
        "config.json",
        "enrollment.lock",
        "verified-assertion.json",
        ".unknown",
        "execution/attempt.json",
    ] {
        let mut fixture = make_fixture();
        drop(fixture.listener.take());
        let path = fixture.directory.path().join(STORE).join(name);
        fs::write(&path, b"must remain").unwrap();
        assert_eq!(attempt(&fixture), Err(Error::StatePresent), "{name}");
        assert_eq!(fs::read(path).unwrap(), b"must remain");
        assert!(!fixture.directory.path().join(QUARANTINE).exists());
    }
    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    symlink(
        "missing",
        fixture.directory.path().join(STORE).join("unknown-link"),
    )
    .unwrap();
    assert_eq!(attempt(&fixture), Err(Error::StatePresent));
}

#[test]
fn changed_original_inputs_or_unexpired_plan_never_move_store() {
    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    fixture.receipt.manifest_sha256[0] ^= 1;
    assert_eq!(attempt(&fixture), Err(Error::StoreRejected));
    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    let path = fixture.directory.path().join(STORE).join("write-plan.json");
    let bytes = br#"{"expires_at_utc_seconds":1001}"#;
    fs::write(path, bytes).unwrap();
    fixture.receipt.plan_sha256 = Sha256::digest(bytes).into();
    assert_eq!(attempt(&fixture), Err(Error::StoreRejected));
    assert!(fixture.directory.path().join(STORE).exists());
}

#[test]
fn existing_quarantine_is_never_overwritten() {
    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    let quarantine = fixture.directory.path().join(QUARANTINE);
    fs::create_dir(&quarantine).unwrap();
    fs::write(quarantine.join("sentinel"), b"preserve").unwrap();
    assert_eq!(attempt(&fixture), Err(Error::QuarantineUnavailable));
    assert_eq!(fs::read(quarantine.join("sentinel")).unwrap(), b"preserve");
    assert!(fixture.directory.path().join(STORE).exists());
}

#[test]
fn replaced_store_is_not_moved_and_postrename_failure_is_uncertain() {
    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    let source = fixture.directory.path().join(STORE);
    let kept = fixture.directory.path().join("original");
    let result = quarantine(
        &fixture.parent,
        fixture.directory.path(),
        geteuid().as_raw(),
        &fixture.receipt,
        1000,
        |stage| {
            if matches!(stage, Stage::Inspected) {
                fs::rename(&source, &kept).unwrap();
                fs::create_dir(&source).unwrap();
            }
            Ok(())
        },
    );
    assert!(matches!(result, Err(Error::StoreRejected)));
    assert!(source.exists() && kept.exists());
    assert!(!fixture.directory.path().join(QUARANTINE).exists());

    let mut fixture = make_fixture();
    drop(fixture.listener.take());
    let result = quarantine(
        &fixture.parent,
        fixture.directory.path(),
        geteuid().as_raw(),
        &fixture.receipt,
        1000,
        |stage| {
            if matches!(stage, Stage::Renamed) {
                Err(Error::Io)
            } else {
                Ok(())
            }
        },
    );
    assert_eq!(result, Err(Error::PublishedUncertain));
    assert!(fixture.directory.path().join(QUARANTINE).exists());
    assert!(!fixture.directory.path().join(STORE).exists());
    assert!(attempt(&fixture).is_err());
}

#[test]
fn nonroot_process_and_arbitrary_arguments_cannot_reach_recovery() {
    assert_eq!(parse_arguments(&[]), Err(Error::Arguments));
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        assert_eq!(
            run(vec![
                "--receipt".into(),
                "/nonexistent".into(),
                "--receipt-sha256".into(),
                "0".repeat(64).into()
            ]),
            Err(Error::RootRequired)
        );
    }
}
