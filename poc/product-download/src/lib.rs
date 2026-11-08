//! Bounded, offline inspection of one selected product application.
//! Includes a staged offline byte encoder; original-reference encoding remains
//! unqualified. There is no configuration evaluator, transport, or write path.
#![forbid(unsafe_code)]

mod archive;
#[cfg(test)]
mod glass_lite_tests;
mod parameter_encoding;
mod resources;
mod xml;

pub use parameter_encoding::{
    ByteChange, OfflinePartialSegment, ParameterEncodingError, encode_display_timeout,
};

use sha2::{Digest, Sha256};
use std::collections::{BTreeMap, BTreeSet};
use std::ops::Range;
use std::sync::Arc;
use std::sync::atomic::{AtomicBool, Ordering};
use xml::Document;

pub const APPLICATION_ID: &str = "M-0083_A-00B3-32-723A";
pub const CATALOG_ID: &str = "M-0083_H-179-0_HP-00B3-32-723A_CI-BE.2D04000.2E02-1";
pub const PRODUCT_ID: &str = "M-0083_H-179-0_P-BE.2D04000.2E02";
pub const HARDWARE_PROGRAM_ID: &str = "M-0083_H-179-0_HP-00B3-32-723A";
pub const NAMESPACE: &str = "http://knx.org/xml/project/20";
pub const APPLICATION_MEMBER: &str = "M-0083/M-0083_A-00B3-32-723A.xml";
pub const REGISTERED_SHA256: &str =
    "51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709";
pub const MAX_ARCHIVE_BYTES: usize = 2 * 1024 * 1024;
pub const MAX_MEMBER_BYTES: usize = 2 * 1024 * 1024;
pub const MAX_TOTAL_BYTES: usize = 8 * 1024 * 1024;
pub const MAX_MEMBERS: usize = 16;
pub const MAX_ELEMENTS: usize = 20_000;
pub const MAX_DEPTH: usize = 32;

/// Explicit structural profiles; none authorizes compilation.
#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum InspectionProfile {
    Be04000,
    GlassV31,
    /// Fixed project/11 structural candidate; no physical or compiler qualification.
    GlassLiteV13,
}
#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub struct InspectionLimits {
    pub archive_bytes: usize,
    pub member_bytes: usize,
    pub total_bytes: usize,
    pub members: usize,
    pub elements: usize,
    pub depth: usize,
}
pub const GLASS_LITE_APPLICATION_ID: &str = "M-0083_A-0090-13-D12F";
pub const GLASS_LITE_APPLICATION_MEMBER: &str = "M-0083/M-0083_A-0090-13-D12F.xml";
pub const GLASS_LITE_REGISTERED_SHA256: &str =
    "c8feaa000ca891df90b455a4ae27de9e1c0e3d3e5cf53e63cf944807f755c7e9";
pub const GLASS_LITE_NAMESPACE: &str = "http://knx.org/xml/project/11";
pub const GLASS_APPLICATION_ID: &str = "M-0083_A-018B-31-369F";
pub const GLASS_APPLICATION_MEMBER: &str = "M-0083/M-0083_A-018B-31-369F.xml";
pub const GLASS_REGISTERED_SHA256: &str =
    "b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d";
struct Identity {
    application: &'static str,
    member: &'static str,
    catalog: &'static str,
    product: &'static str,
    hardware_program: &'static str,
    order_number: &'static str,
    number: u32,
    version: u32,
}
impl InspectionProfile {
    pub const fn namespace(self) -> &'static str {
        match self {
            Self::GlassLiteV13 => GLASS_LITE_NAMESPACE,
            Self::Be04000 | Self::GlassV31 => NAMESPACE,
        }
    }
    pub const fn limits(self) -> InspectionLimits {
        match self {
            Self::Be04000 => InspectionLimits {
                archive_bytes: MAX_ARCHIVE_BYTES,
                member_bytes: MAX_MEMBER_BYTES,
                total_bytes: MAX_TOTAL_BYTES,
                members: MAX_MEMBERS,
                elements: MAX_ELEMENTS,
                depth: MAX_DEPTH,
            },
            Self::GlassLiteV13 => InspectionLimits {
                archive_bytes: 2 * 1024 * 1024,
                member_bytes: 4 * 1024 * 1024,
                total_bytes: 16 * 1024 * 1024,
                members: 16,
                elements: 50_000,
                depth: 32,
            },
            Self::GlassV31 => InspectionLimits {
                archive_bytes: 2 * 1024 * 1024,
                member_bytes: 12 * 1024 * 1024,
                total_bytes: 32 * 1024 * 1024,
                members: 64,
                elements: 100_000,
                depth: 32,
            },
        }
    }
    fn identity(self) -> Identity {
        match self {
            Self::Be04000 => Identity {
                application: APPLICATION_ID,
                member: APPLICATION_MEMBER,
                catalog: CATALOG_ID,
                product: PRODUCT_ID,
                hardware_program: HARDWARE_PROGRAM_ID,
                order_number: "BE-04000.02",
                number: 179,
                version: 50,
            },
            Self::GlassLiteV13 => Identity {
                application: GLASS_LITE_APPLICATION_ID,
                member: GLASS_LITE_APPLICATION_MEMBER,
                catalog: "M-0083_H-142-1_HP-0090-13-D12F_CI-BE.2DGTL4Tx.2E01-1",
                product: "M-0083_H-142-1_P-BE.2DGTL4Tx.2E01",
                hardware_program: "M-0083_H-142-1_HP-0090-13-D12F",
                order_number: "BE-GTL4Tx.01",
                number: 144,
                version: 19,
            },
            Self::GlassV31 => Identity {
                application: GLASS_APPLICATION_ID,
                member: GLASS_APPLICATION_MEMBER,
                catalog: "M-0083_H-3.2E38-1_HP-018B-31-369F_CI-BE.2DGT20x.2E02-1",
                product: "M-0083_H-3.2E38-1_P-BE.2DGT20x.2E02",
                hardware_program: "M-0083_H-3.2E38-1_HP-018B-31-369F",
                order_number: "BE-GT20x.02",
                number: 395,
                version: 49,
            },
        }
    }
}

#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum Error {
    Cancelled,
    Limit,
    Archive,
    ArchivePath,
    MissingMember,
    Xml,
    Namespace,
    UnsupportedApplication,
    UnsupportedConstruct,
    DuplicateId,
    Reference,
    Resource,
    Metadata,
}
impl Error {
    pub fn key(self) -> &'static str {
        match self {
            Self::Cancelled => "product_parser.cancelled",
            Self::Limit => "product_parser.limit",
            Self::Archive => "product_parser.archive_invalid",
            Self::ArchivePath => "product_parser.archive_path_invalid",
            Self::MissingMember => "product_parser.member_missing",
            Self::Xml => "product_parser.xml_invalid",
            Self::Namespace => "product_parser.namespace_unsupported",
            Self::UnsupportedApplication => "product_parser.application_unsupported",
            Self::UnsupportedConstruct => "product_parser.construct_unsupported",
            Self::DuplicateId => "product_parser.id_duplicate",
            Self::Reference => "product_parser.reference_invalid",
            Self::Resource => "product_parser.resource_invalid",
            Self::Metadata => "product_parser.metadata_invalid",
        }
    }
    pub fn english_fallback(self) -> &'static str {
        match self {
            Self::Cancelled => "Product inspection was cancelled.",
            Self::Limit => "Product input exceeds an inspection limit.",
            Self::Archive => "The product archive is invalid or unsupported.",
            Self::ArchivePath => "The archive contains an unsafe or ambiguous path.",
            Self::MissingMember => "A required product archive member is missing.",
            Self::Xml => "The product XML is invalid or contains prohibited markup.",
            Self::Namespace => "The product XML namespace is unsupported.",
            Self::UnsupportedApplication => "The selected application is unsupported.",
            Self::UnsupportedConstruct => "The application contains an unknown construct.",
            Self::DuplicateId => "The product XML contains a duplicate identity.",
            Self::Reference => "A product reference is missing or has the wrong kind or scope.",
            Self::Resource => "A product resource is invalid or unsupported.",
            Self::Metadata => "Required product metadata is missing or inconsistent.",
        }
    }
}
impl std::fmt::Display for Error {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}: {}", self.key(), self.english_fallback())
    }
}
impl std::error::Error for Error {}

pub(crate) fn checkpoint(cancelled: &AtomicBool) -> Result<(), Error> {
    if cancelled.load(Ordering::Relaxed) {
        Err(Error::Cancelled)
    } else {
        Ok(())
    }
}
pub fn sha256(bytes: &[u8]) -> String {
    Sha256::digest(bytes)
        .iter()
        .map(|byte| format!("{byte:02x}"))
        .collect()
}

/// Exact source attributes are declarations, not evaluated parameter semantics.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Declaration {
    pub id: String,
    pub module_scope: Option<String>,
    pub attributes: BTreeMap<String, String>,
    pub source_range: Range<usize>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ParameterType {
    pub declaration: Declaration,
    pub representation: String,
    pub representation_attributes: BTreeMap<String, String>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Parameter {
    pub declaration: Declaration,
    pub union_source: Option<SourceIdentity>,
    pub parameter_type: String,
    pub default_value: Option<String>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Reference {
    pub declaration: Declaration,
    pub target: String,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct CommunicationObject {
    pub declaration: Declaration,
    pub declared_number: u32,
    pub object_size: String,
    pub base_number_argument: Option<String>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct EmbeddedResource {
    pub sha256: String,
    pub bytes: Arc<[u8]>,
    pub source_range: Range<usize>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Segment {
    pub declaration: Declaration,
    pub address: u32,
    pub declared_size: u32,
    /// Absent data is retained as absent; this parser never invents zero fill.
    pub data: Option<EmbeddedResource>,
    pub mask: Option<EmbeddedResource>,
}
/// Identifies a location in immutable source bytes, never a business identity.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct SourceIdentity {
    pub member_sha256: String,
    pub source_range: Range<usize>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub enum MemoryOwner {
    Parameter(String),
    Union(SourceIdentity),
}
#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum UnionMemberKind {
    Parameter,
    Memory,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct UnionMember {
    pub kind: UnionMemberKind,
    pub source: SourceIdentity,
    pub attributes: BTreeMap<String, String>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct UnionDeclaration {
    pub source: SourceIdentity,
    pub module_scope: Option<String>,
    pub attributes: BTreeMap<String, String>,
    pub members: Vec<UnionMember>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct OpaqueDeclaration {
    pub name: String,
    pub attributes: BTreeMap<String, String>,
    pub module_scope: Option<String>,
    pub source_range: Range<usize>,
}
/// An observed reference edge whose scope semantics are unverified.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct UnverifiedScopeReference {
    pub source: SourceIdentity,
    pub node_name: String,
    pub attribute: String,
    pub target_id: String,
    pub target_kind: ScopeReferenceTargetKind,
    pub source_module_scope: Option<String>,
    pub target_module_scope: Option<String>,
}
#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum ScopeReferenceTargetKind {
    ParameterReference,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct MemoryDeclaration {
    pub owner: MemoryOwner,
    pub module_scope: Option<String>,
    pub owner_id: Option<String>,
    pub code_segment: Option<String>,
    pub base_offset_argument: Option<String>,
    pub byte_offset: u32,
    pub bit_offset: u8,
    pub source_range: Range<usize>,
}
#[derive(Clone, Copy, Debug, Eq, Ord, PartialEq, PartialOrd)]
pub enum TableKind {
    CommunicationObjects,
    Addresses,
    Associations,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Table {
    pub kind: TableKind,
    pub code_segment: String,
    pub declared_offset: u32,
    pub max_entries: Option<u32>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct LoadStep {
    pub name: String,
    pub attributes: BTreeMap<String, String>,
    pub source_range: Range<usize>,
}
#[derive(Clone, Copy, Debug, Eq, Ord, PartialEq, PartialOrd)]
pub enum UnsupportedFeature {
    ModuleDefinition,
    ModuleInstance,
    Conditional,
    Assignment,
    Union,
    ParameterCalculation,
    Script,
    DynamicActivation,
    ParameterTypeSemantics,
    UnverifiedLoadProcedure,
    ExternalResourceReference,
    TextRename,
    UnverifiedGlobalCondition,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct CompilationBlocker {
    pub feature: UnsupportedFeature,
    pub occurrences: usize,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct SourceMember {
    pub name: String,
    pub sha256: String,
    pub bytes: Arc<[u8]>,
}
/// A validated manifest declaration and its exact, uninterpreted archive payload.
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ExternalResource {
    pub declaration: Declaration,
    pub payload: SourceMember,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ExternalResourceReference {
    pub source: SourceIdentity,
    pub element_name: String,
    pub target_id: String,
    pub module_scope: Option<String>,
}
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Snapshot {
    pub ir_version: u32,
    pub archive_sha256: String,
    pub namespace: String,
    pub catalog_id: String,
    pub product_id: String,
    pub hardware_program_id: String,
    pub application_id: String,
    pub application_number: u32,
    pub application_version: u32,
    pub mask_version: String,
    pub load_procedure_style: String,
    pub sources: Vec<SourceMember>,
    pub parameter_types: Vec<ParameterType>,
    pub parameters: Vec<Parameter>,
    pub parameter_refs: Vec<Reference>,
    pub communication_objects: Vec<CommunicationObject>,
    pub communication_object_refs: Vec<Reference>,
    pub segments: Vec<Segment>,
    pub memory_declarations: Vec<MemoryDeclaration>,
    pub unions: Vec<UnionDeclaration>,
    pub opaque_declarations: Vec<OpaqueDeclaration>,
    pub external_resources: Vec<ExternalResource>,
    pub external_resource_references: Vec<ExternalResourceReference>,
    pub unverified_scope_references: Vec<UnverifiedScopeReference>,
    pub tables: Vec<Table>,
    pub load_steps: Vec<LoadStep>,
    pub element_counts: BTreeMap<String, usize>,
    /// Always nonempty: declarations cannot be used as an executable plan.
    pub compilation_blockers: Vec<CompilationBlocker>,
}

/// CPU/filesystem-only; never extracts members, follows resources, or accesses a bus.
/// Cancellation is checked before each member and XML event and during IR traversal.
pub fn inspect_archive(
    bytes: &[u8],
    application_id: &str,
    cancelled: &AtomicBool,
) -> Result<Snapshot, Error> {
    inspect_archive_with_profile(bytes, application_id, InspectionProfile::Be04000, cancelled)
}

/// Opt-in offline inspection with fixed profile-specific identity and bounds.
/// GlassLiteV13 additionally requires the exact registered archive SHA-256;
/// a matching catalog/application structure alone cannot qualify other bytes.
pub fn inspect_archive_with_profile(
    bytes: &[u8],
    application_id: &str,
    profile: InspectionProfile,
    cancelled: &AtomicBool,
) -> Result<Snapshot, Error> {
    if profile == InspectionProfile::GlassLiteV13 {
        checkpoint(cancelled)?;
        if bytes.len() > profile.limits().archive_bytes {
            return Err(Error::Limit);
        }
        if application_id != GLASS_LITE_APPLICATION_ID
            || sha256(bytes) != GLASS_LITE_REGISTERED_SHA256
        {
            return Err(Error::UnsupportedApplication);
        }
    }
    inspect_archive_structure(bytes, application_id, profile, cancelled)
}

// Private structural implementation. Only cfg(test) fixtures may exercise the
// Lite grammar without passing the public entrypoint's registered archive pin.
fn inspect_archive_structure(
    bytes: &[u8],
    application_id: &str,
    profile: InspectionProfile,
    cancelled: &AtomicBool,
) -> Result<Snapshot, Error> {
    checkpoint(cancelled)?;
    let identity = profile.identity();
    let limits = profile.limits();
    if application_id != identity.application {
        return Err(Error::UnsupportedApplication);
    }
    let members = archive::read(bytes, limits, profile, cancelled)?;
    let mut sources = Vec::new();
    let mut documents = Vec::new();
    for name in ["M-0083/Catalog.xml", "M-0083/Hardware.xml", identity.member] {
        let source = members.get(name).ok_or(Error::MissingMember)?;
        documents.push(xml::parse(source, limits, profile.namespace(), cancelled)?);
        sources.push(SourceMember {
            name: name.into(),
            sha256: sha256(source),
            bytes: Arc::clone(source),
        });
    }
    select(&documents[0], &documents[1], &documents[2], &identity)?;
    let doc = &documents[2];
    let application = doc.single("ApplicationProgram")?;
    if application.required("Id")? != identity.application
        || application.number("ApplicationNumber")? != identity.number
        || application.number("ApplicationVersion")? != identity.version
        || application.required("MaskVersion")? != "MV-0705"
        || application.required("LoadProcedureStyle")? != "ProductProcedure"
    {
        return Err(Error::UnsupportedApplication);
    }
    let mut snapshot = Snapshot {
        ir_version: 3,
        archive_sha256: sha256(bytes),
        namespace: profile.namespace().into(),
        catalog_id: identity.catalog.into(),
        product_id: identity.product.into(),
        hardware_program_id: identity.hardware_program.into(),
        application_id: identity.application.into(),
        application_number: identity.number,
        application_version: identity.version,
        mask_version: "MV-0705".into(),
        load_procedure_style: "ProductProcedure".into(),
        sources,
        parameter_types: Vec::new(),
        parameters: Vec::new(),
        parameter_refs: Vec::new(),
        communication_objects: Vec::new(),
        communication_object_refs: Vec::new(),
        segments: Vec::new(),
        memory_declarations: Vec::new(),
        unions: Vec::new(),
        opaque_declarations: Vec::new(),
        external_resources: Vec::new(),
        external_resource_references: Vec::new(),
        unverified_scope_references: Vec::new(),
        tables: Vec::new(),
        load_steps: Vec::new(),
        element_counts: BTreeMap::new(),
        compilation_blockers: Vec::new(),
    };
    let member_sha256 = snapshot.sources[2].sha256.clone();
    let source_identity = |node: &xml::Node| SourceIdentity {
        member_sha256: member_sha256.clone(),
        source_range: node.range.clone(),
    };
    let mut blockers = BTreeMap::new();
    for (index, node) in doc.nodes.iter().enumerate() {
        checkpoint(cancelled)?;
        xml::check_application_construct(node, profile)?;
        doc.check_placement(index, profile)?;
        *snapshot
            .element_counts
            .entry(node.name.clone())
            .or_insert(0) += 1;
        if let Some(observation) = validate_references(doc, index, profile, &member_sha256)? {
            snapshot.unverified_scope_references.push(observation);
            *blockers
                .entry(UnsupportedFeature::UnverifiedGlobalCondition)
                .or_insert(0) += 1;
        }
        let declaration = || doc.declaration(index);
        match node.name.as_str() {
            // A missing module identity must never masquerade as global scope.
            "ModuleDef" => {
                let _ = node.required("Id")?;
            }
            "ParameterType" => {
                let kinds: Vec<_> = doc
                    .children(index)
                    .filter(|n| n.name.starts_with("Type"))
                    .collect();
                if kinds.len() != 1 {
                    return Err(Error::Metadata);
                }
                snapshot.parameter_types.push(ParameterType {
                    declaration: declaration()?,
                    representation: kinds[0].name.clone(),
                    representation_attributes: kinds[0].attrs.clone(),
                });
            }
            "Parameter" => snapshot.parameters.push(Parameter {
                declaration: declaration()?,
                union_source: node
                    .parent
                    .filter(|p| doc.nodes[*p].name == "Union")
                    .map(|p| source_identity(&doc.nodes[p])),
                parameter_type: node.required("ParameterType")?.into(),
                default_value: node.attrs.get("Value").cloned(),
            }),
            "ParameterRef" | "ComObjectRef" => {
                let reference = Reference {
                    declaration: declaration()?,
                    target: node.required("RefId")?.into(),
                };
                if node.name == "ParameterRef" {
                    snapshot.parameter_refs.push(reference);
                } else {
                    snapshot.communication_object_refs.push(reference);
                }
            }
            "ComObject" => snapshot.communication_objects.push(CommunicationObject {
                declaration: declaration()?,
                declared_number: node.number("Number")?,
                object_size: node.required("ObjectSize")?.into(),
                base_number_argument: node.attrs.get("BaseNumber").cloned(),
            }),
            "AbsoluteSegment" => {
                let size = node.number("Size")?;
                if size == 0 || size as usize > limits.member_bytes {
                    return Err(Error::Resource);
                }
                let address = node.number("Address")?;
                address.checked_add(size).ok_or(Error::Resource)?;
                let resource = |name: &str| -> Result<Option<EmbeddedResource>, Error> {
                    let children: Vec<_> = doc.children(index).filter(|n| n.name == name).collect();
                    if children.len() > 1 {
                        return Err(Error::Resource);
                    }
                    children
                        .first()
                        .map(|n| {
                            let bytes: Arc<[u8]> = decode_base64(&n.text)?.into();
                            if bytes.len() > size as usize {
                                return Err(Error::Resource);
                            }
                            Ok(EmbeddedResource {
                                sha256: sha256(&bytes),
                                bytes,
                                source_range: n.range.clone(),
                            })
                        })
                        .transpose()
                };
                snapshot.segments.push(Segment {
                    declaration: declaration()?,
                    address,
                    declared_size: size,
                    data: resource("Data")?,
                    mask: resource("Mask")?,
                });
            }
            "Memory" => {
                let bit = node.number("BitOffset")?;
                if bit > 7 {
                    return Err(Error::Metadata);
                }
                let offset = node.number("Offset")?;
                if let Some(segment_id) = node.attrs.get("CodeSegment") {
                    let segment = doc.by_id(segment_id)?;
                    if offset >= segment.number("Size")? {
                        return Err(Error::Resource);
                    }
                }
                if !node.attrs.contains_key("CodeSegment") && !node.attrs.contains_key("BaseOffset")
                {
                    return Err(Error::Reference);
                }
                let parent = node.parent.ok_or(Error::Metadata)?;
                let owner = match doc.nodes[parent].name.as_str() {
                    "Parameter" => MemoryOwner::Parameter(doc.nodes[parent].required("Id")?.into()),
                    "Union" => MemoryOwner::Union(source_identity(&doc.nodes[parent])),
                    _ => return Err(Error::Metadata),
                };
                snapshot.memory_declarations.push(MemoryDeclaration {
                    owner,
                    module_scope: doc.scope(index),
                    owner_id: node
                        .parent
                        .and_then(|p| doc.nodes[p].attrs.get("Id").cloned()),
                    code_segment: node.attrs.get("CodeSegment").cloned(),
                    base_offset_argument: node.attrs.get("BaseOffset").cloned(),
                    byte_offset: offset,
                    bit_offset: bit as u8,
                    source_range: node.range.clone(),
                });
            }
            "Union" => snapshot.unions.push(UnionDeclaration {
                source: source_identity(node),
                module_scope: doc.scope(index),
                attributes: node.attrs.clone(),
                members: doc
                    .children(index)
                    .map(|child| {
                        Ok(UnionMember {
                            kind: match child.name.as_str() {
                                "Parameter" => UnionMemberKind::Parameter,
                                "Memory" => UnionMemberKind::Memory,
                                _ => return Err(Error::UnsupportedConstruct),
                            },
                            source: source_identity(child),
                            attributes: child.attrs.clone(),
                        })
                    })
                    .collect::<Result<Vec<_>, Error>>()?,
            }),
            "TypePicture" | "Baggage" | "Rename" | "ParameterBlockRename" => {
                snapshot.opaque_declarations.push(OpaqueDeclaration {
                    name: node.name.clone(),
                    attributes: node.attrs.clone(),
                    module_scope: doc.scope(index),
                    source_range: node.range.clone(),
                })
            }
            "ComObjectTable" | "AddressTable" | "AssociationTable" => {
                let segment = node.required("CodeSegment")?;
                let offset = node.number("Offset")?;
                if offset >= doc.by_id(segment)?.number("Size")? {
                    return Err(Error::Resource);
                }
                snapshot.tables.push(Table {
                    kind: match node.name.as_str() {
                        "ComObjectTable" => TableKind::CommunicationObjects,
                        "AddressTable" => TableKind::Addresses,
                        _ => TableKind::Associations,
                    },
                    code_segment: segment.into(),
                    declared_offset: offset,
                    max_entries: node
                        .attrs
                        .get("MaxEntries")
                        .map(|_| node.number("MaxEntries"))
                        .transpose()?,
                });
            }
            name if name.starts_with("LdCtrl") => snapshot.load_steps.push(LoadStep {
                name: name.into(),
                attributes: node.attrs.clone(),
                source_range: node.range.clone(),
            }),
            _ => {}
        }
        // Preserve observed project/11 conditional/module declarations without
        // evaluating activation, allocation or transition semantics.
        if profile == InspectionProfile::GlassLiteV13
            && matches!(
                node.name.as_str(),
                "choose" | "when" | "Assign" | "Module" | "ModuleDef"
            )
        {
            snapshot.opaque_declarations.push(OpaqueDeclaration {
                name: node.name.clone(),
                attributes: node.attrs.clone(),
                module_scope: doc.scope(index),
                source_range: node.range.clone(),
            });
        }
        let feature = match node.name.as_str() {
            "ModuleDef" => Some(UnsupportedFeature::ModuleDefinition),
            "Module" => Some(UnsupportedFeature::ModuleInstance),
            "choose" | "when" => Some(UnsupportedFeature::Conditional),
            "Assign" => Some(UnsupportedFeature::Assignment),
            "Union" => Some(UnsupportedFeature::Union),
            "ParameterCalculation" => Some(UnsupportedFeature::ParameterCalculation),
            "Script" => Some(UnsupportedFeature::Script),
            "Dynamic" => Some(UnsupportedFeature::DynamicActivation),
            "ParameterType" => Some(UnsupportedFeature::ParameterTypeSemantics),
            "LoadProcedure" => Some(UnsupportedFeature::UnverifiedLoadProcedure),
            "TypePicture" | "Baggage" => Some(UnsupportedFeature::ExternalResourceReference),
            "Rename" | "ParameterBlockRename" => Some(UnsupportedFeature::TextRename),
            _ => None,
        };
        if let Some(feature) = feature {
            *blockers.entry(feature).or_insert(0) += 1;
        }
    }
    if snapshot.segments.is_empty() || snapshot.tables.len() != 3 || snapshot.load_steps.is_empty()
    {
        return Err(Error::Metadata);
    }
    let kinds: BTreeSet<_> = snapshot.tables.iter().map(|t| t.kind).collect();
    if kinds.len() != 3 {
        return Err(Error::Metadata);
    }
    snapshot.compilation_blockers = blockers
        .into_iter()
        .map(|(feature, occurrences)| CompilationBlocker {
            feature,
            occurrences,
        })
        .collect();
    if snapshot.compilation_blockers.is_empty() {
        return Err(Error::Metadata);
    }
    if profile == InspectionProfile::GlassV31 {
        resources::inspect(doc, &members, limits, cancelled, &mut snapshot)?;
    }
    Ok(snapshot)
}

fn select(
    catalog: &Document,
    hardware: &Document,
    app: &Document,
    identity: &Identity,
) -> Result<(), Error> {
    for doc in [catalog, hardware, app] {
        if doc.single("Manufacturer")?.required("RefId")? != "M-0083" {
            return Err(Error::UnsupportedApplication);
        }
    }
    let item = catalog.by_id(identity.catalog)?;
    if item.name != "CatalogItem"
        || item.required("ProductRefId")? != identity.product
        || item.required("Hardware2ProgramRefId")? != identity.hardware_program
    {
        return Err(Error::UnsupportedApplication);
    }
    let product = hardware.by_id(identity.product)?;
    let association = hardware.by_id(identity.hardware_program)?;
    if product.name != "Product"
        || product.required("OrderNumber")? != identity.order_number
        || association.name != "Hardware2Program"
    {
        return Err(Error::UnsupportedApplication);
    }
    let product_hardware = hardware.ancestor(product, "Hardware");
    if product_hardware.is_none() || product_hardware != hardware.ancestor(association, "Hardware")
    {
        return Err(Error::Reference);
    }
    let index = *hardware
        .ids
        .get(identity.hardware_program)
        .ok_or(Error::Reference)?;
    let references: Vec<_> = hardware
        .children(index)
        .filter(|n| n.name == "ApplicationProgramRef")
        .collect();
    if references.len() != 1 || references[0].required("RefId")? != identity.application {
        return Err(Error::UnsupportedApplication);
    }
    if app.single("ApplicationProgram")?.required("Id")? != identity.application {
        return Err(Error::UnsupportedApplication);
    }
    Ok(())
}

fn validate_references(
    doc: &Document,
    index: usize,
    profile: InspectionProfile,
    member_sha256: &str,
) -> Result<Option<UnverifiedScopeReference>, Error> {
    let node = &doc.nodes[index];
    if doc.ancestor(node, "Languages").is_some() {
        return Ok(None);
    }
    let mut observation = None;
    for (attribute, value) in &node.attrs {
        let expected = match attribute.as_str() {
            "ParameterType" => "ParameterType",
            "CodeSegment" => "AbsoluteSegment",
            "BaseOffset" | "BaseNumber" => "Argument",
            "TextParameterRefId" | "ParamRefId" | "TargetParamRefRef" | "SourceParamRefRef" => {
                "ParameterRef"
            }
            "RefId" => match node.name.as_str() {
                "ParameterRef" => "Parameter",
                "ComObjectRef" => "ComObject",
                "ParameterRefRef" => "ParameterRef",
                "ComObjectRefRef" => "ComObjectRef",
                "Module" => "ModuleDef",
                "Rename" if profile == InspectionProfile::GlassV31 => "ParameterBlock",
                "ParameterBlockRename" if profile == InspectionProfile::GlassLiteV13 => {
                    "ParameterBlock"
                }
                "TypePicture" | "Baggage" if profile == InspectionProfile::GlassV31 => continue,
                "NumericArg" | "TextArg" => "Argument",
                "Manufacturer" => continue,
                _ => return Err(Error::Reference),
            },
            _ => continue,
        };
        let target = doc.by_id(value)?;
        if target.name != expected {
            return Err(Error::Reference);
        }
        let target_index = *doc.ids.get(value).ok_or(Error::Reference)?;
        if matches!(
            expected,
            "Parameter"
                | "ComObject"
                | "ParameterRef"
                | "ComObjectRef"
                | "Argument"
                | "ParameterBlock"
        ) && !matches!(node.name.as_str(), "NumericArg" | "TextArg")
            && doc.scope(index) != doc.scope(target_index)
        {
            let source_scope = doc.scope(index);
            let target_scope = doc.scope(target_index);
            if profile == InspectionProfile::GlassV31
                && node.name == "choose"
                && attribute == "ParamRefId"
                && source_scope.is_some()
                && target_scope.is_none()
            {
                observation = Some(UnverifiedScopeReference {
                    source: SourceIdentity {
                        member_sha256: member_sha256.into(),
                        source_range: node.range.clone(),
                    },
                    node_name: node.name.clone(),
                    attribute: attribute.clone(),
                    target_id: value.clone(),
                    target_kind: ScopeReferenceTargetKind::ParameterReference,
                    source_module_scope: source_scope,
                    target_module_scope: target_scope,
                });
            } else {
                return Err(Error::Reference);
            }
        }
        if matches!(node.name.as_str(), "NumericArg" | "TextArg") {
            let module_index = doc
                .ancestor_index(index, "Module")
                .ok_or(Error::Reference)?;
            let definition = doc.nodes[module_index].required("RefId")?;
            if doc.scope(target_index).as_deref() != Some(definition) {
                return Err(Error::Reference);
            }
        }
    }
    Ok(observation)
}

// Resource encoding only; this is not a KNX wire codec. Accept canonical padded
// base64 with XML whitespace, reject nonzero pad bits and partial quartets.
fn decode_base64(input: &str) -> Result<Vec<u8>, Error> {
    let chars: Vec<_> = input
        .bytes()
        .filter(|b| !matches!(b, b' ' | b'\t' | b'\r' | b'\n'))
        .collect();
    if chars.len() % 4 != 0 {
        return Err(Error::Resource);
    }
    let value = |b| -> Result<u8, Error> {
        match b {
            b'A'..=b'Z' => Ok(b - b'A'),
            b'a'..=b'z' => Ok(b - b'a' + 26),
            b'0'..=b'9' => Ok(b - b'0' + 52),
            b'+' => Ok(62),
            b'/' => Ok(63),
            _ => Err(Error::Resource),
        }
    };
    let mut bytes = Vec::with_capacity(chars.len() / 4 * 3);
    for (i, c) in chars.as_chunks::<4>().0.iter().enumerate() {
        let a = value(c[0])?;
        let b = value(c[1])?;
        bytes.push(a << 2 | b >> 4);
        if c[2] == b'=' {
            if c[3] != b'=' || b & 15 != 0 || (i + 1) * 4 != chars.len() {
                return Err(Error::Resource);
            }
        } else {
            let d = value(c[2])?;
            bytes.push(b << 4 | d >> 2);
            if c[3] == b'=' {
                if d & 3 != 0 || (i + 1) * 4 != chars.len() {
                    return Err(Error::Resource);
                }
            } else {
                bytes.push(d << 6 | value(c[3])?);
            }
        }
    }
    Ok(bytes)
}
