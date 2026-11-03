//! Bounded read-only inspection of the selected MDT Knxapp profile.
//! No archive content is extracted, loaded as code, or resolved over a network.
#![forbid(unsafe_code)]

use quick_xml::{Reader, events::Event};
use sha2::{Digest, Sha256};
use std::{
    collections::{BTreeMap, BTreeSet},
    fmt,
    fs::File,
    io::{Cursor, Read},
    path::Path,
    sync::atomic::{AtomicBool, Ordering},
};
use zip::{
    CompressionMethod, ZipArchive,
    read::{ArchiveOffset, Config},
};

pub const MAX_ARCHIVE: usize = 8 * 1024 * 1024;
pub const MAX_ENTRIES: usize = 256;
pub const MAX_FILE: u64 = 4 * 1024 * 1024;
pub const MAX_EXPANDED: u64 = 16 * 1024 * 1024;
pub const MAX_XML: usize = 16 * 1024;
const MAX_RATIO: u64 = 200;
const MAX_DEPTH: usize = 8;
const MAX_EVENTS: usize = 512;
const TOKENS: &[&str] = &[
    ".NETFramework,Version=v4.6.1",
    "Knx.Ets.Sdk",
    "PresentationFramework",
    "WindowsBase",
    "IDeviceConfigurationAddIn",
    "Knx.Falcon",
    "Knx.Ets.Xml.ObjectModel",
    "Knx.Bus.Common",
    "Knx.Ets.DynamicData",
    "Knx.Ets.Converter.ConverterEngine",
];

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Error {
    Io,
    Archive,
    ArchiveLimit,
    EntryLimit,
    UnsafePath,
    Collision,
    SpecialFile,
    Encryption,
    Compression,
    FileLimit,
    ExpandedLimit,
    RatioLimit,
    Integrity,
    Manifest,
    XmlLimit,
    UnsupportedProfile,
    MissingFile,
    Cancelled,
}
impl Error {
    pub fn key(self) -> &'static str {
        match self {
            Self::Io => "dca_inspect.io",
            Self::Archive => "dca_inspect.archive",
            Self::ArchiveLimit => "dca_inspect.archive_limit",
            Self::EntryLimit => "dca_inspect.entry_limit",
            Self::UnsafePath => "dca_inspect.unsafe_path",
            Self::Collision => "dca_inspect.collision",
            Self::SpecialFile => "dca_inspect.special_file",
            Self::Encryption => "dca_inspect.encryption",
            Self::Compression => "dca_inspect.compression",
            Self::FileLimit => "dca_inspect.file_limit",
            Self::ExpandedLimit => "dca_inspect.expanded_limit",
            Self::RatioLimit => "dca_inspect.ratio_limit",
            Self::Integrity => "dca_inspect.integrity",
            Self::Manifest => "dca_inspect.manifest",
            Self::XmlLimit => "dca_inspect.xml_limit",
            Self::UnsupportedProfile => "dca_inspect.unsupported_profile",
            Self::MissingFile => "dca_inspect.missing_file",
            Self::Cancelled => "dca_inspect.cancelled",
        }
    }
    pub fn english(self) -> &'static str {
        match self {
            Self::Io => "Cannot read the local input file.",
            Self::Archive => "Unsupported or malformed ZIP32 archive.",
            Self::ArchiveLimit => "Archive exceeds the input size limit.",
            Self::EntryLimit => "Archive exceeds the entry count limit.",
            Self::UnsafePath => "Archive contains an unsafe or unsupported path.",
            Self::Collision => "Archive paths are ambiguous or duplicated.",
            Self::SpecialFile => "Archive contains a special file or inconsistent directory.",
            Self::Encryption => "Encrypted entries are unsupported.",
            Self::Compression => "Compression method is unsupported.",
            Self::FileLimit => "Entry exceeds the expanded size limit.",
            Self::ExpandedLimit => "Archive exceeds the aggregate expanded size limit.",
            Self::RatioLimit => "Entry exceeds the expansion ratio limit.",
            Self::Integrity => "Archive headers, data sizes or CRC are inconsistent.",
            Self::Manifest => "Manifest is malformed, duplicated or incomplete.",
            Self::XmlLimit => "Manifest exceeds an XML parsing limit.",
            Self::UnsupportedProfile => "Manifest is outside the selected MDT Knxapp profile.",
            Self::MissingFile => "A required regular file is missing.",
            Self::Cancelled => "Inspection was cancelled.",
        }
    }
}
impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}: {}", self.key(), self.english())
    }
}
impl std::error::Error for Error {}
fn cancelled(flag: &AtomicBool) -> Result<(), Error> {
    if flag.load(Ordering::Relaxed) {
        Err(Error::Cancelled)
    } else {
        Ok(())
    }
}
fn hex(bytes: &[u8]) -> String {
    bytes.iter().map(|b| format!("{b:02x}")).collect()
}
pub fn sha256(bytes: &[u8]) -> String {
    hex(&Sha256::digest(bytes))
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Manifest {
    pub app_id: String,
    pub assembly: String,
    pub is_64_bit_compatible: bool,
    pub single_instance: bool,
    pub applications: Vec<String>,
}
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Entry {
    pub name: String,
    pub directory: bool,
    pub bytes: u64,
    pub sha256: String,
}
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Report {
    pub archive_sha256: String,
    pub archive_bytes: usize,
    pub files: usize,
    pub directories: usize,
    pub expanded_bytes: u64,
    pub manifest: Manifest,
    pub actual_assembly: String,
    pub assembly_case_difference: bool,
    pub entries: Vec<Entry>,
    /// Predefined literal byte tokens only; not CLR metadata or runtime verification.
    pub primary_byte_tokens: Vec<String>,
    pub config_byte_tokens: Vec<String>,
}
impl fmt::Display for Report {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        writeln!(f, "schema=dca-inspect-v1")?;
        writeln!(
            f,
            "observation=static_only; CLR_metadata=NOT_RUN; runtime=NOT_RUN"
        )?;
        writeln!(
            f,
            "external_metadata_document=docs/DCA-CONTRACT-MAP.md; scope=documented_hashes_only"
        )?;
        writeln!(
            f,
            "archive_sha256={} archive_bytes={}",
            self.archive_sha256, self.archive_bytes
        )?;
        writeln!(
            f,
            "entries={} files={} directories={} expanded_bytes={}",
            self.entries.len(),
            self.files,
            self.directories,
            self.expanded_bytes
        )?;
        writeln!(
            f,
            "app_id={:?} declared_64_bit_compatible={} single_instance={}",
            self.manifest.app_id, self.manifest.is_64_bit_compatible, self.manifest.single_instance
        )?;
        writeln!(
            f,
            "manifest_assembly={:?} actual_assembly={:?} case_difference={}",
            self.manifest.assembly, self.actual_assembly, self.assembly_case_difference
        )?;
        for id in &self.manifest.applications {
            writeln!(f, "application_id={id:?}")?;
        }
        writeln!(
            f,
            "primary_literal_byte_tokens={:?}",
            self.primary_byte_tokens
        )?;
        writeln!(
            f,
            "config_literal_byte_tokens={:?}",
            self.config_byte_tokens
        )?;
        for e in &self.entries {
            writeln!(
                f,
                "{} name={:?} bytes={} sha256={}",
                if e.directory {
                    "directory"
                } else if e.name.to_ascii_lowercase().ends_with(".dll") {
                    "assembly_filename_only"
                } else {
                    "file"
                },
                e.name,
                e.bytes,
                e.sha256
            )?;
        }
        Ok(())
    }
}

pub fn inspect_path(path: &Path, cancel: &AtomicBool) -> Result<Report, Error> {
    cancelled(cancel)?;
    if !std::fs::symlink_metadata(path)
        .map_err(|_| Error::Io)?
        .is_file()
    {
        return Err(Error::Io);
    }
    let file = File::open(path).map_err(|_| Error::Io)?;
    let metadata = file.metadata().map_err(|_| Error::Io)?;
    if !metadata.is_file() {
        return Err(Error::Io);
    }
    if metadata.len() > MAX_ARCHIVE as u64 {
        return Err(Error::ArchiveLimit);
    }
    let mut bytes = Vec::new();
    file.take(MAX_ARCHIVE as u64 + 1)
        .read_to_end(&mut bytes)
        .map_err(|_| Error::Io)?;
    inspect(&bytes, cancel)
}

// Restrict the container to ordinary single-disk ZIP32 with a final uncommented
// EOCD before the library allocates central-directory metadata. ZIP64, SFX,
// trailing data, extra directory records and comments are outside this profile.
fn zip_preflight(bytes: &[u8]) -> Result<(usize, usize), Error> {
    if bytes.len() > MAX_ARCHIVE {
        return Err(Error::ArchiveLimit);
    }
    let end = bytes.len().checked_sub(22).ok_or(Error::Archive)?;
    let e = &bytes[end..];
    if &e[..4] != b"PK\x05\x06" || e[4..8] != [0; 4] || e[20..22] != [0; 2] {
        return Err(Error::Archive);
    }
    let count = u16::from_le_bytes([e[10], e[11]]) as usize;
    if e[8..10] != e[10..12] || count == 0 {
        return Err(Error::Archive);
    }
    if count > MAX_ENTRIES {
        return Err(Error::EntryLimit);
    }
    let size = u32::from_le_bytes([e[12], e[13], e[14], e[15]]) as usize;
    let start = u32::from_le_bytes([e[16], e[17], e[18], e[19]]) as usize;
    if size > 128 * 1024
        || start.checked_add(size) != Some(end)
        || !bytes.starts_with(b"PK\x03\x04")
    {
        return Err(Error::Archive);
    }
    // Count and traverse exactly the advertised records, rejecting hidden records
    // before handing the bounded container to zip's consistency/CRC checks.
    let mut pos = start;
    for _ in 0..count {
        let h = bytes
            .get(pos..pos.saturating_add(46))
            .ok_or(Error::Archive)?;
        if &h[..4] != b"PK\x01\x02" {
            return Err(Error::Archive);
        }
        let n = u16::from_le_bytes([h[28], h[29]]) as usize;
        let x = u16::from_le_bytes([h[30], h[31]]) as usize;
        let c = u16::from_le_bytes([h[32], h[33]]) as usize;
        if h[8] & 1 != 0 {
            return Err(Error::Encryption);
        }
        // Only the selected archive's flags=0 and UTF-8 names are supported.
        if u16::from_le_bytes([h[8], h[9]]) & !0x0800 != 0 || h[34..36] != [0, 0] {
            return Err(Error::Archive);
        }
        if n > 240 || x > 1024 || c != 0 {
            return Err(Error::Archive);
        }
        let extra_start = pos.checked_add(46 + n).ok_or(Error::Archive)?;
        check_extra(
            bytes
                .get(extra_start..extra_start + x)
                .ok_or(Error::Archive)?,
        )?;
        let local = u32::from_le_bytes([h[42], h[43], h[44], h[45]]) as usize;
        let lh = bytes
            .get(local..local.saturating_add(30))
            .ok_or(Error::Archive)?;
        if &lh[..4] != b"PK\x03\x04" {
            return Err(Error::Archive);
        }
        let ln = u16::from_le_bytes([lh[26], lh[27]]) as usize;
        let lx = u16::from_le_bytes([lh[28], lh[29]]) as usize;
        let ls = local.checked_add(30 + ln).ok_or(Error::Archive)?;
        if ln > 240 || lx > 1024 || ls.saturating_add(lx) > start {
            return Err(Error::Archive);
        }
        check_extra(bytes.get(ls..ls + lx).ok_or(Error::Archive)?)?;
        pos = pos.checked_add(46 + n + x + c).ok_or(Error::Archive)?;
        if pos > end {
            return Err(Error::Archive);
        }
    }
    if pos != end {
        return Err(Error::Archive);
    }
    Ok((count, start))
}

// NTFS timestamps are present in the selected package. They are ignored, never
// used to alter paths or sizes. ZIP64 and Unicode-name overrides are unsupported.
fn check_extra(mut bytes: &[u8]) -> Result<(), Error> {
    while !bytes.is_empty() {
        let h = bytes.get(..4).ok_or(Error::Archive)?;
        if h[..2] != [0x0a, 0] {
            return Err(Error::Archive);
        }
        let size = u16::from_le_bytes([h[2], h[3]]) as usize;
        bytes = bytes.get(4 + size..).ok_or(Error::Archive)?;
    }
    Ok(())
}

fn safe_name(name: &str) -> Result<String, Error> {
    if name.is_empty() || name.len() > 240 || !name.is_ascii() || name.contains('\\') {
        return Err(Error::UnsafePath);
    }
    let trimmed = name.strip_suffix('/').unwrap_or(name);
    for part in trimmed.split('/') {
        if part.is_empty()
            || part == "."
            || part == ".."
            || part.ends_with([' ', '.'])
            || part
                .bytes()
                .any(|b| b < 32 || b == 127 || b"<>:\"|?*".contains(&b))
        {
            return Err(Error::UnsafePath);
        }
        let stem = part
            .split('.')
            .next()
            .ok_or(Error::UnsafePath)?
            .to_ascii_uppercase();
        if matches!(stem.as_str(), "CON" | "PRN" | "AUX" | "NUL")
            || (stem.len() == 4
                && (stem.starts_with("COM") || stem.starts_with("LPT"))
                && matches!(stem.as_bytes()[3], b'1'..=b'9'))
        {
            return Err(Error::UnsafePath);
        }
    }
    Ok(trimmed.to_ascii_lowercase())
}

pub fn inspect(bytes: &[u8], cancel: &AtomicBool) -> Result<Report, Error> {
    cancelled(cancel)?;
    let (count, central_start) = zip_preflight(bytes)?;
    let mut archive = ZipArchive::with_config(
        Config {
            archive_offset: ArchiveOffset::Known(0),
        },
        Cursor::new(bytes),
    )
    .map_err(|_| Error::Archive)?;
    archive
        .validate_header_consistency()
        .map_err(|_| Error::Integrity)?;
    if archive.len() != count || archive.central_directory_start() != central_start as u64 {
        return Err(Error::Integrity);
    }
    let mut names = BTreeMap::new();
    let mut expanded = 0;
    // Validate every entry before decompressing even the manifest.
    for index in 0..count {
        cancelled(cancel)?;
        let file = archive.by_index_data(index).map_err(|_| Error::Archive)?;
        let name = std::str::from_utf8(file.name_raw()).map_err(|_| Error::UnsafePath)?;
        let key = safe_name(name)?;
        if names.insert(key, (index, file.is_dir())).is_some() {
            return Err(Error::Collision);
        }
        if file.encrypted() {
            return Err(Error::Encryption);
        }
        if !matches!(
            file.compression(),
            CompressionMethod::Stored | CompressionMethod::Deflated
        ) {
            return Err(Error::Compression);
        }
        let kind = file.unix_mode().unwrap_or(0) & 0o170000;
        if (file.is_dir() && !matches!(kind, 0 | 0o040000))
            || (!file.is_dir() && !matches!(kind, 0 | 0o100000))
            || (file.is_dir() && file.size() != 0)
        {
            return Err(Error::SpecialFile);
        }
        if file.size() > MAX_FILE {
            return Err(Error::FileLimit);
        }
        expanded += file.size();
        if expanded > MAX_EXPANDED {
            return Err(Error::ExpandedLimit);
        }
        if file.size() > file.compressed_size().max(1).saturating_mul(MAX_RATIO) {
            return Err(Error::RatioLimit);
        }
    }
    // Detect a file used as a parent and inconsistent casing of path components.
    let mut components = BTreeMap::new();
    for index in 0..count {
        let file = archive.by_index_data(index).map_err(|_| Error::Archive)?;
        let name = std::str::from_utf8(file.name_raw()).map_err(|_| Error::UnsafePath)?;
        let mut prefix = String::new();
        let parts: Vec<_> = name.trim_end_matches('/').split('/').collect();
        for (i, part) in parts.iter().enumerate() {
            if i > 0 {
                prefix.push('/');
            }
            prefix.push_str(part);
            let key = prefix.to_ascii_lowercase();
            if components
                .insert(key.clone(), prefix.clone())
                .is_some_and(|old| old != prefix)
                || (i + 1 < parts.len() && names.get(&key).is_some_and(|(_, dir)| !dir))
            {
                return Err(Error::Collision);
            }
        }
    }
    let required = |key: &str| -> Result<usize, Error> {
        names
            .get(key)
            .filter(|(_, dir)| !dir)
            .map(|(i, _)| *i)
            .ok_or(Error::MissingFile)
    };
    let manifest_index = required("addinmanifest.xml")?;
    let mut entries = Vec::new();
    let mut selected = BTreeMap::new();
    // Read every member to EOF so CRC errors in unrelated resources also fail.
    for index in 0..count {
        cancelled(cancel)?;
        let mut file = archive.by_index(index).map_err(|_| Error::Archive)?;
        let name = std::str::from_utf8(file.name_raw())
            .map_err(|_| Error::UnsafePath)?
            .to_owned();
        let mut hash = Sha256::new();
        let mut data = Vec::new();
        let retain = index == manifest_index
            || name.eq_ignore_ascii_case("MDT-DaliDcaPlus.dll")
            || name.eq_ignore_ascii_case("MDT-DaliDcaPlus.dll.config");
        if index == manifest_index && file.size() > MAX_XML as u64 {
            return Err(Error::XmlLimit);
        }
        let mut actual = 0;
        let mut chunk = [0; 8192];
        loop {
            cancelled(cancel)?;
            let n = file.read(&mut chunk).map_err(|_| Error::Integrity)?;
            if n == 0 {
                break;
            }
            actual += n as u64;
            if actual > file.size() || actual > MAX_FILE {
                return Err(Error::FileLimit);
            }
            hash.update(&chunk[..n]);
            if retain {
                data.extend_from_slice(&chunk[..n]);
            }
        }
        if actual != file.size() {
            return Err(Error::Integrity);
        }
        entries.push(Entry {
            name,
            directory: file.is_dir(),
            bytes: actual,
            sha256: hex(&hash.finalize()),
        });
        if retain {
            selected.insert(index, data);
        }
    }
    let manifest = parse_manifest(
        selected.get(&manifest_index).ok_or(Error::MissingFile)?,
        cancel,
    )?;
    let primary_index = required(&manifest.assembly.to_ascii_lowercase())?;
    let config_index = required("mdt-dalidcaplus.dll.config")?;
    let actual_assembly = entries[primary_index].name.clone();
    let primary_byte_tokens = tokens(selected.get(&primary_index).ok_or(Error::MissingFile)?);
    let config_byte_tokens = tokens(selected.get(&config_index).ok_or(Error::MissingFile)?);
    let directories = entries.iter().filter(|e| e.directory).count();
    entries.sort_by(|a, b| a.name.cmp(&b.name));
    Ok(Report {
        archive_sha256: sha256(bytes),
        archive_bytes: bytes.len(),
        files: count - directories,
        directories,
        expanded_bytes: expanded,
        assembly_case_difference: manifest.assembly != actual_assembly,
        manifest,
        actual_assembly,
        entries,
        primary_byte_tokens,
        config_byte_tokens,
    })
}
fn tokens(bytes: &[u8]) -> Vec<String> {
    TOKENS
        .iter()
        .filter(|token| bytes.windows(token.len()).any(|w| w == token.as_bytes()))
        .map(|s| (*s).to_owned())
        .collect()
}

/// Exact observed Knxapp shape; new elements/attributes require a separate profile review.
pub fn parse_manifest(bytes: &[u8], cancel: &AtomicBool) -> Result<Manifest, Error> {
    if bytes.len() > MAX_XML {
        return Err(Error::XmlLimit);
    }
    let bytes = bytes.strip_prefix(b"\xef\xbb\xbf").unwrap_or(bytes);
    if !bytes.starts_with(b"<?xml ") {
        return Err(Error::Manifest);
    }
    // The selected manifest is ASCII UTF-8 after its optional UTF-8 BOM. Entities, DTDs, non-ASCII encodings
    // and non-whitespace control bytes are deliberately unsupported.
    if !bytes.is_ascii()
        || bytes.contains(&b'&')
        || bytes.iter().any(|b| *b < 32 && !b"\t\r\n".contains(b))
    {
        return Err(Error::Manifest);
    }
    let mut reader = Reader::from_reader(bytes);
    reader.config_mut().expand_empty_elements = true;
    let mut stack: Vec<String> = Vec::new();
    let mut seen = BTreeMap::<String, usize>::new();
    let mut applications = Vec::new();
    let mut ids = BTreeSet::new();
    let mut manifest = None;
    let mut config = None;
    let mut declarations = 0;
    for _ in 0..MAX_EVENTS {
        cancelled(cancel)?;
        match reader.read_event().map_err(|_| Error::Manifest)? {
            Event::Start(e) => {
                let name = e.name().as_ref().to_owned();
                let parent = stack.last().map(String::as_str).unwrap_or("");
                let allowed: &[&str] = match (parent, name.as_str()) {
                    ("", "Knxapp") => &["AppId", "Icon", "AddInAssembly"],
                    ("Knxapp", "Configuration") => &[
                        "Is64BitCompatible",
                        "IsSingleInstanceAddIn",
                        "AddToContentSwitcher",
                        "AddToMainToolbar",
                        "HasConfigurationDialog",
                    ],
                    (
                        "Knxapp",
                        "DisplayNames"
                        | "AddInToolbarButtons"
                        | "HelpFiles"
                        | "DeviceConfiguration",
                    ) => &[],
                    ("DisplayNames", "DisplayName") => &["LanguageId", "Name"],
                    ("HelpFiles", "HelpFile") => &["LanguageId", "Path", "Name"],
                    ("DeviceConfiguration", "ApplicationProgram") => &["Id"],
                    _ => return Err(Error::UnsupportedProfile),
                };
                let path = format!("{}/{}", stack.join("/"), name);
                let count = seen.entry(path).or_default();
                *count += 1;
                if *count > 1 && !matches!(name.as_str(), "ApplicationProgram" | "HelpFile") {
                    return Err(Error::Manifest);
                }
                let mut attrs = BTreeMap::new();
                for attr in e.attributes() {
                    let attr = attr.map_err(|_| Error::Manifest)?;
                    let key = attr.key.as_ref().to_owned();
                    let value = attr.value.into_owned();
                    if !allowed.contains(&key.as_str()) {
                        return Err(Error::UnsupportedProfile);
                    }
                    if value.len() > 240
                        || value.bytes().any(|b| b < 32 || b == 127 || b == b'<')
                        || attrs.insert(key, value).is_some()
                    {
                        return Err(Error::Manifest);
                    }
                }
                if attrs.len() != allowed.len() {
                    return Err(Error::Manifest);
                }
                match name.as_str() {
                    "Knxapp" => {
                        if attrs["AppId"] != "M0083-A0158"
                            || attrs["AddInAssembly"] != "Mdt-DaliDCAPlus.dll"
                        {
                            return Err(Error::UnsupportedProfile);
                        }
                        manifest = Some((attrs["AppId"].clone(), attrs["AddInAssembly"].clone()));
                    }
                    "Configuration" => {
                        if attrs["AddToContentSwitcher"] != "false"
                            || attrs["AddToMainToolbar"] != "false"
                            || attrs["HasConfigurationDialog"] != "false"
                        {
                            return Err(Error::UnsupportedProfile);
                        }
                        config = Some((
                            boolean(&attrs["Is64BitCompatible"])?,
                            boolean(&attrs["IsSingleInstanceAddIn"])?,
                        ));
                    }
                    "ApplicationProgram" => {
                        let id = &attrs["Id"];
                        if id.is_empty()
                            || !id.bytes().all(|b| {
                                b.is_ascii_uppercase()
                                    || b.is_ascii_digit()
                                    || b == b'-'
                                    || b == b'_'
                            })
                            || !ids.insert(id.clone())
                        {
                            return Err(Error::Manifest);
                        }
                        applications.push(id.clone());
                        if applications.len() > 6 {
                            return Err(Error::UnsupportedProfile);
                        }
                    }
                    _ => {}
                }
                stack.push(name);
                if stack.len() > MAX_DEPTH {
                    return Err(Error::XmlLimit);
                }
            }
            Event::End(e) => {
                if stack.pop().as_deref() != Some(e.name().as_ref()) {
                    return Err(Error::Manifest);
                }
            }
            Event::Text(e) if e.as_ref().bytes().all(|b| b.is_ascii_whitespace()) => {}
            Event::Decl(e) => {
                let mut count = 0;
                for attr in quick_xml::events::BytesStart::from_content(e.as_ref(), 3).attributes()
                {
                    let attr = attr.map_err(|_| Error::Manifest)?;
                    if !matches!(attr.key.as_ref(), "version" | "encoding") {
                        return Err(Error::Manifest);
                    }
                    count += 1;
                }
                if count != 2 {
                    return Err(Error::Manifest);
                }
                declarations += 1;
                if declarations != 1
                    || !seen.is_empty()
                    || e.version().map_err(|_| Error::Manifest)?.as_ref() != "1.0"
                    || e.encoding()
                        .transpose()
                        .map_err(|_| Error::Manifest)?
                        .is_some_and(|v| !v.eq_ignore_ascii_case("utf-8"))
                {
                    return Err(Error::Manifest);
                }
            }
            Event::Eof => {
                if !stack.is_empty() || applications.len() != 6 {
                    return Err(Error::Manifest);
                }
                for path in [
                    "/Knxapp",
                    "Knxapp/Configuration",
                    "Knxapp/DisplayNames",
                    "Knxapp/DisplayNames/DisplayName",
                    "Knxapp/AddInToolbarButtons",
                    "Knxapp/HelpFiles",
                    "Knxapp/DeviceConfiguration",
                ] {
                    if seen.get(path) != Some(&1) {
                        return Err(Error::Manifest);
                    }
                }
                let (app_id, assembly) = manifest.ok_or(Error::Manifest)?;
                let (is_64_bit_compatible, single_instance) = config.ok_or(Error::Manifest)?;
                return Ok(Manifest {
                    app_id,
                    assembly,
                    is_64_bit_compatible,
                    single_instance,
                    applications,
                });
            }
            _ => return Err(Error::Manifest),
        }
    }
    Err(Error::XmlLimit)
}
fn boolean(s: &str) -> Result<bool, Error> {
    match s {
        "true" => Ok(true),
        "false" => Ok(false),
        _ => Err(Error::Manifest),
    }
}
