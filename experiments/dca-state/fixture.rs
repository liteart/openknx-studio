//! Explicitly trusted offline fixture orchestration, not production authorization.
use super::*;
use std::collections::BTreeMap;

const MAX_CANDIDATE_BYTES: usize = 4096;
const MAX_RECEIPT_BYTES: usize = 8192;

#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct FixtureSource {
    pub kind: String,
    pub uuid_revision_synthetic: bool,
    pub project_id: String,
    pub installation_id: String,
    pub device_id: String,
    pub sdk_sha256: String,
    pub object_model_sha256: String,
}

impl FixtureSource {
    pub(super) fn validate(&self) -> Result<(), Error> {
        if self.kind != "owned-offline-fixture" || !self.uuid_revision_synthetic {
            return Err(Error::CaptureMismatch);
        }
        text_field(&self.project_id, "project_id")?;
        text_field(&self.installation_id, "installation_id")?;
        text_field(&self.device_id, "device_id")?;
        hash_field(&self.sdk_sha256, "sdk_sha256")?;
        hash_field(&self.object_model_sha256, "object_model_sha256")
    }
}

/// Independently assembled by the offline test coordinator, never by the DCA.
#[derive(Clone, Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct FixtureExpected {
    pub identity: Identity,
    pub originating_revision: String,
    pub source: FixtureSource,
    pub host_sha256: String,
    pub host_process_id: u32,
    pub candidate_sha256: String,
    pub launcher_stdout_sha256: String,
    pub completion_sha256: String,
    pub expected_value_sha256: String,
}

impl FixtureExpected {
    pub fn decode(bytes: &[u8]) -> Result<Self, Error> {
        if bytes.len() > MAX_RECEIPT_BYTES {
            return Err(Error::Limit);
        }
        let expected: Self = serde_json::from_slice(bytes)?;
        expected.validate()?;
        Ok(expected)
    }

    fn validate(&self) -> Result<(), Error> {
        self.identity.validate()?;
        self.source.validate()?;
        State::capture(
            self.identity.clone(),
            self.originating_revision.clone(),
            String::new(),
        )?;
        for (hash, field) in [
            (&self.host_sha256, "host_sha256"),
            (&self.candidate_sha256, "candidate_sha256"),
            (&self.launcher_stdout_sha256, "launcher_stdout_sha256"),
            (&self.completion_sha256, "completion_sha256"),
            (&self.expected_value_sha256, "expected_value_sha256"),
        ] {
            hash_field(hash, field)?;
        }
        if self.host_process_id == 0 {
            return Err(Error::InvalidField("host_process_id"));
        }
        Ok(())
    }
}

#[derive(Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Candidate {
    format_version: u32,
    kind: String,
    host_process_id: u32,
    project_id: String,
    installation_id: String,
    device_id: String,
    application_id: String,
    product_sha256: String,
    dca_sha256: String,
    dca_version: String,
    sdk_sha256: String,
    object_model_sha256: String,
    host_sha256: String,
    parameter_unique_id: String,
    parameter_internal_name: String,
    value: String,
    host_final_validation: String,
    requires_confirmed_exit_zero: bool,
    accepted_configuration: bool,
}

#[derive(Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Completion {
    status: Status,
    source: String,
    qualification: String,
    hardware: String,
}

#[derive(Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Status {
    state: String,
    result: i32,
    done: bool,
    owned: u32,
}

impl State {
    /// Caller supplies separately collected completion evidence and expected
    /// hashes. Candidate self-reported success is never sufficient.
    pub fn import_fixture(
        candidate: &[u8],
        launcher_stdout: &[u8],
        completion: &[u8],
        expected: &FixtureExpected,
    ) -> Result<Self, Error> {
        expected.validate()?;
        verify_bytes(candidate, MAX_CANDIDATE_BYTES, &expected.candidate_sha256)?;
        verify_bytes(
            launcher_stdout,
            MAX_RECEIPT_BYTES,
            &expected.launcher_stdout_sha256,
        )?;
        verify_bytes(completion, MAX_RECEIPT_BYTES, &expected.completion_sha256)?;
        verify_launcher(launcher_stdout, expected.host_process_id)?;
        let receipt: Completion = serde_json::from_slice(completion)?;
        if receipt.status.state != "Ready"
            || receipt.status.result != 0
            || !receipt.status.done
            || receipt.status.owned != 0
            || receipt.qualification != "NOT_ESTABLISHED"
            || receipt.hardware != "NOT_RUN"
        {
            return Err(Error::UnconfirmedExit);
        }
        text_field(&receipt.source, "completion_source")?;
        let candidate: Candidate = serde_json::from_slice(candidate)?;
        let identity = &expected.identity;
        let source = &expected.source;
        if candidate.format_version != 1
            || candidate.kind != "ordinary-sdk-string"
            || candidate.host_final_validation != "PASS_BEFORE_EXIT"
            || !candidate.requires_confirmed_exit_zero
            || candidate.accepted_configuration
            || candidate.host_process_id != expected.host_process_id
            || candidate.project_id != source.project_id
            || candidate.installation_id != source.installation_id
            || candidate.device_id != source.device_id
            || candidate.application_id != identity.application_id
            || candidate.product_sha256 != identity.product_sha256
            || candidate.dca_sha256 != identity.dca_sha256
            || candidate.dca_version != identity.dca_version
            || candidate.sdk_sha256 != source.sdk_sha256
            || candidate.object_model_sha256 != source.object_model_sha256
            || candidate.host_sha256 != expected.host_sha256
            || candidate.parameter_unique_id != identity.parameter_unique_id
            || candidate.parameter_internal_name != identity.parameter_internal_name
            || digest(candidate.value.as_bytes()) != expected.expected_value_sha256
        {
            return Err(Error::CaptureMismatch);
        }
        if candidate.value.len() > 120 || candidate.value.encode_utf16().count() > 30 {
            return Err(Error::Limit);
        }
        let mut state = Self::capture(
            identity.clone(),
            expected.originating_revision.clone(),
            candidate.value,
        )?;
        state.content.fixture_source = Some(source.clone());
        state.content_sha256 = digest(&serde_json::to_vec(&state.content)?);
        state.validate()?;
        Ok(state)
    }

    /// Bounded projection for a separately pinned fresh Windows host. It carries
    /// the artifact hash, but does not authorize any device operation.
    pub fn fixture_restore_projection(&self, expected: &FixtureExpected) -> Result<Vec<u8>, Error> {
        expected.validate()?;
        self.validate()?;
        if self.identity() != &expected.identity
            || self.originating_revision() != expected.originating_revision
            || self.content.fixture_source.as_ref() != Some(&expected.source)
        {
            return Err(Error::IdentityMismatch);
        }
        if digest(self.value().as_bytes()) != expected.expected_value_sha256 {
            return Err(Error::CaptureMismatch);
        }
        #[derive(Serialize)]
        struct Projection<'a> {
            format_version: u32,
            kind: &'static str,
            fixture_uuid_revision_synthetic: bool,
            accepted_configuration: bool,
            state_artifact_sha256: String,
            device_uuid: &'a str,
            originating_revision: &'a str,
            project_id: &'a str,
            installation_id: &'a str,
            device_id: &'a str,
            application_id: &'a str,
            product_sha256: &'a str,
            dca_sha256: &'a str,
            dca_version: &'a str,
            sdk_sha256: &'a str,
            object_model_sha256: &'a str,
            parameter_unique_id: &'a str,
            parameter_internal_name: &'a str,
            value: &'a str,
        }
        let identity = self.identity();
        let source = &expected.source;
        let bytes = serde_json::to_vec(&Projection {
            format_version: 1,
            kind: "owned-offline-scalar-restore",
            fixture_uuid_revision_synthetic: true,
            accepted_configuration: false,
            state_artifact_sha256: digest(&self.canonical_bytes()?),
            device_uuid: &identity.device_uuid,
            originating_revision: self.originating_revision(),
            project_id: &source.project_id,
            installation_id: &source.installation_id,
            device_id: &source.device_id,
            application_id: &identity.application_id,
            product_sha256: &identity.product_sha256,
            dca_sha256: &identity.dca_sha256,
            dca_version: &identity.dca_version,
            sdk_sha256: &source.sdk_sha256,
            object_model_sha256: &source.object_model_sha256,
            parameter_unique_id: &identity.parameter_unique_id,
            parameter_internal_name: &identity.parameter_internal_name,
            value: self.value(),
        })?;
        if bytes.len() > MAX_CANDIDATE_BYTES {
            return Err(Error::Limit);
        }
        Ok(bytes)
    }
}

fn verify_bytes(bytes: &[u8], maximum: usize, expected: &str) -> Result<(), Error> {
    if bytes.len() > maximum {
        return Err(Error::Limit);
    }
    if digest(bytes) != expected {
        return Err(Error::IntegrityMismatch);
    }
    Ok(())
}

fn fields(line: &str) -> Result<BTreeMap<&str, &str>, Error> {
    let mut fields = BTreeMap::new();
    for token in line.split_ascii_whitespace().skip(1) {
        let (key, value) = token.split_once('=').ok_or(Error::UnconfirmedExit)?;
        if fields.len() >= 5 || fields.insert(key, value).is_some() {
            return Err(Error::UnconfirmedExit);
        }
    }
    if fields.len() != 5 {
        return Err(Error::UnconfirmedExit);
    }
    Ok(fields)
}

fn verify_launcher(bytes: &[u8], pid: u32) -> Result<(), Error> {
    let text = std::str::from_utf8(bytes).map_err(|_| Error::UnconfirmedExit)?;
    let mut start = None;
    let mut result = None;
    if text.lines().count() > 16 {
        return Err(Error::Limit);
    }
    for line in text.lines() {
        if line.len() > 1024 {
            return Err(Error::Limit);
        }
        if line.starts_with("LAUNCH_START ") {
            if start.replace(fields(line)?).is_some() {
                return Err(Error::UnconfirmedExit);
            }
        } else if line.starts_with("LAUNCH_RESULT ") && result.replace(fields(line)?).is_some() {
            return Err(Error::UnconfirmedExit);
        }
    }
    let start = start.ok_or(Error::UnconfirmedExit)?;
    let result = result.ok_or(Error::UnconfirmedExit)?;
    if start.get("pid").and_then(|value| value.parse::<u32>().ok()) != Some(pid)
        || start.get("mode") != Some(&"original-sdk-product-store-fixture")
        || start.get("deadline-seconds") != Some(&"660")
        || start.get("capture-bytes") != Some(&"65536")
        || start.get("descendants") != Some(&"NOT_CONTAINED")
        || result.get("reason") != Some(&"EXIT")
        || result.get("child-exit") != Some(&"0")
        || result.get("child-exit-confirmed") != Some(&"true")
        || result
            .get("captured-bytes")
            .and_then(|value| value.parse::<u32>().ok())
            .is_none_or(|value| value > 65536)
        || result.get("descendants") != Some(&"NOT_CONTAINED")
    {
        return Err(Error::UnconfirmedExit);
    }
    Ok(())
}

#[cfg(test)]
mod tests;
