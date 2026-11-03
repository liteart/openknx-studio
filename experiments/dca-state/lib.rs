//! Offline Task-012 scalar-state artifacts. This is not the accepted-project
//! transaction service, a DCA bridge, or proof of a Windows restart round-trip.

use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::fs::{self, File};
use std::io::{self, Read};
use std::path::{Path, PathBuf};

pub const MAX_ENVELOPE_BYTES: usize = 16 * 1024;
pub const MAX_VALUE_BYTES: usize = 1024;
const FORMAT_VERSION: u32 = 1;

mod fixture;
pub use fixture::{FixtureExpected, FixtureSource};

#[derive(Debug)]
pub enum Error {
    Io(io::Error),
    Json(serde_json::Error),
    Limit,
    InvalidField(&'static str),
    UnsupportedVersion,
    IdentityMismatch,
    IntegrityMismatch,
    NonCanonical,
    MissingArtifact,
    NotRegular,
    UnsupportedDurability,
    CaptureMismatch,
    UnconfirmedExit,
}

impl Error {
    /// Stable technical code; diagnostics must not echo captured configuration.
    pub fn code(&self) -> &'static str {
        match self {
            Self::Io(_) => "dca_state.io",
            Self::Json(_) => "dca_state.json",
            Self::Limit => "dca_state.limit",
            Self::InvalidField(_) => "dca_state.invalid_field",
            Self::UnsupportedVersion => "dca_state.unsupported_version",
            Self::IdentityMismatch => "dca_state.identity_mismatch",
            Self::IntegrityMismatch => "dca_state.integrity_mismatch",
            Self::NonCanonical => "dca_state.noncanonical",
            Self::MissingArtifact => "dca_state.missing_artifact",
            Self::NotRegular => "dca_state.not_regular",
            Self::UnsupportedDurability => "dca_state.unsupported_durability",
            Self::CaptureMismatch => "dca_state.capture_mismatch",
            Self::UnconfirmedExit => "dca_state.unconfirmed_exit",
        }
    }
}

impl std::fmt::Display for Error {
    fn fmt(&self, formatter: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        formatter.write_str(self.code())
    }
}

impl std::error::Error for Error {
    fn source(&self) -> Option<&(dyn std::error::Error + 'static)> {
        match self {
            Self::Io(error) => Some(error),
            Self::Json(error) => Some(error),
            _ => None,
        }
    }
}

impl From<io::Error> for Error {
    fn from(error: io::Error) -> Self {
        Self::Io(error)
    }
}

impl From<serde_json::Error> for Error {
    fn from(error: serde_json::Error) -> Self {
        Self::Json(error)
    }
}

/// Immutable association supplied by a qualified caller, not inferred from UI text.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct Identity {
    pub device_uuid: String,
    pub application_id: String,
    pub product_sha256: String,
    pub dca_sha256: String,
    pub dca_version: String,
    pub parameter_unique_id: String,
    pub parameter_internal_name: String,
}

impl Identity {
    fn validate(&self) -> Result<(), Error> {
        let uuid = self.device_uuid.as_bytes();
        if uuid.len() != 36
            || !uuid.iter().enumerate().all(|(index, byte)| {
                if [8, 13, 18, 23].contains(&index) {
                    *byte == b'-'
                } else {
                    lower_hex(*byte)
                }
            })
        {
            return Err(Error::InvalidField("device_uuid"));
        }
        text_field(&self.application_id, "application_id")?;
        hash_field(&self.product_sha256, "product_sha256")?;
        hash_field(&self.dca_sha256, "dca_sha256")?;
        text_field(&self.dca_version, "dca_version")?;
        text_field(&self.parameter_unique_id, "parameter_unique_id")?;
        text_field(&self.parameter_internal_name, "parameter_internal_name")
    }
}

#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Content {
    format_version: u32,
    identity: Identity,
    originating_revision: String,
    value: String,
    fixture_source: Option<FixtureSource>,
}

/// The selected SDK value is a known string; no undocumented blob is invented.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct State {
    content: Content,
    content_sha256: String,
}

impl State {
    pub fn capture(
        identity: Identity,
        originating_revision: String,
        value: String,
    ) -> Result<Self, Error> {
        let content = Content {
            format_version: FORMAT_VERSION,
            identity,
            originating_revision,
            value,
            fixture_source: None,
        };
        validate_content(&content)?;
        let content_sha256 = digest(&serde_json::to_vec(&content)?);
        Ok(Self {
            content,
            content_sha256,
        })
    }

    pub fn identity(&self) -> &Identity {
        &self.content.identity
    }

    pub fn value(&self) -> &str {
        &self.content.value
    }

    pub fn originating_revision(&self) -> &str {
        &self.content.originating_revision
    }

    pub fn content_sha256(&self) -> &str {
        &self.content_sha256
    }

    fn validate(&self) -> Result<(), Error> {
        validate_content(&self.content)?;
        hash_field(&self.content_sha256, "content_sha256")?;
        if digest(&serde_json::to_vec(&self.content)?) != self.content_sha256 {
            return Err(Error::IntegrityMismatch);
        }
        Ok(())
    }

    /// Fixed struct field order, compact JSON, UTF-8; no map or locale ordering.
    pub fn canonical_bytes(&self) -> Result<Vec<u8>, Error> {
        self.validate()?;
        let bytes = serde_json::to_vec(self)?;
        if bytes.len() > MAX_ENVELOPE_BYTES {
            return Err(Error::Limit);
        }
        Ok(bytes)
    }

    pub fn decode(bytes: &[u8], expected: &Identity) -> Result<Self, Error> {
        if bytes.len() > MAX_ENVELOPE_BYTES {
            return Err(Error::Limit);
        }
        expected.validate()?;
        // Derived struct visitors reject repeated fields at every object level.
        // serde_json's default recursion bound remains enabled.
        let state: Self = serde_json::from_slice(bytes)?;
        state.validate()?;
        if &state.content.identity != expected {
            return Err(Error::IdentityMismatch);
        }
        if state.canonical_bytes()? != bytes {
            return Err(Error::NonCanonical);
        }
        Ok(state)
    }
}

fn validate_content(content: &Content) -> Result<(), Error> {
    if content.format_version != FORMAT_VERSION {
        return Err(Error::UnsupportedVersion);
    }
    content.identity.validate()?;
    if ![40, 64].contains(&content.originating_revision.len())
        || !content.originating_revision.bytes().all(lower_hex)
    {
        return Err(Error::InvalidField("originating_revision"));
    }
    if content.value.len() > MAX_VALUE_BYTES {
        return Err(Error::Limit);
    }
    if let Some(source) = &content.fixture_source {
        source.validate()?;
    }
    Ok(())
}

fn lower_hex(byte: u8) -> bool {
    byte.is_ascii_digit() || (b'a'..=b'f').contains(&byte)
}

fn hash_field(value: &str, field: &'static str) -> Result<(), Error> {
    if value.len() != 64 || !value.bytes().all(lower_hex) {
        return Err(Error::InvalidField(field));
    }
    Ok(())
}

fn text_field(value: &str, field: &'static str) -> Result<(), Error> {
    if value.is_empty() || value.len() > 256 || value.chars().any(char::is_control) {
        return Err(Error::InvalidField(field));
    }
    Ok(())
}

fn digest(bytes: &[u8]) -> String {
    const HEX: &[u8; 16] = b"0123456789abcdef";
    Sha256::digest(bytes)
        .iter()
        .flat_map(|byte| {
            [
                HEX[(byte >> 4) as usize] as char,
                HEX[(byte & 15) as usize] as char,
            ]
        })
        .collect()
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum SaveOutcome {
    Created,
    Unchanged,
}

/// Receipt for a durable artifact only, never an accepted configuration commit.
#[derive(Debug, PartialEq, Eq)]
pub struct ArtifactReceipt {
    pub sha256: String,
    pub bytes: usize,
    pub outcome: SaveOutcome,
}

/// A dedicated caller-controlled directory, not an imported project path.
/// Hostile concurrent directory replacement requires a later capability-based
/// filesystem service; this PoC does not claim that containment.
pub struct Store {
    root: PathBuf,
}

impl Store {
    pub fn open(root: &Path) -> Result<Self, Error> {
        let metadata = fs::symlink_metadata(root)?;
        if !metadata.is_dir() || metadata.file_type().is_symlink() {
            return Err(Error::NotRegular);
        }
        Ok(Self {
            root: fs::canonicalize(root)?,
        })
    }

    fn path_for(&self, sha256: &str) -> PathBuf {
        self.root.join(format!("{sha256}.json"))
    }

    pub fn load(&self, sha256: &str, expected: &Identity) -> Result<State, Error> {
        let bytes = self.read_artifact(sha256)?;
        State::decode(&bytes, expected)
    }

    fn read_artifact(&self, sha256: &str) -> Result<Vec<u8>, Error> {
        self.read_artifact_open(sha256).map(|(bytes, _)| bytes)
    }

    fn read_artifact_open(&self, sha256: &str) -> Result<(Vec<u8>, File), Error> {
        hash_field(sha256, "artifact_sha256")?;
        let path = self.path_for(sha256);
        let metadata = fs::symlink_metadata(&path).map_err(|error| {
            if error.kind() == io::ErrorKind::NotFound {
                Error::MissingArtifact
            } else {
                Error::Io(error)
            }
        })?;
        if !metadata.is_file() || metadata.file_type().is_symlink() {
            return Err(Error::NotRegular);
        }
        if metadata.len() > MAX_ENVELOPE_BYTES as u64 {
            return Err(Error::Limit);
        }
        let mut file = File::open(path)?;
        if !file.metadata()?.is_file() {
            return Err(Error::NotRegular);
        }
        let mut bytes = Vec::new();
        (&mut file)
            .take(MAX_ENVELOPE_BYTES as u64 + 1)
            .read_to_end(&mut bytes)?;
        if bytes.len() > MAX_ENVELOPE_BYTES {
            return Err(Error::Limit);
        }
        if digest(&bytes) != sha256 {
            return Err(Error::IntegrityMismatch);
        }
        Ok((bytes, file))
    }

    pub fn save(&self, state: &State) -> Result<ArtifactReceipt, Error> {
        let bytes = state.canonical_bytes()?;
        let sha256 = digest(&bytes);
        let outcome = self.persist(&sha256, &bytes)?;
        Ok(ArtifactReceipt {
            sha256,
            bytes: bytes.len(),
            outcome,
        })
    }

    #[cfg(unix)]
    fn persist(&self, sha256: &str, bytes: &[u8]) -> Result<SaveOutcome, Error> {
        use std::io::Write;
        match self.read_artifact_open(sha256) {
            Ok((existing, file)) => {
                if existing != bytes {
                    return Err(Error::IntegrityMismatch);
                }
                // A matching file may have been published by another writer
                // without flushing. Sync the handle whose bytes we validated.
                file.sync_all()?;
                File::open(&self.root)?.sync_all()?;
                return Ok(SaveOutcome::Unchanged);
            }
            Err(Error::MissingArtifact) => {}
            Err(error) => return Err(error),
        }
        let mut staging = tempfile::NamedTempFile::new_in(&self.root)?;
        staging.write_all(bytes)?;
        staging.as_file().sync_all()?;
        let outcome = match staging.persist_noclobber(self.path_for(sha256)) {
            Ok(_) => SaveOutcome::Created,
            Err(error) if error.error.kind() == io::ErrorKind::AlreadyExists => {
                let (existing, file) = self.read_artifact_open(sha256)?;
                if existing != bytes {
                    return Err(Error::IntegrityMismatch);
                }
                file.sync_all()?;
                SaveOutcome::Unchanged
            }
            Err(error) => return Err(Error::Io(error.error)),
        };
        // File content was flushed before publication; now persist the name.
        // Failure is returned even though the artifact may already be visible.
        File::open(&self.root)?.sync_all()?;
        Ok(outcome)
    }

    #[cfg(not(unix))]
    fn persist(&self, _sha256: &str, _bytes: &[u8]) -> Result<SaveOutcome, Error> {
        // Do not acknowledge durability before platform-tested directory flush.
        Err(Error::UnsupportedDurability)
    }
}

#[cfg(test)]
mod tests;
