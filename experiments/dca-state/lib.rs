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
    MissingBlob,
    BlobMismatch,
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
            Self::MissingBlob => "dca_state.missing_blob",
            Self::BlobMismatch => "dca_state.blob_mismatch",
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

#[derive(Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct ValueBlob {
    encoding: String,
    sha256: String,
    bytes: usize,
}

#[derive(Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct StoredEnvelope {
    storage_format_version: u32,
    kind: String,
    identity: Identity,
    originating_revision: String,
    fixture_source: Option<FixtureSource>,
    value_blob: ValueBlob,
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

    fn storage_envelope_bytes(&self) -> Result<Vec<u8>, Error> {
        self.validate()?;
        let bytes = serde_json::to_vec(&StoredEnvelope {
            storage_format_version: 2,
            kind: "sdk-string-with-utf8-blob".into(),
            identity: self.content.identity.clone(),
            originating_revision: self.content.originating_revision.clone(),
            fixture_source: self.content.fixture_source.clone(),
            value_blob: ValueBlob {
                encoding: "utf-8".into(),
                sha256: digest(self.value().as_bytes()),
                bytes: self.value().len(),
            },
            content_sha256: self.content_sha256.clone(),
        })?;
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
        self.decode_stored(&bytes, expected).map(|(state, _)| state)
    }

    fn read_artifact(&self, sha256: &str) -> Result<Vec<u8>, Error> {
        self.read_artifact_open(sha256).map(|(bytes, _)| bytes)
    }

    fn read_artifact_open(&self, sha256: &str) -> Result<(Vec<u8>, File), Error> {
        hash_field(sha256, "artifact_sha256")?;
        Self::read_hashed_open(&self.path_for(sha256), sha256, MAX_ENVELOPE_BYTES)
    }

    fn read_hashed_open(
        path: &Path,
        sha256: &str,
        maximum: usize,
    ) -> Result<(Vec<u8>, File), Error> {
        let metadata = fs::symlink_metadata(path).map_err(|error| {
            if error.kind() == io::ErrorKind::NotFound {
                Error::MissingArtifact
            } else {
                Error::Io(error)
            }
        })?;
        if !metadata.is_file() || metadata.file_type().is_symlink() {
            return Err(Error::NotRegular);
        }
        if metadata.len() > maximum as u64 {
            return Err(Error::Limit);
        }
        let mut file = File::open(path)?;
        if !file.metadata()?.is_file() {
            return Err(Error::NotRegular);
        }
        let mut bytes = Vec::new();
        (&mut file)
            .take(maximum as u64 + 1)
            .read_to_end(&mut bytes)?;
        if bytes.len() > maximum {
            return Err(Error::Limit);
        }
        if digest(&bytes) != sha256 {
            return Err(Error::IntegrityMismatch);
        }
        Ok((bytes, file))
    }

    fn blob_directory(&self) -> PathBuf {
        self.root.join("configuration-blobs").join("sha256")
    }

    fn decode_stored(
        &self,
        bytes: &[u8],
        expected: &Identity,
    ) -> Result<(State, Option<File>), Error> {
        expected.validate()?;
        let envelope = match serde_json::from_slice::<StoredEnvelope>(bytes) {
            Ok(envelope) => envelope,
            // Historical v1 inline artifacts remain readable. New saves always
            // publish v2 envelopes with a mandatory blob.
            Err(_) => return State::decode(bytes, expected).map(|state| (state, None)),
        };
        if envelope.storage_format_version != 2 || envelope.kind != "sdk-string-with-utf8-blob" {
            return Err(Error::UnsupportedVersion);
        }
        if &envelope.identity != expected {
            return Err(Error::IdentityMismatch);
        }
        if serde_json::to_vec(&envelope)? != bytes {
            return Err(Error::NonCanonical);
        }
        if envelope.value_blob.encoding != "utf-8" {
            return Err(Error::BlobMismatch);
        }
        hash_field(&envelope.value_blob.sha256, "value_blob_sha256")?;
        if envelope.value_blob.bytes > MAX_VALUE_BYTES {
            return Err(Error::Limit);
        }
        for directory in [self.root.join("configuration-blobs"), self.blob_directory()] {
            let metadata = fs::symlink_metadata(directory).map_err(|error| {
                if error.kind() == io::ErrorKind::NotFound {
                    Error::MissingBlob
                } else {
                    Error::Io(error)
                }
            })?;
            if !metadata.is_dir() || metadata.file_type().is_symlink() {
                return Err(Error::NotRegular);
            }
        }
        let path = self
            .blob_directory()
            .join(format!("{}.bin", envelope.value_blob.sha256));
        let (payload, file) =
            Self::read_hashed_open(&path, &envelope.value_blob.sha256, MAX_VALUE_BYTES).map_err(
                |error| match error {
                    Error::MissingArtifact => Error::MissingBlob,
                    Error::IntegrityMismatch => Error::BlobMismatch,
                    other => other,
                },
            )?;
        if payload.len() != envelope.value_blob.bytes {
            return Err(Error::BlobMismatch);
        }
        let value = String::from_utf8(payload).map_err(|_| Error::BlobMismatch)?;
        let state = State {
            content: Content {
                format_version: FORMAT_VERSION,
                identity: envelope.identity,
                originating_revision: envelope.originating_revision,
                value,
                fixture_source: envelope.fixture_source,
            },
            content_sha256: envelope.content_sha256,
        };
        state.validate()?;
        Ok((state, Some(file)))
    }

    pub fn save(&self, state: &State) -> Result<ArtifactReceipt, Error> {
        let bytes = state.storage_envelope_bytes()?;
        let sha256 = digest(&bytes);
        let outcome = self.persist(&sha256, &bytes, state)?;
        Ok(ArtifactReceipt {
            sha256,
            bytes: bytes.len(),
            outcome,
        })
    }

    #[cfg(unix)]
    fn persist(&self, sha256: &str, bytes: &[u8], state: &State) -> Result<SaveOutcome, Error> {
        match self.read_artifact_open(sha256) {
            Ok((existing, file)) => {
                if existing != bytes {
                    return Err(Error::IntegrityMismatch);
                }
                let (stored, blob) = self.decode_stored(&existing, state.identity())?;
                if stored != *state {
                    return Err(Error::IntegrityMismatch);
                }
                if let Some(blob) = blob {
                    blob.sync_all()?;
                }
                file.sync_all()?;
                self.sync_blob_directories()?;
                return Ok(SaveOutcome::Unchanged);
            }
            Err(Error::MissingArtifact) => {}
            Err(error) => return Err(error),
        }
        // Blob publication precedes the envelope. A failed envelope publication
        // can leave only an unreferenced immutable blob, never a success receipt.
        self.ensure_blob_directories()?;
        Self::publish_hashed(
            &self.blob_directory(),
            &digest(state.value().as_bytes()),
            "bin",
            state.value().as_bytes(),
            MAX_VALUE_BYTES,
        )
        .map_err(|error| match error {
            Error::IntegrityMismatch => Error::BlobMismatch,
            other => other,
        })?;
        Self::publish_hashed(&self.root, sha256, "json", bytes, MAX_ENVELOPE_BYTES)
    }

    #[cfg(unix)]
    fn ensure_blob_directories(&self) -> Result<(), Error> {
        let mut parent = self.root.clone();
        for name in ["configuration-blobs", "sha256"] {
            let directory = parent.join(name);
            match fs::create_dir(&directory) {
                Ok(()) => {}
                Err(error) if error.kind() == io::ErrorKind::AlreadyExists => {}
                Err(error) => return Err(Error::Io(error)),
            }
            let metadata = fs::symlink_metadata(&directory)?;
            if !metadata.is_dir() || metadata.file_type().is_symlink() {
                return Err(Error::NotRegular);
            }
            File::open(&parent)?.sync_all()?;
            parent = directory;
        }
        Ok(())
    }

    #[cfg(unix)]
    fn sync_blob_directories(&self) -> Result<(), Error> {
        for directory in [
            self.blob_directory(),
            self.root.join("configuration-blobs"),
            self.root.clone(),
        ] {
            File::open(directory)?.sync_all()?;
        }
        Ok(())
    }

    #[cfg(unix)]
    fn publish_hashed(
        directory: &Path,
        sha256: &str,
        suffix: &str,
        bytes: &[u8],
        maximum: usize,
    ) -> Result<SaveOutcome, Error> {
        use std::io::Write;
        let path = directory.join(format!("{sha256}.{suffix}"));
        match Self::read_hashed_open(&path, sha256, maximum) {
            Ok((existing, file)) => {
                if existing != bytes {
                    return Err(Error::IntegrityMismatch);
                }
                file.sync_all()?;
                File::open(directory)?.sync_all()?;
                return Ok(SaveOutcome::Unchanged);
            }
            Err(Error::MissingArtifact) => {}
            Err(error) => return Err(error),
        }
        let mut staging = tempfile::NamedTempFile::new_in(directory)?;
        staging.write_all(bytes)?;
        staging.as_file().sync_all()?;
        let outcome = match staging.persist_noclobber(&path) {
            Ok(_) => SaveOutcome::Created,
            Err(error) if error.error.kind() == io::ErrorKind::AlreadyExists => {
                let (existing, file) = Self::read_hashed_open(&path, sha256, maximum)?;
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
        File::open(directory)?.sync_all()?;
        Ok(outcome)
    }

    #[cfg(not(unix))]
    fn persist(&self, _sha256: &str, _bytes: &[u8], _state: &State) -> Result<SaveOutcome, Error> {
        // Do not acknowledge durability before platform-tested directory flush.
        Err(Error::UnsupportedDurability)
    }
}

#[cfg(test)]
mod tests;
