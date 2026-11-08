use crate::{Declaration, Error, InspectionLimits, InspectionProfile, checkpoint};
use quick_xml::Reader;
use quick_xml::events::Event;
use std::collections::BTreeMap;
use std::ops::Range;
use std::sync::atomic::AtomicBool;

#[derive(Debug)]
pub(crate) struct Node {
    pub name: String,
    pub attrs: BTreeMap<String, String>,
    pub parent: Option<usize>,
    children: Vec<usize>,
    pub text: String,
    pub range: Range<usize>,
}
impl Node {
    pub fn required(&self, key: &str) -> Result<&str, Error> {
        self.attrs
            .get(key)
            .filter(|v| !v.is_empty())
            .map(String::as_str)
            .ok_or(Error::Metadata)
    }
    pub fn number(&self, key: &str) -> Result<u32, Error> {
        let text = self.required(key)?;
        if !text.bytes().all(|b| b.is_ascii_digit()) {
            return Err(Error::Metadata);
        }
        text.parse().map_err(|_| Error::Metadata)
    }
}
pub(crate) struct Document {
    pub nodes: Vec<Node>,
    pub ids: BTreeMap<String, usize>,
}
impl Document {
    pub fn by_id(&self, id: &str) -> Result<&Node, Error> {
        self.ids
            .get(id)
            .map(|i| &self.nodes[*i])
            .ok_or(Error::Reference)
    }
    pub fn single(&self, name: &str) -> Result<&Node, Error> {
        let mut nodes = self.nodes.iter().filter(|n| n.name == name);
        let node = nodes.next().ok_or(Error::Metadata)?;
        if nodes.next().is_some() {
            return Err(Error::Metadata);
        }
        Ok(node)
    }
    pub fn children(&self, index: usize) -> impl Iterator<Item = &Node> {
        self.nodes[index].children.iter().map(|i| &self.nodes[*i])
    }
    pub fn ancestor_index(&self, index: usize, name: &str) -> Option<usize> {
        let mut parent = self.nodes[index].parent;
        while let Some(i) = parent {
            if self.nodes[i].name == name {
                return Some(i);
            }
            parent = self.nodes[i].parent;
        }
        None
    }
    pub fn ancestor(&self, node: &Node, name: &str) -> Option<usize> {
        let mut parent = node.parent;
        while let Some(i) = parent {
            if self.nodes[i].name == name {
                return Some(i);
            }
            parent = self.nodes[i].parent;
        }
        None
    }
    pub fn scope(&self, index: usize) -> Option<String> {
        self.ancestor_index(index, "ModuleDef")
            .and_then(|i| self.nodes[i].attrs.get("Id").cloned())
    }
    pub fn declaration(&self, index: usize) -> Result<Declaration, Error> {
        let node = &self.nodes[index];
        Ok(Declaration {
            id: node.required("Id")?.into(),
            module_scope: self.scope(index),
            attributes: node.attrs.clone(),
            source_range: node.range.clone(),
        })
    }
    pub fn check_placement(&self, index: usize, profile: InspectionProfile) -> Result<(), Error> {
        let node = &self.nodes[index];
        let expected: &[&str] = match node.name.as_str() {
            "KNX" => {
                return if node.parent.is_none() {
                    Ok(())
                } else {
                    Err(Error::Metadata)
                };
            }
            "ManufacturerData" => &["KNX"],
            "Manufacturer" => &["ManufacturerData"],
            "ApplicationPrograms" | "Languages" => &["Manufacturer"],
            "ApplicationProgram" => &["ApplicationPrograms"],
            "Static" | "Dynamic" => &["ApplicationProgram", "ModuleDef"],
            "ModuleDefs" => &["ApplicationProgram"],
            "Code"
            | "ParameterTypes"
            | "Parameters"
            | "ParameterRefs"
            | "ComObjectTable"
            | "ComObjectRefs"
            | "AddressTable"
            | "AssociationTable"
            | "LoadProcedures"
            | "Extension"
            | "Script"
            | "Allocators"
            | "Options"
            | "ParameterCalculations"
            | "ComObjects" => &["Static"],
            "AbsoluteSegment" => &["Code"],
            "Data" | "Mask" => &["AbsoluteSegment"],
            "ParameterType" => &["ParameterTypes"],
            "Enumeration" => &["TypeRestriction"],
            "Parameter" => &["Parameters", "Union"],
            "Memory" => &["Parameter", "Union"],
            "ParameterRef" => &["ParameterRefs"],
            "ComObject" => &["ComObjectTable", "ComObjects"],
            "ComObjectRef" => &["ComObjectRefs"],
            "LoadProcedure" => &["LoadProcedures"],
            "Allocator" => &["Allocators"],
            "ModuleDef" => &["ModuleDefs"],
            "Arguments" => &["ModuleDef"],
            "Argument" => &["Arguments"],
            "Union" => &["Parameters"],
            "ParameterCalculation" => &["ParameterCalculations"],
            "RLTransformation" | "LRTransformation" | "LParameters" | "RParameters" => {
                &["ParameterCalculation"]
            }
            "ParameterRefRef" => &[
                "LParameters",
                "RParameters",
                "ParameterBlock",
                "when",
                "Dynamic",
            ],
            "ParameterBlock"
                if matches!(
                    profile,
                    InspectionProfile::GlassV31 | InspectionProfile::GlassLiteV13
                ) =>
            {
                &["Channel", "ChannelIndependentBlock", "Dynamic", "when"]
            }
            "ParameterBlock" => &["Channel", "ChannelIndependentBlock", "Dynamic"],
            "choose" if profile == InspectionProfile::GlassLiteV13 => &[
                "Channel",
                "ChannelIndependentBlock",
                "ParameterBlock",
                "when",
            ],
            "choose" => &["Channel", "ParameterBlock", "when"],
            "when" => &["choose"],
            "ParameterSeparator" => &["ParameterBlock", "when"],
            "ComObjectRefRef"
                if matches!(
                    profile,
                    InspectionProfile::GlassV31 | InspectionProfile::GlassLiteV13
                ) =>
            {
                &["when", "Dynamic", "ParameterBlock"]
            }
            "ComObjectRefRef" => &["when", "Dynamic"],
            "Baggage" if profile == InspectionProfile::GlassV31 => &["Extension"],
            "Rename" if profile == InspectionProfile::GlassV31 => &["when"],
            "ParameterBlockRename" if profile == InspectionProfile::GlassLiteV13 => &["when"],
            "Assign" => &["when"],
            "ChannelIndependentBlock" | "Channel" => &["Dynamic"],
            "Module" => &["Channel", "when"],
            "TextArg" | "NumericArg" => &["Module"],
            "Language" => &["Languages"],
            "TranslationUnit" => &["Language"],
            "TranslationElement" => &["TranslationUnit"],
            "Translation" => &["TranslationElement"],
            name if name.starts_with("Type") => &["ParameterType"],
            name if name.starts_with("LdCtrl") => &["LoadProcedure"],
            _ => return Err(Error::UnsupportedConstruct),
        };
        if !node
            .parent
            .is_some_and(|p| expected.contains(&self.nodes[p].name.as_str()))
        {
            return Err(Error::Metadata);
        }
        if !matches!(
            node.name.as_str(),
            "ManufacturerData"
                | "Manufacturer"
                | "ApplicationPrograms"
                | "ApplicationProgram"
                | "Languages"
                | "Language"
                | "TranslationUnit"
                | "TranslationElement"
                | "Translation"
        ) && self.ancestor_index(index, "ApplicationProgram").is_none()
        {
            return Err(Error::Metadata);
        }
        Ok(())
    }
}

pub(crate) fn parse(
    bytes: &[u8],
    limits: InspectionLimits,
    namespace: &str,
    cancelled: &AtomicBool,
) -> Result<Document, Error> {
    if bytes.len() > limits.member_bytes {
        return Err(Error::Limit);
    }
    let text = std::str::from_utf8(bytes).map_err(|_| Error::Xml)?;
    if !text.chars().all(valid_xml_char) {
        return Err(Error::Xml);
    }
    // quick-xml consumes a UTF-8 BOM without counting it in buffer_position.
    // Remove it explicitly so every returned range indexes the original bytes.
    let (text, source_offset) = text
        .strip_prefix('\u{feff}')
        .map_or((text, 0), |text| (text, 3));
    let mut reader = Reader::from_str(text);
    reader.config_mut().check_comments = true;
    reader.config_mut().check_end_names = true;
    let mut doc = Document {
        nodes: Vec::new(),
        ids: BTreeMap::new(),
    };
    let mut stack: Vec<usize> = Vec::new();
    let mut root_seen = false;
    loop {
        checkpoint(cancelled)?;
        let start = reader.buffer_position() as usize + source_offset;
        let event = reader.read_event().map_err(|_| Error::Xml)?;
        match event {
            Event::Start(ref element) | Event::Empty(ref element) => {
                let empty = matches!(event, Event::Empty(_));
                if stack.len() >= limits.depth || doc.nodes.len() >= limits.elements {
                    return Err(Error::Limit);
                }
                let name = element.name().as_ref().to_owned();
                if name.contains(':') {
                    return Err(Error::Namespace);
                }
                let mut attrs = BTreeMap::new();
                for attribute in element.attributes() {
                    if attrs.len() >= 64 {
                        return Err(Error::Limit);
                    }
                    let attribute = attribute.map_err(|_| Error::Xml)?;
                    let key = attribute.key.as_ref().to_owned();
                    if key.contains(':')
                        && !(stack.is_empty() && matches!(key.as_str(), "xmlns:xsi" | "xmlns:xsd"))
                    {
                        return Err(Error::Namespace);
                    }
                    let value = attribute
                        .normalized_value(quick_xml::XmlVersion::Implicit1_0)
                        .map_err(|_| Error::Xml)?
                        .into_owned();
                    if !value.chars().all(valid_xml_char) {
                        return Err(Error::Xml);
                    }
                    if attrs.insert(key, value).is_some() {
                        return Err(Error::Xml);
                    }
                }
                if stack.is_empty() {
                    if root_seen || name != "KNX" {
                        return Err(Error::Xml);
                    }
                    root_seen = true;
                    if attrs.get("xmlns").map(String::as_str) != Some(namespace) {
                        return Err(Error::Namespace);
                    }
                    for (key, expected) in [
                        ("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                        ("xmlns:xsd", "http://www.w3.org/2001/XMLSchema"),
                    ] {
                        if attrs.get(key).is_some_and(|value| value != expected) {
                            return Err(Error::Namespace);
                        }
                    }
                } else if attrs.contains_key("xmlns") {
                    return Err(Error::Namespace);
                }
                let index = doc.nodes.len();
                if let Some(id) = attrs.get("Id")
                    && (id.is_empty() || doc.ids.insert(id.clone(), index).is_some())
                {
                    return Err(Error::DuplicateId);
                }
                let parent = stack.last().copied();
                if let Some(p) = parent {
                    doc.nodes[p].children.push(index);
                }
                doc.nodes.push(Node {
                    name,
                    attrs,
                    parent,
                    children: Vec::new(),
                    text: String::new(),
                    range: start..reader.buffer_position() as usize + source_offset,
                });
                if !empty {
                    stack.push(index);
                }
            }
            Event::End(_) => {
                let index = stack.pop().ok_or(Error::Xml)?;
                doc.nodes[index].range.end = reader.buffer_position() as usize + source_offset;
            }
            Event::Text(value) => {
                append_text(&mut doc, &stack, value.as_ref(), limits.member_bytes)?
            }
            Event::CData(value) => {
                if stack.is_empty() {
                    return Err(Error::Xml);
                }
                append_text(&mut doc, &stack, value.as_ref(), limits.member_bytes)?;
            }
            Event::GeneralRef(value) => {
                if stack.is_empty() {
                    return Err(Error::Xml);
                }
                let reference = format!("&{};", value.as_ref());
                let decoded = quick_xml::escape::unescape(&reference).map_err(|_| Error::Xml)?;
                append_text(&mut doc, &stack, &decoded, limits.member_bytes)?;
            }
            Event::Decl(value) => {
                if root_seen
                    || start != source_offset
                    || value.version().map_err(|_| Error::Xml)?.as_ref() != "1.0"
                {
                    return Err(Error::Xml);
                }
                if let Some(encoding) = value.encoding()
                    && !encoding
                        .map_err(|_| Error::Xml)?
                        .eq_ignore_ascii_case("utf-8")
                {
                    return Err(Error::Xml);
                }
            }
            Event::DocType(_) | Event::PI(_) => return Err(Error::Xml),
            Event::Comment(_) => {}
            Event::Eof => break,
        }
    }
    if !root_seen || !stack.is_empty() {
        return Err(Error::Xml);
    }
    Ok(doc)
}
fn valid_xml_char(c: char) -> bool {
    matches!(c, '\u{9}' | '\u{a}' | '\u{d}' | '\u{20}'..='\u{d7ff}' | '\u{e000}'..='\u{fffd}' | '\u{10000}'..='\u{10ffff}')
}
fn append_text(
    doc: &mut Document,
    stack: &[usize],
    value: &str,
    member_bytes: usize,
) -> Result<(), Error> {
    if !value.chars().all(valid_xml_char) {
        return Err(Error::Xml);
    }
    if let Some(index) = stack.last() {
        let target = &mut doc.nodes[*index].text;
        if target.len() + value.len() > member_bytes {
            return Err(Error::Limit);
        }
        target.push_str(value);
    } else if !value.chars().all(|c| matches!(c, ' ' | '\t' | '\r' | '\n')) {
        return Err(Error::Xml);
    }
    Ok(())
}

// Reference-specific observed shape, not a complete KNX XML schema. Unknown
// elements AND attributes fail closed, including unknown load controls.
pub(crate) fn check_application_construct(
    node: &Node,
    profile: InspectionProfile,
) -> Result<(), Error> {
    let allowed = match node.name.as_str() {
        "KNX" => "xmlns xmlns:xsi xmlns:xsd CreatedBy ToolVersion",
        "Manufacturer" => "RefId",
        "ApplicationProgram" => {
            "ApplicationNumber ApplicationVersion CreatedFromLegacySchemaVersion DefaultLanguage DynamicTableManagement Hash Id Linkable LoadProcedureStyle MaskVersion MinEtsVersion Name NonRegRelevantDataVersion PeiType ProgramType ReplacesVersions"
        }
        "AbsoluteSegment" => "Address Id MemoryType Size",
        "ParameterType" => "Id Name",
        "TypeText" if profile == InspectionProfile::GlassLiteV13 => "Pattern SizeInBit",
        "TypeText" => "SizeInBit",
        "TypeRestriction" => "Base SizeInBit",
        "Enumeration" => "Id Text Value",
        "TypeNumber" => "SizeInBit Type maxInclusive minInclusive",
        "TypeFloat" if profile == InspectionProfile::GlassV31 => {
            "Encoding Increment maxInclusive minInclusive"
        }
        "TypeFloat" => "Encoding maxInclusive minInclusive",
        "TypePicture" | "Baggage" if profile == InspectionProfile::GlassV31 => "RefId",
        "Rename" if profile == InspectionProfile::GlassV31 => "Id RefId Text",
        "ParameterBlockRename" if profile == InspectionProfile::GlassLiteV13 => "Id RefId Text",
        "Options" if profile == InspectionProfile::GlassV31 => {
            "Comparable DownloadInvisibleParameters Reconstructable TextParameterEncoding"
        }
        "TranslationUnit" if profile == InspectionProfile::GlassV31 => "RefId Version",
        "ComObjectRef" if profile == InspectionProfile::GlassV31 => {
            "CommunicationFlag DatapointType FunctionText Id InternalDescription Name ObjectSize ReadFlag RefId Text TextParameterRefId TransmitFlag UpdateFlag WriteFlag"
        }
        "TypeColor" => "Space",
        "Parameter" => {
            "Access BitOffset DefaultUnionParameter Id LegacyPatchAlways Name Offset ParameterType SuffixText Text Value"
        }
        "Memory" => "BaseOffset BitOffset CodeSegment Offset",
        "ParameterRef" => "Access ForbidGrantingUseByCustomer Id RefId SuffixText Text Value",
        "ComObjectTable" => "CodeSegment Offset",
        "ComObject" => {
            "BaseNumber CommunicationFlag DatapointType FunctionText Id MayRead Name Number ObjectSize ReadFlag ReadOnInitFlag Text TransmitFlag UpdateFlag WriteFlag"
        }
        "ComObjectRef" => {
            "CommunicationFlag DatapointType FunctionText Id Name ObjectSize ReadFlag RefId Text TextParameterRefId TransmitFlag UpdateFlag WriteFlag"
        }
        "AddressTable" | "AssociationTable" => "CodeSegment MaxEntries Offset",
        "LdCtrlCompareProp" => "InlineData ObjIdx PropId",
        "LdCtrlUnload" | "LdCtrlLoad" | "LdCtrlLoadCompleted" => "LsmIdx",
        "LdCtrlAbsSegment" => "Access Address LsmIdx MemType SegFlags SegType Size",
        "LdCtrlTaskSegment" => "Address LsmIdx",
        "Allocator" => "Id Name Start maxInclusive",
        "Options" => {
            "Comparable DownloadInvisibleParameters LegacyAllowPartialDownloadIfAp2Mismatch Reconstructable"
        }
        "ModuleDef" | "Module" => {
            if node.name == "Module" {
                "Id Name RefId"
            } else {
                "Id Name"
            }
        }
        "Argument" => "Allocates Id Name Type",
        "Union" => "SizeInBit",
        "ParameterCalculation" => "Id Language Name",
        "ParameterRefRef" => "AliasName RefId",
        "ParameterBlock" => "Access Id Name Text TextParameterRefId",
        "choose" => "ParamRefId",
        "when" => "InternalDescription default test",
        "ComObjectRefRef" => "RefId",
        "ParameterSeparator" if profile == InspectionProfile::GlassLiteV13 => "Access Id Text",
        "ParameterSeparator" => "Id Text UIHint",
        "Assign" => "SourceParamRefRef TargetParamRefRef Value",
        "Channel" => "Id Name Number Text",
        "TextArg" => "Id RefId Value",
        "NumericArg" => "RefId Value",
        "Language" => "Identifier",
        "TranslationUnit" | "TranslationElement" => "RefId",
        "Translation" => "AttributeName Text",
        "ManufacturerData"
        | "ApplicationPrograms"
        | "Static"
        | "Code"
        | "Data"
        | "Mask"
        | "ParameterTypes"
        | "Parameters"
        | "ParameterRefs"
        | "ComObjectRefs"
        | "LoadProcedures"
        | "LoadProcedure"
        | "LdCtrlConnect"
        | "LdCtrlRestart"
        | "LdCtrlDisconnect"
        | "Extension"
        | "Script"
        | "Allocators"
        | "ModuleDefs"
        | "Arguments"
        | "ParameterCalculations"
        | "RLTransformation"
        | "LRTransformation"
        | "LParameters"
        | "RParameters"
        | "ComObjects"
        | "Dynamic"
        | "ChannelIndependentBlock"
        | "Languages" => "",
        _ => return Err(Error::UnsupportedConstruct),
    };
    if node
        .attrs
        .keys()
        .any(|key| !allowed.split_ascii_whitespace().any(|known| known == key))
    {
        return Err(Error::UnsupportedConstruct);
    }
    if !node.text.trim().is_empty()
        && !matches!(
            node.name.as_str(),
            "Data" | "Mask" | "Script" | "RLTransformation" | "LRTransformation"
        )
    {
        return Err(Error::UnsupportedConstruct);
    }
    Ok(())
}
