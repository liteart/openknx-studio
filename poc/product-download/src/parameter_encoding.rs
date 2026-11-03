//! Staged single-byte encoding, with no qualified original-product route yet.
use crate::{GLASS_APPLICATION_ID, GLASS_REGISTERED_SHA256, Snapshot, checkpoint, sha256};
use std::ops::Range;
use std::sync::Arc;
use std::sync::atomic::AtomicBool;

const MAX_SEGMENT_BYTES: usize = 1_968;
const MAX_OCCUPIED_RANGES: usize = 539;
const MAX_VALUE: u32 = 60;

#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum ParameterEncodingError {
    Cancelled,
    Limit,
    ValueOutOfRange,
    LayoutEvidenceMissing,
    OutOfBounds,
    ArithmeticOverflow,
    Overlap,
    UnsupportedReference,
    UnverifiedReferenceContext,
}
impl ParameterEncodingError {
    pub fn key(self) -> &'static str {
        match self {
            Self::Cancelled => "parameter_encoding.cancelled",
            Self::Limit => "parameter_encoding.limit",
            Self::ValueOutOfRange => "parameter_encoding.value_out_of_range",
            Self::LayoutEvidenceMissing => "parameter_encoding.layout_evidence_missing",
            Self::OutOfBounds => "parameter_encoding.out_of_bounds",
            Self::ArithmeticOverflow => "parameter_encoding.arithmetic_overflow",
            Self::Overlap => "parameter_encoding.overlap",
            Self::UnsupportedReference => "parameter_encoding.reference_unsupported",
            Self::UnverifiedReferenceContext => "parameter_encoding.reference_context_unverified",
        }
    }
    pub fn english_fallback(self) -> &'static str {
        match self {
            Self::Cancelled => "Parameter encoding was cancelled.",
            Self::Limit => "Parameter encoding exceeds a fixed inspection limit.",
            Self::ValueOutOfRange => "The parameter value must be between 0 and 60.",
            Self::LayoutEvidenceMissing => {
                "The required byte layout evidence is missing or unsupported."
            }
            Self::OutOfBounds => "The parameter layout is outside the segment.",
            Self::ArithmeticOverflow => "The parameter layout arithmetic overflowed.",
            Self::Overlap => "The parameter layout overlaps another declared field.",
            Self::UnsupportedReference => "The parameter reference input is unsupported.",
            Self::UnverifiedReferenceContext => {
                "The parameter activation context and module occupancy are unverified."
            }
        }
    }
}
impl std::fmt::Display for ParameterEncodingError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}: {}", self.key(), self.english_fallback())
    }
}
impl std::error::Error for ParameterEncodingError {}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ByteChange {
    /// Segment-relative byte offset, never a device address.
    pub offset: usize,
    pub before: u8,
    pub after: u8,
}

/// Offline partial bytes only, without load controls or execution authority.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct OfflinePartialSegment {
    pub baseline_sha256: String,
    pub encoded_sha256: String,
    pub bytes: Arc<[u8]>,
    pub changes: Vec<ByteChange>,
}

// No public constructor or caller-supplied proof flag. Only a reviewed qualifier
// may supply this layout to the core. Unit tests use clearly owned fixtures.
struct ByteLayout {
    byte_offset: usize,
    bit_offset: u8,
    width_bits: u8,
    declared_segment_size: usize,
    other_occupied_bits: Vec<Range<usize>>,
}

/// The registered Glass candidate remains unsupported for original encoding.
/// Activation of P-252_R-114 and all module occupancy must be independently
/// qualified before a future reviewed change can return partial segment bytes.
pub fn encode_display_timeout(
    snapshot: &Snapshot,
    value: u32,
    cancelled: &AtomicBool,
) -> Result<OfflinePartialSegment, ParameterEncodingError> {
    checkpoint(cancelled).map_err(|_| ParameterEncodingError::Cancelled)?;
    validate_value(value)?;
    let (baseline, layout) = qualify_reference(snapshot)?;
    encode_unsigned_byte(baseline, &layout, value, cancelled)
}

fn qualify_reference(snapshot: &Snapshot) -> Result<(&[u8], ByteLayout), ParameterEncodingError> {
    if snapshot.application_id != GLASS_APPLICATION_ID
        || snapshot.archive_sha256 != GLASS_REGISTERED_SHA256
    {
        return Err(ParameterEncodingError::UnsupportedReference);
    }
    // These identity labels cannot prove activation or occupancy. In particular,
    // do not treat mutable snapshot fields or removal of blockers as approval.
    Err(ParameterEncodingError::UnverifiedReferenceContext)
}

fn validate_value(value: u32) -> Result<(), ParameterEncodingError> {
    if value > MAX_VALUE {
        return Err(ParameterEncodingError::ValueOutOfRange);
    }
    Ok(())
}

fn encode_unsigned_byte(
    baseline: &[u8],
    layout: &ByteLayout,
    value: u32,
    cancelled: &AtomicBool,
) -> Result<OfflinePartialSegment, ParameterEncodingError> {
    checkpoint(cancelled).map_err(|_| ParameterEncodingError::Cancelled)?;
    validate_value(value)?;
    if baseline.len() > MAX_SEGMENT_BYTES || layout.other_occupied_bits.len() > MAX_OCCUPIED_RANGES
    {
        return Err(ParameterEncodingError::Limit);
    }
    if baseline.is_empty()
        || baseline.len() != layout.declared_segment_size
        || layout.bit_offset != 0
        || layout.width_bits != 8
    {
        return Err(ParameterEncodingError::LayoutEvidenceMissing);
    }
    let end = layout
        .byte_offset
        .checked_add(1)
        .ok_or(ParameterEncodingError::ArithmeticOverflow)?;
    let bit_start = layout
        .byte_offset
        .checked_mul(8)
        .ok_or(ParameterEncodingError::ArithmeticOverflow)?;
    let bit_end = bit_start
        .checked_add(8)
        .ok_or(ParameterEncodingError::ArithmeticOverflow)?;
    if end > baseline.len() {
        return Err(ParameterEncodingError::OutOfBounds);
    }
    let segment_bits = baseline.len() * 8; // Fixed byte bound above prevents overflow.
    for occupied in &layout.other_occupied_bits {
        checkpoint(cancelled).map_err(|_| ParameterEncodingError::Cancelled)?;
        if occupied.start >= occupied.end || occupied.end > segment_bits {
            return Err(ParameterEncodingError::LayoutEvidenceMissing);
        }
        if bit_start < occupied.end && occupied.start < bit_end {
            return Err(ParameterEncodingError::Overlap);
        }
    }
    let after = u8::try_from(value).map_err(|_| ParameterEncodingError::ValueOutOfRange)?;
    let before = baseline[layout.byte_offset];
    let mut bytes = baseline.to_vec();
    bytes[layout.byte_offset] = after;
    let changes = if before == after {
        Vec::new()
    } else {
        vec![ByteChange {
            offset: layout.byte_offset,
            before,
            after,
        }]
    };
    checkpoint(cancelled).map_err(|_| ParameterEncodingError::Cancelled)?;
    Ok(OfflinePartialSegment {
        baseline_sha256: sha256(baseline),
        encoded_sha256: sha256(&bytes),
        bytes: bytes.into(),
        changes,
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    // Original owned 64-byte fixture, distinct from the private 1,968-byte segment.
    fn owned_fixture() -> (Vec<u8>, ByteLayout) {
        let mut bytes: Vec<u8> = (0..64).collect();
        bytes[17] = 20;
        (
            bytes,
            ByteLayout {
                byte_offset: 17,
                bit_offset: 0,
                width_bits: 8,
                declared_segment_size: 64,
                other_occupied_bits: vec![0..136, 144..512],
            },
        )
    }

    #[test]
    fn owned_default_and_changed_vectors_are_deterministic_and_preserve_other_bytes() {
        let (baseline, layout) = owned_fixture();
        let immutable_before = baseline.clone();
        let cancelled = AtomicBool::new(false);
        let default = encode_unsigned_byte(&baseline, &layout, 20, &cancelled).unwrap();
        assert_eq!(default.bytes.as_ref(), baseline.as_slice());
        assert!(default.changes.is_empty());
        assert_eq!(
            default.encoded_sha256,
            "5a26f8719cbf3605d84c13001ae2e33e37467513e94d88d3b259151547a559a1"
        );
        let changed = encode_unsigned_byte(&baseline, &layout, 21, &cancelled).unwrap();
        assert_eq!(
            changed,
            encode_unsigned_byte(&baseline, &layout, 21, &cancelled).unwrap()
        );
        assert_eq!(changed.baseline_sha256, default.encoded_sha256);
        assert_eq!(
            changed.encoded_sha256,
            "eeb4621685ecfe7466df45f79c1935ea747bfb9cf243f13c1b7d3e634c568ea9"
        );
        assert_eq!(
            changed.changes,
            [ByteChange {
                offset: 17,
                before: 20,
                after: 21
            }]
        );
        for (offset, (&before, &after)) in baseline.iter().zip(changed.bytes.iter()).enumerate() {
            assert_eq!(after, if offset == 17 { 21 } else { before });
        }
        assert_eq!(baseline, immutable_before);
        assert_eq!(default.bytes.len(), changed.bytes.len());
    }

    #[test]
    fn owned_min_max_and_invalid_unsigned_values() {
        let (baseline, layout) = owned_fixture();
        for value in [0, 60] {
            let result =
                encode_unsigned_byte(&baseline, &layout, value, &AtomicBool::new(false)).unwrap();
            assert_eq!(u32::from(result.bytes[17]), value);
        }
        for value in [61, 255, 256, u32::MAX] {
            assert_eq!(
                encode_unsigned_byte(&baseline, &layout, value, &AtomicBool::new(false)),
                Err(ParameterEncodingError::ValueOutOfRange)
            );
        }
    }

    #[test]
    fn owned_layout_rejects_missing_evidence_out_of_bounds_and_overflow() {
        let (baseline, mut layout) = owned_fixture();
        for offset in [baseline.len(), baseline.len() + 1] {
            layout.byte_offset = offset;
            assert_eq!(
                encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
                Err(ParameterEncodingError::OutOfBounds)
            );
        }
        layout.byte_offset = usize::MAX;
        assert_eq!(
            encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
            Err(ParameterEncodingError::ArithmeticOverflow)
        );
        layout.byte_offset = usize::MAX / 8 + 1;
        assert_eq!(
            encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
            Err(ParameterEncodingError::ArithmeticOverflow)
        );
        for (bit_offset, width_bits) in [(1, 8), (0, 7), (0, 16), (0, 0)] {
            layout.byte_offset = 17;
            layout.bit_offset = bit_offset;
            layout.width_bits = width_bits;
            assert_eq!(
                encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
                Err(ParameterEncodingError::LayoutEvidenceMissing)
            );
        }
        let (_, mut layout) = owned_fixture();
        layout.declared_segment_size -= 1;
        assert_eq!(
            encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
            Err(ParameterEncodingError::LayoutEvidenceMissing)
        );
        assert_eq!(
            encode_unsigned_byte(&[], &layout, 21, &AtomicBool::new(false)),
            Err(ParameterEncodingError::LayoutEvidenceMissing)
        );
    }

    #[test]
    fn owned_bit_occupancy_rejects_any_overlap_and_invalid_ranges() {
        let (baseline, mut layout) = owned_fixture();
        for range in [135..137, 136..137, 136..144, 143..145] {
            layout.other_occupied_bits = vec![range];
            assert_eq!(
                encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
                Err(ParameterEncodingError::Overlap)
            );
        }
        for range in [137..137, 144..513] {
            layout.other_occupied_bits = vec![range];
            assert_eq!(
                encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
                Err(ParameterEncodingError::LayoutEvidenceMissing)
            );
        }
    }

    #[test]
    fn owned_work_is_bounded_and_cancellation_returns_no_bytes() {
        let (baseline, mut layout) = owned_fixture();
        assert_eq!(
            encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(true)),
            Err(ParameterEncodingError::Cancelled)
        );
        layout.other_occupied_bits = vec![0..1; MAX_OCCUPIED_RANGES + 1];
        assert_eq!(
            encode_unsigned_byte(&baseline, &layout, 21, &AtomicBool::new(false)),
            Err(ParameterEncodingError::Limit)
        );
        layout.other_occupied_bits.clear();
        assert_eq!(
            encode_unsigned_byte(
                &vec![0; MAX_SEGMENT_BYTES + 1],
                &layout,
                21,
                &AtomicBool::new(false)
            ),
            Err(ParameterEncodingError::Limit)
        );
    }

    #[test]
    fn encoding_errors_have_stable_keys_and_english_fallbacks() {
        let errors = [
            ParameterEncodingError::Cancelled,
            ParameterEncodingError::Limit,
            ParameterEncodingError::ValueOutOfRange,
            ParameterEncodingError::LayoutEvidenceMissing,
            ParameterEncodingError::OutOfBounds,
            ParameterEncodingError::ArithmeticOverflow,
            ParameterEncodingError::Overlap,
            ParameterEncodingError::UnsupportedReference,
            ParameterEncodingError::UnverifiedReferenceContext,
        ];
        let keys: std::collections::BTreeSet<_> = errors.iter().map(|e| e.key()).collect();
        assert_eq!(keys.len(), errors.len());
        for error in errors {
            assert!(error.key().starts_with("parameter_encoding."));
            assert!(!error.english_fallback().is_empty());
            assert_eq!(
                error.to_string(),
                format!("{}: {}", error.key(), error.english_fallback())
            );
        }
    }
}
