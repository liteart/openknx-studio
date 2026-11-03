use super::*;

pub(crate) fn identity() -> Identity {
    Identity {
        device_uuid: "ba0332b4-cc78-4c94-80b2-829e3c527fd1".into(),
        application_id: "owned-reference-application".into(),
        product_sha256: "a".repeat(64),
        dca_sha256: "b".repeat(64),
        dca_version: "owned-test-1".into(),
        parameter_unique_id: "owned-parameter-1".into(),
        parameter_internal_name: "owned-description".into(),
    }
}

fn state(value: &str) -> State {
    State::capture(identity(), "c".repeat(40), value.into()).unwrap()
}

#[test]
fn scalar_round_trip_is_canonical_and_bound_to_every_identity_field() {
    let original = state("owned <value> Ω");
    let bytes = original.canonical_bytes().unwrap();
    assert_eq!(State::decode(&bytes, &identity()).unwrap(), original);
    for field in 0..7 {
        let mut wrong = identity();
        match field {
            0 => wrong.device_uuid.replace_range(..1, "c"),
            1 => wrong.application_id.push('x'),
            2 => wrong.product_sha256.replace_range(..1, "c"),
            3 => wrong.dca_sha256.replace_range(..1, "c"),
            4 => wrong.dca_version.push('x'),
            5 => wrong.parameter_unique_id.push('x'),
            _ => wrong.parameter_internal_name.push('x'),
        }
        assert!(matches!(
            State::decode(&bytes, &wrong),
            Err(Error::IdentityMismatch)
        ));
    }
}

#[test]
fn rejects_duplicate_unknown_noncanonical_corrupt_and_oversized_input() {
    let bytes = state("owned").canonical_bytes().unwrap();
    let json = String::from_utf8(bytes.clone()).unwrap();
    let duplicate = json.replacen(
        "\"format_version\":1",
        "\"format_version\":1,\"format_version\":1",
        1,
    );
    let nested = json.replacen(
        "\"device_uuid\":",
        "\"device_uuid\":\"ba0332b4-cc78-4c94-80b2-829e3c527fd1\",\"device_uuid\":",
        1,
    );
    let unknown = json.replacen("{", "{\"unknown\":false,", 1);
    for invalid in [duplicate, nested, unknown] {
        assert!(matches!(
            State::decode(invalid.as_bytes(), &identity()),
            Err(Error::Json(_))
        ));
    }
    let corrupted = json.replacen("owned\"", "other\"", 1);
    assert!(matches!(
        State::decode(corrupted.as_bytes(), &identity()),
        Err(Error::IntegrityMismatch)
    ));
    let mut whitespace = bytes;
    whitespace.push(b'\n');
    assert!(matches!(
        State::decode(&whitespace, &identity()),
        Err(Error::NonCanonical)
    ));
    assert!(matches!(
        State::decode(&vec![b' '; MAX_ENVELOPE_BYTES + 1], &identity()),
        Err(Error::Limit)
    ));
}

#[test]
fn rejects_unbounded_or_invalid_capture_without_an_artifact() {
    assert!(matches!(
        State::capture(identity(), "c".repeat(40), "x".repeat(MAX_VALUE_BYTES + 1)),
        Err(Error::Limit)
    ));
    assert!(matches!(
        State::capture(identity(), "bad-revision".into(), "v".into()),
        Err(Error::InvalidField("originating_revision"))
    ));
    let mut wrong = identity();
    wrong.device_uuid = "../../outside".into();
    assert!(matches!(
        State::capture(wrong, "c".repeat(40), "v".into()),
        Err(Error::InvalidField("device_uuid"))
    ));
}

#[cfg(unix)]
#[test]
fn durable_artifact_survives_reopen_and_noop_does_not_replace_it() {
    let directory = tempfile::tempdir().unwrap();
    let store = Store::open(directory.path()).unwrap();
    let first = store.save(&state("original UI fixture")).unwrap();
    assert_eq!(first.outcome, SaveOutcome::Created);
    let before = std::fs::metadata(store.path_for(&first.sha256))
        .unwrap()
        .modified()
        .unwrap();
    drop(store);
    let reopened = Store::open(directory.path()).unwrap();
    assert_eq!(
        reopened.load(&first.sha256, &identity()).unwrap(),
        state("original UI fixture")
    );
    let second = reopened.save(&state("original UI fixture")).unwrap();
    assert_eq!(second.outcome, SaveOutcome::Unchanged);
    assert_eq!(first.sha256, second.sha256);
    assert_eq!(
        std::fs::metadata(reopened.path_for(&first.sha256))
            .unwrap()
            .modified()
            .unwrap(),
        before
    );
    assert_eq!(std::fs::read_dir(directory.path()).unwrap().count(), 2);
}

#[cfg(unix)]
#[test]
fn existing_external_artifact_is_verified_and_flushed_before_noop_receipt() {
    let directory = tempfile::tempdir().unwrap();
    let store = Store::open(directory.path()).unwrap();
    let state = state("externally published fixture");
    let bytes = state.storage_envelope_bytes().unwrap();
    let sha256 = digest(&bytes);
    // This writer deliberately does not call sync_all. Save must not rely on
    // another writer having made a matching, visible file durable already.
    std::fs::write(store.path_for(&sha256), &bytes).unwrap();
    std::fs::create_dir_all(store.blob_directory()).unwrap();
    std::fs::write(
        store
            .blob_directory()
            .join(format!("{}.bin", digest(state.value().as_bytes()))),
        state.value().as_bytes(),
    )
    .unwrap();
    let receipt = store.save(&state).unwrap();
    assert_eq!(receipt.outcome, SaveOutcome::Unchanged);
    assert_eq!(receipt.sha256, sha256);
    assert_eq!(store.load(&sha256, &identity()).unwrap(), state);
    assert_eq!(std::fs::read_dir(directory.path()).unwrap().count(), 2);
}

#[cfg(unix)]
#[test]
fn missing_or_corrupt_artifact_fails_without_overwriting_evidence() {
    let directory = tempfile::tempdir().unwrap();
    let store = Store::open(directory.path()).unwrap();
    assert!(matches!(
        store.load(&"a".repeat(64), &identity()),
        Err(Error::MissingArtifact)
    ));
    let receipt = store.save(&state("owned")).unwrap();
    let path = store.path_for(&receipt.sha256);
    std::fs::write(&path, b"corrupt evidence").unwrap();
    assert!(matches!(
        store.load(&receipt.sha256, &identity()),
        Err(Error::IntegrityMismatch)
    ));
    assert!(matches!(
        store.save(&state("owned")),
        Err(Error::IntegrityMismatch)
    ));
    assert_eq!(std::fs::read(path).unwrap(), b"corrupt evidence");
}

#[cfg(unix)]
#[test]
fn rejects_symlink_store_and_artifacts() {
    use std::os::unix::fs::symlink;
    let parent = tempfile::tempdir().unwrap();
    let real = parent.path().join("real");
    std::fs::create_dir(&real).unwrap();
    let link = parent.path().join("link");
    symlink(&real, &link).unwrap();
    assert!(matches!(Store::open(&link), Err(Error::NotRegular)));
    let store = Store::open(&real).unwrap();
    let external = parent.path().join("external");
    std::fs::write(&external, b"outside").unwrap();
    symlink(&external, store.path_for(&"a".repeat(64))).unwrap();
    assert!(matches!(
        store.load(&"a".repeat(64), &identity()),
        Err(Error::NotRegular)
    ));
}

#[cfg(unix)]
#[test]
fn concurrent_identical_saves_publish_only_one_complete_artifact() {
    let directory = tempfile::tempdir().unwrap();
    let workers: Vec<_> = (0..4)
        .map(|_| {
            let path = directory.path().to_path_buf();
            std::thread::spawn(move || Store::open(&path).unwrap().save(&state("same")).unwrap())
        })
        .collect();
    let receipts: Vec<_> = workers
        .into_iter()
        .map(|worker| worker.join().unwrap())
        .collect();
    assert_eq!(
        receipts
            .iter()
            .filter(|receipt| receipt.outcome == SaveOutcome::Created)
            .count(),
        1
    );
    assert!(
        receipts
            .iter()
            .all(|receipt| receipt.sha256 == receipts[0].sha256)
    );
    let store = Store::open(directory.path()).unwrap();
    assert_eq!(
        store.load(&receipts[0].sha256, &identity()).unwrap(),
        state("same")
    );
    assert_eq!(std::fs::read_dir(directory.path()).unwrap().count(), 2);
}

#[cfg(unix)]
#[test]
fn mandatory_utf8_blob_is_verified_and_never_silently_healed() {
    let directory = tempfile::tempdir().unwrap();
    let store = Store::open(directory.path()).unwrap();
    let captured = state("known UTF8 Ω");
    let receipt = store.save(&captured).unwrap();
    let envelope_bytes = std::fs::read(store.path_for(&receipt.sha256)).unwrap();
    let envelope: StoredEnvelope = serde_json::from_slice(&envelope_bytes).unwrap();
    assert_eq!(envelope.storage_format_version, 2);
    assert_eq!(envelope.value_blob.encoding, "utf-8");
    assert_eq!(envelope.value_blob.bytes, captured.value().len());
    let blob = store
        .blob_directory()
        .join(format!("{}.bin", envelope.value_blob.sha256));
    assert_eq!(std::fs::read(&blob).unwrap(), captured.value().as_bytes());
    std::fs::remove_file(&blob).unwrap();
    assert!(matches!(
        store.load(&receipt.sha256, &identity()),
        Err(Error::MissingBlob)
    ));
    assert!(matches!(store.save(&captured), Err(Error::MissingBlob)));
    assert!(!blob.exists());
    assert_eq!(
        std::fs::read(store.path_for(&receipt.sha256)).unwrap(),
        envelope_bytes
    );
}

#[cfg(unix)]
#[test]
fn corrupt_blob_or_mismatched_declared_size_is_rejected() {
    let directory = tempfile::tempdir().unwrap();
    let store = Store::open(directory.path()).unwrap();
    let captured = state("known fixture");
    let receipt = store.save(&captured).unwrap();
    let mut envelope: StoredEnvelope =
        serde_json::from_slice(&std::fs::read(store.path_for(&receipt.sha256)).unwrap()).unwrap();
    envelope.value_blob.bytes += 1;
    let mismatched = serde_json::to_vec(&envelope).unwrap();
    let mismatched_hash = digest(&mismatched);
    std::fs::write(store.path_for(&mismatched_hash), mismatched).unwrap();
    assert!(matches!(
        store.load(&mismatched_hash, &identity()),
        Err(Error::BlobMismatch)
    ));
    let blob = store
        .blob_directory()
        .join(format!("{}.bin", envelope.value_blob.sha256));
    std::fs::write(&blob, b"corrupted").unwrap();
    assert!(matches!(
        store.load(&receipt.sha256, &identity()),
        Err(Error::BlobMismatch)
    ));
    assert!(matches!(store.save(&captured), Err(Error::BlobMismatch)));
    assert_eq!(std::fs::read(blob).unwrap(), b"corrupted");
}

#[cfg(unix)]
#[test]
fn historical_inline_artifact_remains_readable_without_rewriting_it() {
    let directory = tempfile::tempdir().unwrap();
    let store = Store::open(directory.path()).unwrap();
    let captured = state("historical fixture");
    let bytes = captured.canonical_bytes().unwrap();
    let old_hash = digest(&bytes);
    std::fs::write(store.path_for(&old_hash), &bytes).unwrap();
    assert_eq!(store.load(&old_hash, &identity()).unwrap(), captured);
    let current = store.save(&captured).unwrap();
    assert_ne!(current.sha256, old_hash);
    assert_eq!(std::fs::read(store.path_for(&old_hash)).unwrap(), bytes);
    assert_eq!(store.load(&current.sha256, &identity()).unwrap(), captured);
}
