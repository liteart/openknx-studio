//! Offline fixture helper. Output receipts never include captured values.
use dca_state_poc::{Error, FixtureExpected, MAX_ENVELOPE_BYTES, SaveOutcome, State, Store};
use sha2::{Digest, Sha256};
use std::fs::File;
use std::io::{Read, Write};
use std::path::Path;

fn read(path: &Path) -> Result<Vec<u8>, Error> {
    let mut bytes = Vec::new();
    File::open(path)?
        .take(MAX_ENVELOPE_BYTES as u64 + 1)
        .read_to_end(&mut bytes)?;
    if bytes.len() > MAX_ENVELOPE_BYTES {
        return Err(Error::Limit);
    }
    Ok(bytes)
}

#[cfg(unix)]
fn private_output(path: &Path) -> Result<File, Error> {
    use std::os::unix::fs::OpenOptionsExt;
    Ok(std::fs::OpenOptions::new()
        .write(true)
        .create_new(true)
        .mode(0o600)
        .open(path)?)
}

#[cfg(not(unix))]
fn private_output(_path: &Path) -> Result<File, Error> {
    // No platform-tested private ACL creation is available in this PoC.
    Err(Error::UnsupportedDurability)
}

fn run() -> Result<(), Error> {
    let args: Vec<_> = std::env::args_os().collect();
    match args.get(1).and_then(|argument| argument.to_str()) {
        Some("import-fixture") if args.len() == 7 => {
            let candidate = read(Path::new(&args[2]))?;
            let launcher = read(Path::new(&args[3]))?;
            let completion = read(Path::new(&args[4]))?;
            let expected = FixtureExpected::decode(&read(Path::new(&args[5]))?)?;
            let state = State::import_fixture(&candidate, &launcher, &completion, &expected)?;
            let store = Store::open(Path::new(&args[6]))?;
            let receipt = store.save(&state)?;
            if store.load(&receipt.sha256, &expected.identity)? != state {
                return Err(Error::IntegrityMismatch);
            }
            println!(
                "{}",
                serde_json::json!({
                    "kind":"owned-offline-artifact-receipt",
                    "artifact_sha256":receipt.sha256,
                    "bytes":receipt.bytes,
                    "outcome":match receipt.outcome {SaveOutcome::Created=>"created",SaveOutcome::Unchanged=>"unchanged"},
                    "accepted_configuration":false
                })
            );
        }
        Some("export-fixture") if args.len() == 6 => {
            let store = Store::open(Path::new(&args[2]))?;
            let hash = args[3]
                .to_str()
                .ok_or(Error::InvalidField("artifact_sha256"))?;
            let expected = FixtureExpected::decode(&read(Path::new(&args[4]))?)?;
            let state = store.load(hash, &expected.identity)?;
            let projection = state.fixture_restore_projection_for_artifact(&expected, hash)?;
            let mut output = private_output(Path::new(&args[5]))?;
            output.write_all(&projection)?;
            output.sync_all()?;
            let digest = Sha256::digest(&projection);
            const HEX: &[u8; 16] = b"0123456789abcdef";
            let sha256: String = digest
                .iter()
                .flat_map(|byte| {
                    [
                        HEX[(byte >> 4) as usize] as char,
                        HEX[(byte & 15) as usize] as char,
                    ]
                })
                .collect();
            println!(
                "{}",
                serde_json::json!({"kind":"owned-offline-restore-projection","sha256":sha256,"bytes":projection.len(),"accepted_configuration":false})
            );
        }
        _ => return Err(Error::InvalidField("arguments")),
    }
    Ok(())
}

fn main() {
    if let Err(error) = run() {
        eprintln!("{}", error.code());
        std::process::exit(2);
    }
}
