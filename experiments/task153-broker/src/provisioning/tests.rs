// SPDX-License-Identifier: Apache-2.0
//! Unprivileged real-filesystem fixtures; never the protected destination.
use super::*;
use std::fs;
use std::os::unix::fs::{PermissionsExt, symlink};

fn fixture() -> (tempfile::TempDir, File) {
    let dir = tempfile::Builder::new()
        .prefix("task153-provision-")
        .tempdir_in("/private/tmp")
        .unwrap();
    let file = File::open(dir.path()).unwrap();
    (dir, file)
}
#[test]
fn actual_nonroot_and_unknown_arguments_cannot_reach_inputs_or_publication() {
    assert_eq!(run(vec![]), Err(Error::Arguments));
    assert_ne!(geteuid().as_raw(), 0, "unprivileged source tests only");
    assert_eq!(
        run(vec![
            "--receipt".into(),
            "/must-not-be-read".into(),
            "--receipt-sha256".into(),
            "a".repeat(64).into()
        ]),
        Err(Error::RootRequired)
    );
}
#[test]
fn source_snapshots_bind_exact_bytes_and_reject_symlinks_hash_mismatch_and_bounds() {
    let (dir, _) = fixture();
    let path = dir.path().join("input.json");
    let data = b"{\"schema\":1}";
    fs::write(&path, data).unwrap();
    fs::set_permissions(&path, fs::Permissions::from_mode(0o600)).unwrap();
    let hash = Sha256::digest(data).into();
    assert_eq!(snapshot(&path, hash).unwrap().bytes, data);
    assert!(matches!(
        snapshot(&path, [0; 32]),
        Err(Error::InputRejected)
    ));
    let link = dir.path().join("link");
    symlink(&path, &link).unwrap();
    assert!(matches!(snapshot(&link, hash), Err(Error::InputRejected)));
    let parent_link = dir.path().join("parent-link");
    symlink(dir.path(), &parent_link).unwrap();
    assert!(matches!(
        snapshot(&parent_link.join("input.json"), hash),
        Err(Error::InputRejected)
    ));
    assert_eq!(fs::read(&path).unwrap(), data);
    fs::write(&path, vec![1; MAX_BYTES + 1]).unwrap();
    assert!(matches!(snapshot(&path, hash), Err(Error::InputRejected)));
}
#[test]
fn publication_is_atomic_fixed_only_and_refuses_existing_destination_or_retry() {
    let (dir, parent) = fixture();
    let mut stages = vec![];
    publish(&parent, b"manifest", b"plan", |s| {
        stages.push(s);
        Ok(())
    })
    .unwrap();
    assert_eq!(
        stages,
        [Stage::Created, Stage::DataSynced, Stage::Published]
    );
    let finaldir = dir.path().join(DESTINATION);
    assert_eq!(
        fs::read(finaldir.join("service-manifest.json")).unwrap(),
        b"manifest"
    );
    assert_eq!(fs::read(finaldir.join("write-plan.json")).unwrap(), b"plan");
    assert_eq!(fs::metadata(&finaldir).unwrap().mode() & 0o7777, 0o711);
    assert_eq!(
        fs::metadata(finaldir.join("execution")).unwrap().mode() & 0o7777,
        0o700
    );
    assert_eq!(
        fs::metadata(finaldir.join("write-plan.json"))
            .unwrap()
            .mode()
            & 0o7777,
        0o600
    );
    assert_eq!(fs::read_dir(finaldir.join("execution")).unwrap().count(), 0);
    assert_eq!(
        publish(&parent, b"other", b"other", |_| Ok(())),
        Err(Error::ExistingOrUnavailable)
    );
    assert!(!dir.path().join(STAGING).exists());
    assert_eq!(fs::read(finaldir.join("write-plan.json")).unwrap(), b"plan");
}
#[test]
fn prepublication_failure_quarantines_stage_and_never_exposes_final_store() {
    for point in [Stage::Created, Stage::DataSynced] {
        let (dir, parent) = fixture();
        assert_eq!(
            publish(&parent, b"manifest", b"plan", |s| if s == point {
                Err(Error::Io)
            } else {
                Ok(())
            }),
            Err(Error::Io)
        );
        assert!(dir.path().join(STAGING).is_dir());
        assert!(!dir.path().join(DESTINATION).exists());
        assert_eq!(
            publish(&parent, b"manifest", b"plan", |_| Ok(())),
            Err(Error::ExistingOrUnavailable)
        );
    }
}
#[test]
fn publication_acknowledgement_failure_is_unknown_not_rollback() {
    let (dir, parent) = fixture();
    assert_eq!(
        publish(
            &parent,
            b"manifest",
            b"plan",
            |s| if s == Stage::Published {
                Err(Error::Io)
            } else {
                Ok(())
            }
        ),
        Err(Error::PublishedUncertain)
    );
    assert!(dir.path().join(DESTINATION).is_dir());
    assert!(!dir.path().join(STAGING).exists());
    assert_eq!(
        fs::read(dir.path().join(DESTINATION).join("write-plan.json")).unwrap(),
        b"plan"
    );
}
#[test]
fn racing_destination_and_existing_symlink_are_never_overwritten_or_deleted() {
    let (dir, parent) = fixture();
    assert_eq!(
        publish(&parent, b"manifest", b"plan", |s| {
            if s == Stage::DataSynced {
                fs::create_dir(dir.path().join(DESTINATION)).unwrap();
                fs::write(dir.path().join(DESTINATION).join("original"), b"preserve").unwrap();
            }
            Ok(())
        }),
        Err(Error::ExistingOrUnavailable)
    );
    assert_eq!(
        fs::read(dir.path().join(DESTINATION).join("original")).unwrap(),
        b"preserve"
    );
    assert!(dir.path().join(STAGING).exists());
    let (dir, parent) = fixture();
    symlink("missing", dir.path().join(DESTINATION)).unwrap();
    assert_eq!(
        publish(&parent, b"manifest", b"plan", |_| Ok(())),
        Err(Error::ExistingOrUnavailable)
    );
    assert!(
        fs::symlink_metadata(dir.path().join(DESTINATION))
            .unwrap()
            .file_type()
            .is_symlink()
    );
}
