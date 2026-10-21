//! Bounded, offline inspection of one selected product application.
//! This crate has no configuration evaluator, compiler, transport, or write path.
#![forbid(unsafe_code)]

mod archive;
mod xml;

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
            Self::Resource => "An embedded product resource is invalid.",
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
#[derive(Clone, Debug, Eq, PartialEq)]
pub struct MemoryDeclaration {
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
    checkpoint(cancelled)?;
    if application_id != APPLICATION_ID {
        return Err(Error::UnsupportedApplication);
    }
    let members = archive::read(bytes, cancelled)?;
    let mut sources = Vec::new();
    let mut documents = Vec::new();
    for name in [
        "M-0083/Catalog.xml",
        "M-0083/Hardware.xml",
        APPLICATION_MEMBER,
    ] {
        let source = members.get(name).ok_or(Error::MissingMember)?;
        documents.push(xml::parse(source, cancelled)?);
        sources.push(SourceMember {
            name: name.into(),
            sha256: sha256(source),
            bytes: Arc::clone(source),
        });
    }
    select(&documents[0], &documents[1], &documents[2])?;
    let doc = &documents[2];
    let application = doc.single("ApplicationProgram")?;
    if application.required("Id")? != APPLICATION_ID
        || application.number("ApplicationNumber")? != 179
        || application.number("ApplicationVersion")? != 50
        || application.required("MaskVersion")? != "MV-0705"
        || application.required("LoadProcedureStyle")? != "ProductProcedure"
    {
        return Err(Error::UnsupportedApplication);
    }
    let mut snapshot = Snapshot {
        ir_version: 1,
        archive_sha256: sha256(bytes),
        namespace: NAMESPACE.into(),
        catalog_id: CATALOG_ID.into(),
        product_id: PRODUCT_ID.into(),
        hardware_program_id: HARDWARE_PROGRAM_ID.into(),
        application_id: APPLICATION_ID.into(),
        application_number: 179,
        application_version: 50,
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
        tables: Vec::new(),
        load_steps: Vec::new(),
        element_counts: BTreeMap::new(),
        compilation_blockers: Vec::new(),
    };
    let mut blockers = BTreeMap::new();
    for (index, node) in doc.nodes.iter().enumerate() {
        checkpoint(cancelled)?;
        xml::check_application_construct(node)?;
        doc.check_placement(index)?;
        *snapshot
            .element_counts
            .entry(node.name.clone())
            .or_insert(0) += 1;
        validate_references(doc, index)?;
        let declaration = || doc.declaration(index);
        match node.name.as_str() {
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
                if size == 0 || size as usize > MAX_MEMBER_BYTES {
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
                snapshot.memory_declarations.push(MemoryDeclaration {
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
    Ok(snapshot)
}

fn select(catalog: &Document, hardware: &Document, app: &Document) -> Result<(), Error> {
    for doc in [catalog, hardware, app] {
        if doc.single("Manufacturer")?.required("RefId")? != "M-0083" {
            return Err(Error::UnsupportedApplication);
        }
    }
    let item = catalog.by_id(CATALOG_ID)?;
    if item.name != "CatalogItem"
        || item.required("ProductRefId")? != PRODUCT_ID
        || item.required("Hardware2ProgramRefId")? != HARDWARE_PROGRAM_ID
    {
        return Err(Error::UnsupportedApplication);
    }
    let product = hardware.by_id(PRODUCT_ID)?;
    let association = hardware.by_id(HARDWARE_PROGRAM_ID)?;
    if product.name != "Product"
        || product.required("OrderNumber")? != "BE-04000.02"
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
        .get(HARDWARE_PROGRAM_ID)
        .ok_or(Error::Reference)?;
    let references: Vec<_> = hardware
        .children(index)
        .filter(|n| n.name == "ApplicationProgramRef")
        .collect();
    if references.len() != 1 || references[0].required("RefId")? != APPLICATION_ID {
        return Err(Error::UnsupportedApplication);
    }
    if app.single("ApplicationProgram")?.required("Id")? != APPLICATION_ID {
        return Err(Error::UnsupportedApplication);
    }
    Ok(())
}

fn validate_references(doc: &Document, index: usize) -> Result<(), Error> {
    let node = &doc.nodes[index];
    if doc.ancestor(node, "Languages").is_some() {
        return Ok(());
    }
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
            "Parameter" | "ComObject" | "ParameterRef" | "ComObjectRef" | "Argument"
        ) && !matches!(node.name.as_str(), "NumericArg" | "TextArg")
            && doc.scope(index) != doc.scope(target_index)
        {
            return Err(Error::Reference);
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
    Ok(())
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
