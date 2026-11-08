// SPDX-License-Identifier: Apache-2.0
//! Fixed registry layout and metadata bounds, independent of native calls.

use crate::{Digest, Event, Key, MAX_RECORD_BYTES, StorageError};

pub(crate) const DATA: &str = "Data";
pub(crate) const CHUNK_BYTES: usize = 16;
pub(crate) const MAX_CHUNKS: usize = MAX_RECORD_BYTES / CHUNK_BYTES;
pub(crate) const MAX_CHUNK_NAME: usize = 10 + CHUNK_BYTES * 2;

pub(crate) fn hex(bytes: &[u8]) -> String {
    const DIGITS: &[u8; 16] = b"0123456789abcdef";
    let mut result = String::with_capacity(bytes.len() * 2);
    for byte in bytes {
        result.push(DIGITS[usize::from(byte >> 4)] as char);
        result.push(DIGITS[usize::from(byte & 15)] as char);
    }
    result
}

pub(crate) fn route_path(route: Digest) -> String {
    format!("Routes\\{}", hex(&route))
}

pub(crate) fn record_path(key: Key) -> String {
    match key {
        Key::Operation(id) => format!("Operations\\{}", hex(&id)),
        Key::Approval(id) => format!("Approvals\\{}", hex(&id)),
        Key::Event(id, event) => format!(
            "Events\\{}\\{}",
            hex(&id),
            match event {
                Event::Intent => "intent",
                Event::DesiredIntent => "desired-intent",
                Event::DesiredReadback => "desired-readback",
                Event::RestoreIntent => "restore-intent",
                Event::RestoredReadback => "restored-readback",
                Event::CleanupConfirmed => "cleanup-confirmed",
            }
        ),
        Key::GuardOperation(id) => format!("GO\\{}", hex(&id)),
        Key::GuardApproval(id) => format!("GA\\{}", hex(&id)),
        Key::GuardIntent(id) => format!("GI\\{}", hex(&id)),
        Key::GuardIntentPart(id, index) => format!("GP\\{}\\{index:02x}", hex(&id)),
        Key::GuardEvidence(id, evidence) => format!("GE\\{}\\{}", hex(&id), evidence.key()),
    }
}

pub(crate) fn encode_chunks(bytes: &[u8]) -> Result<Vec<String>, StorageError> {
    if bytes.is_empty() || bytes.len() > MAX_RECORD_BYTES {
        return Err(StorageError::Unavailable);
    }
    Ok(bytes
        .chunks(CHUNK_BYTES)
        .enumerate()
        .map(|(index, chunk)| format!("{index:04x}_{:04x}_{}", bytes.len(), hex(chunk)))
        .collect())
}

fn number(text: &str) -> Result<usize, StorageError> {
    if text.len() != 4
        || !text
            .bytes()
            .all(|b| b.is_ascii_digit() || (b'a'..=b'f').contains(&b))
    {
        return Err(StorageError::Unavailable);
    }
    usize::from_str_radix(text, 16).map_err(|_| StorageError::Unavailable)
}

pub(crate) fn decode_chunks(
    names: impl Iterator<Item = Result<String, StorageError>>,
    limit: usize,
) -> Result<Vec<u8>, StorageError> {
    let mut chunks: [Option<Vec<u8>>; MAX_CHUNKS] = std::array::from_fn(|_| None);
    let mut declared = None;
    let mut seen = 0;
    for name in names.take(MAX_CHUNKS + 1) {
        let name = name?;
        seen += 1;
        if seen > MAX_CHUNKS || name.len() > MAX_CHUNK_NAME || !name.is_ascii() {
            return Err(StorageError::Unavailable);
        }
        let mut fields = name.split('_');
        let index = number(fields.next().ok_or(StorageError::Unavailable)?)?;
        let total = number(fields.next().ok_or(StorageError::Unavailable)?)?;
        let data = fields.next().ok_or(StorageError::Unavailable)?;
        if fields.next().is_some()
            || total == 0
            || total > limit.min(MAX_RECORD_BYTES)
            || declared.is_some_and(|expected| expected != total)
            || index >= total.div_ceil(CHUNK_BYTES)
            || chunks[index].is_some()
        {
            return Err(StorageError::Unavailable);
        }
        declared = Some(total);
        let expected = (total - index * CHUNK_BYTES).min(CHUNK_BYTES);
        if data.len() != expected * 2
            || !data
                .bytes()
                .all(|b| b.is_ascii_digit() || (b'a'..=b'f').contains(&b))
        {
            return Err(StorageError::Unavailable);
        }
        chunks[index] = Some(
            data.as_bytes()
                .as_chunks::<2>()
                .0
                .iter()
                .map(|pair| {
                    let digit = |b: u8| {
                        if b.is_ascii_digit() {
                            b - b'0'
                        } else {
                            b - b'a' + 10
                        }
                    };
                    (digit(pair[0]) << 4) | digit(pair[1])
                })
                .collect(),
        );
    }
    let total = declared.ok_or(StorageError::Unavailable)?;
    if seen != total.div_ceil(CHUNK_BYTES) {
        return Err(StorageError::Unavailable);
    }
    let mut bytes = Vec::with_capacity(total);
    for chunk in &mut chunks[..seen] {
        bytes.extend(chunk.take().ok_or(StorageError::Unavailable)?);
    }
    Ok(bytes)
}

pub(crate) fn validate_data_shape(
    subkeys: u32,
    values: u32,
    max_name: u32,
) -> Result<(), StorageError> {
    if subkeys == 0
        || subkeys as usize > MAX_CHUNKS
        || values != 0
        || max_name as usize > MAX_CHUNK_NAME
    {
        return Err(StorageError::Unavailable);
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn paths_have_fixed_names_and_exact_hex_fields() {
        assert_eq!(DATA, "Data");
        assert_eq!(
            route_path([0xab; 32]),
            format!("Routes\\{}", "ab".repeat(32))
        );
        assert_eq!(
            record_path(Key::Approval([0xcd; 16])),
            format!("Approvals\\{}", "cd".repeat(16))
        );
        for event in crate::EVENTS {
            let path = record_path(Key::Event([1; 32], event));
            assert!(path.starts_with(&format!("Events\\{}\\", "01".repeat(32))));
            assert!(path.len() < 120);
        }
        assert_ne!(
            record_path(Key::Operation([1; 32])),
            record_path(Key::Event([1; 32], Event::Intent))
        );
        let mut guard_keys = vec![
            Key::GuardOperation([1; 32]),
            Key::GuardApproval([2; 16]),
            Key::GuardIntent([1; 32]),
        ];
        for index in 0..=8 {
            guard_keys.push(Key::GuardIntentPart([1; 32], index));
        }
        for evidence in [
            crate::guard_storage::Evidence::DesiredReadback,
            crate::guard_storage::Evidence::RestoreIntent,
            crate::guard_storage::Evidence::RestoredReadback,
            crate::guard_storage::Evidence::CleanupConfirmed,
        ] {
            guard_keys.push(Key::GuardEvidence([1; 32], evidence));
        }
        let mut paths = std::collections::HashSet::new();
        for key in guard_keys {
            let path = record_path(key);
            assert!(paths.insert(path.clone()), "guard keys must be distinct");
            assert!(!path.starts_with("Operations\\") && !path.starts_with("Events\\"));
            // Every new guard key path is shorter than the pre-existing longest
            // path already verified against Windows' full registry-key bound.
            assert!(path.len() <= record_path(Key::Event([1; 32], Event::CleanupConfirmed)).len());
        }
    }

    #[test]
    fn chunk_roundtrip_and_registry_full_path_are_bounded() {
        for size in [1, 16, 17, 2048] {
            let bytes = vec![0xa5; size];
            let mut names = encode_chunks(&bytes).unwrap();
            names.reverse();
            assert_eq!(
                decode_chunks(names.into_iter().map(Ok), MAX_RECORD_BYTES),
                Ok(bytes)
            );
        }
        let longest = format!(
            "HKEY_CURRENT_USER\\Software\\OpenKNXStudio\\J156KeysV1\\{}\\{}\\Data\\{}",
            "a".repeat(64),
            record_path(Key::Event([1; 32], Event::CleanupConfirmed)),
            encode_chunks(&[0; 16]).unwrap()[0]
        );
        assert!(longest.len() <= 255);
        assert!(encode_chunks(&vec![0; 2049]).is_err());
    }

    #[test]
    fn missing_duplicate_malformed_truncated_or_oversized_chunks_fail_closed() {
        let full = encode_chunks(&[0xab; 33]).unwrap();
        let mut missing = full.clone();
        missing.pop();
        let mut duplicate = full.clone();
        duplicate[1] = duplicate[0].clone();
        let mut truncated = full.clone();
        truncated[2].pop();
        for names in [
            missing,
            duplicate,
            truncated,
            vec!["0000_0801_ab".into()],
            vec!["0000_0001_zz".into()],
            vec!["0000_0001_ab_extra".into()],
        ] {
            assert_eq!(
                decode_chunks(names.into_iter().map(Ok), MAX_RECORD_BYTES),
                Err(StorageError::Unavailable)
            );
        }
        assert_eq!(
            decode_chunks(full.into_iter().map(Ok), 32),
            Err(StorageError::Unavailable)
        );
        for tuple in [(0, 0, 42), (129, 0, 42), (1, 1, 42), (1, 0, 256)] {
            assert_eq!(
                validate_data_shape(tuple.0, tuple.1, tuple.2),
                Err(StorageError::Unavailable)
            );
        }
    }

    #[test]
    fn even_an_unbounded_concurrent_name_source_has_a_fixed_read_budget() {
        let calls = std::cell::Cell::new(0);
        let names = std::iter::from_fn(|| {
            let index = calls.get();
            calls.set(index + 1);
            Some(Ok(format!("{index:04x}_0800_{}", "ab".repeat(16))))
        });
        assert!(decode_chunks(names, MAX_RECORD_BYTES).is_err());
        assert_eq!(calls.get(), MAX_CHUNKS + 1);
    }

    #[test]
    fn every_torn_chunk_replacement_is_rejected_until_the_new_record_is_complete() {
        let old = encode_chunks(&[1; 65]).unwrap();
        let new = encode_chunks(&[2; 81]).unwrap();
        for deleted in 1..=old.len() {
            assert!(
                decode_chunks(old[deleted..].iter().cloned().map(Ok), MAX_RECORD_BYTES).is_err()
            );
        }
        for written in 0..new.len() {
            assert!(
                decode_chunks(new[..written].iter().cloned().map(Ok), MAX_RECORD_BYTES).is_err()
            );
        }
        assert_eq!(
            decode_chunks(new.into_iter().map(Ok), MAX_RECORD_BYTES),
            Ok(vec![2; 81])
        );
    }
}
