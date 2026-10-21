use crate::{Error, MAX_ARCHIVE_BYTES, MAX_MEMBER_BYTES, MAX_MEMBERS, MAX_TOTAL_BYTES, checkpoint};
use std::collections::{BTreeMap, BTreeSet};
use std::io::{Cursor, Read};
use std::sync::Arc;
use std::sync::atomic::AtomicBool;

pub(crate) fn read(
    bytes: &[u8],
    cancelled: &AtomicBool,
) -> Result<BTreeMap<String, Arc<[u8]>>, Error> {
    if bytes.len() > MAX_ARCHIVE_BYTES {
        return Err(Error::Limit);
    }
    let mut archive = zip::ZipArchive::new(Cursor::new(bytes)).map_err(|_| Error::Archive)?;
    if archive.len() > MAX_MEMBERS {
        return Err(Error::Limit);
    }
    // The library's name index deduplicates central-directory names. Its
    // explicit validation also rejects hidden duplicates, divergent local
    // headers, and overlapping entries before our bounded member traversal.
    archive
        .validate_header_consistency()
        .map_err(|_| Error::Archive)?;
    let mut seen = BTreeSet::new();
    let mut members = BTreeMap::new();
    let mut total = 0;
    for index in 0..archive.len() {
        checkpoint(cancelled)?;
        let mut entry = archive.by_index(index).map_err(|_| Error::Archive)?;
        let name = std::str::from_utf8(entry.name_raw())
            .map_err(|_| Error::ArchivePath)?
            .to_owned();
        let canonical = name.strip_suffix('/').unwrap_or(&name);
        if name.len() > 240
            || canonical.is_empty()
            || !name.is_ascii()
            || name.contains(['\\', ':', '\0'])
            || canonical.split('/').any(|part| {
                part.is_empty() || part == "." || part == ".." || part.ends_with(['.', ' '])
            })
            || !name
                .bytes()
                .all(|b| b.is_ascii_alphanumeric() || b"/_-.".contains(&b))
            || entry.enclosed_name().is_none()
            || entry.is_symlink()
            || entry
                .unix_mode()
                .is_some_and(|mode| !matches!(mode & 0o170000, 0 | 0o100000 | 0o040000))
            || !seen.insert(canonical.to_ascii_lowercase())
        {
            return Err(Error::ArchivePath);
        }
        if entry.size() > MAX_MEMBER_BYTES as u64
            || entry.size() > entry.compressed_size().max(1).saturating_mul(64)
        {
            return Err(Error::Limit);
        }
        total += entry.size() as usize;
        if total > MAX_TOTAL_BYTES {
            return Err(Error::Limit);
        }
        let declared_size = entry.size() as usize;
        let directory = entry.is_dir();
        if directory && declared_size != 0 {
            return Err(Error::Archive);
        }
        let mut content = Vec::with_capacity(declared_size);
        let mut buffer = [0u8; 8192];
        loop {
            checkpoint(cancelled)?;
            let read = entry.read(&mut buffer).map_err(|_| Error::Archive)?;
            if read == 0 {
                break;
            }
            if content.len() + read > declared_size {
                return Err(Error::Limit);
            }
            content.extend_from_slice(&buffer[..read]);
        }
        // Reading to EOF validates the decoder's CRC, including ignored members.
        if content.len() != declared_size {
            return Err(Error::Archive);
        }
        if !directory {
            members.insert(name, Arc::from(content));
        }
    }
    // Prevent file/directory shadowing without ever creating a filesystem path.
    for name in members.keys() {
        let mut parent = name.as_str();
        while let Some((prefix, _)) = parent.rsplit_once('/') {
            if members
                .keys()
                .any(|candidate| candidate.eq_ignore_ascii_case(prefix))
            {
                return Err(Error::ArchivePath);
            }
            parent = prefix;
        }
    }
    Ok(members)
}
