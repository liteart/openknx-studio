// SPDX-License-Identifier: Apache-2.0
//! Actual filesystem fault fixtures, never protected trust or device grants.
use super::*;
use std::fs;
use std::os::unix::fs::symlink;

#[test]
fn actual_record_persists_private_bytes_in_order_and_never_overwrites() {
    let directory = tempfile::tempdir_in("/private/tmp").unwrap();
    let parent = File::open(directory.path()).unwrap();
    let bytes = br#"{"fixture":"synthetic-public-counter-state"}"#;
    let mut stages = Vec::new();
    create_record(&parent, bytes, |stage| {
        stages.push(stage);
        Ok(())
    })
    .unwrap();
    assert_eq!(
        stages,
        [
            Stage::Created,
            Stage::ReservationDurable,
            Stage::Written,
            Stage::FileSynced,
            Stage::Committed
        ]
    );
    let path = directory.path().join(RECORD);
    assert_eq!(fs::metadata(&path).unwrap().mode() & 0o7777, 0o600);
    assert_eq!(fs::read(&path).unwrap(), bytes);
    assert_eq!(
        create_record(&parent, b"replacement", |_| Ok(())),
        Err(Error::AlreadyConsumed)
    );
    assert_eq!(fs::read(&path).unwrap(), bytes);
    let reopened = File::open(directory.path()).unwrap();
    assert_eq!(ensure_unused(&reopened), Err(Error::AlreadyConsumed));
}
#[test]
fn interrupted_record_retains_quarantine_at_every_stage() {
    for fault in [
        Stage::Created,
        Stage::ReservationDurable,
        Stage::Written,
        Stage::FileSynced,
        Stage::Committed,
    ] {
        let directory = tempfile::tempdir_in("/private/tmp").unwrap();
        let parent = File::open(directory.path()).unwrap();
        assert_eq!(
            create_record(&parent, b"synthetic-updated-counter", |stage| {
                if stage == fault {
                    Err(Error::Io)
                } else {
                    Ok(())
                }
            }),
            Err(Error::Io)
        );
        assert_eq!(
            ensure_unused(&File::open(directory.path()).unwrap()),
            Err(Error::AlreadyConsumed)
        );
        assert_eq!(
            create_record(&parent, b"retry", |_| Ok(())),
            Err(Error::AlreadyConsumed)
        );
    }
}
#[test]
fn symlink_is_never_followed_and_renamed_parent_keeps_original_descriptor() {
    let directory = tempfile::tempdir_in("/private/tmp").unwrap();
    let original = directory.path().join("original");
    fs::create_dir(&original).unwrap();
    let parent = File::open(&original).unwrap();
    let target = directory.path().join("target");
    fs::write(&target, b"unchanged").unwrap();
    symlink(&target, original.join(RECORD)).unwrap();
    assert_eq!(
        create_record(&parent, b"synthetic", |_| Ok(())),
        Err(Error::AlreadyConsumed)
    );
    assert_eq!(fs::read(&target).unwrap(), b"unchanged");
    fs::remove_file(original.join(RECORD)).unwrap();
    let moved = directory.path().join("moved");
    fs::rename(&original, &moved).unwrap();
    fs::create_dir(&original).unwrap();
    create_record(&parent, b"synthetic", |_| Ok(())).unwrap();
    assert_eq!(fs::read(moved.join(RECORD)).unwrap(), b"synthetic");
    assert!(!original.join(RECORD).exists());
}
#[test]
fn unprivileged_runtime_and_invalid_bounds_create_no_authority() {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        assert_eq!(ensure_available(), Err(Error::RootRequired));
    }
    let directory = tempfile::tempdir_in("/private/tmp").unwrap();
    let parent = File::open(directory.path()).unwrap();
    for bytes in [&[][..], &vec![0; MAX_CONFIGURATION_BYTES + 1][..]] {
        assert_eq!(
            create_record(&parent, bytes, |_| Ok(())),
            Err(Error::RecordRejected)
        );
    }
    assert_eq!(ensure_unused(&parent), Ok(()));
}
