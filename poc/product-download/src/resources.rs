//! Only the observed Glass project/20 flat PNG baggage mapping is supported.
use crate::{
    Error, ExternalResource, ExternalResourceReference, InspectionLimits, Snapshot, SourceIdentity,
    SourceMember, checkpoint, sha256, xml,
};
use std::collections::{BTreeMap, BTreeSet};
use std::sync::Arc;
use std::sync::atomic::AtomicBool;

const MANIFEST: &str = "M-0083/Baggages.xml";

pub(crate) fn inspect(
    application: &xml::Document,
    members: &BTreeMap<String, Arc<[u8]>>,
    limits: InspectionLimits,
    cancelled: &AtomicBool,
    snapshot: &mut Snapshot,
) -> Result<(), Error> {
    // Preserve application source order, including repeated references.
    for (index, node) in application.nodes.iter().enumerate() {
        checkpoint(cancelled)?;
        if matches!(node.name.as_str(), "TypePicture" | "Baggage") {
            snapshot
                .external_resource_references
                .push(ExternalResourceReference {
                    source: SourceIdentity {
                        member_sha256: snapshot.sources[2].sha256.clone(),
                        source_range: node.range.clone(),
                    },
                    element_name: node.name.clone(),
                    target_id: node.required("RefId")?.into(),
                    module_scope: application.scope(index),
                });
        }
    }
    if snapshot.external_resource_references.is_empty() {
        return Ok(());
    }
    let source = members.get(MANIFEST).ok_or(Error::MissingMember)?;
    let doc = xml::parse(source, limits, crate::NAMESPACE, cancelled)?;
    let mut paths = BTreeSet::new();
    for (index, node) in doc.nodes.iter().enumerate() {
        checkpoint(cancelled)?;
        let (parent, attributes, child, minimum, maximum): (_, &[&str], _, _, _) =
            match node.name.as_str() {
                "KNX" => (
                    None,
                    &[
                        "xmlns",
                        "xmlns:xsi",
                        "xmlns:xsd",
                        "CreatedBy",
                        "ToolVersion",
                    ],
                    Some("ManufacturerData"),
                    1,
                    1,
                ),
                "ManufacturerData" => (Some("KNX"), &[], Some("Manufacturer"), 1, 1),
                "Manufacturer" => (Some("ManufacturerData"), &["RefId"], Some("Baggages"), 1, 1),
                "Baggages" => (
                    Some("Manufacturer"),
                    &[],
                    Some("Baggage"),
                    1,
                    limits.elements,
                ),
                "Baggage" => (
                    Some("Baggages"),
                    &["Id", "TargetPath", "Name"],
                    Some("FileInfo"),
                    1,
                    1,
                ),
                "FileInfo" => (Some("Baggage"), &["TimeInfo"], None, 0, 0),
                _ => return Err(Error::UnsupportedConstruct),
            };
        if node
            .attrs
            .keys()
            .any(|key| !attributes.contains(&key.as_str()))
        {
            return Err(Error::UnsupportedConstruct);
        }
        if node.parent.map(|i| doc.nodes[i].name.as_str()) != parent || !node.text.trim().is_empty()
        {
            return Err(Error::Metadata);
        }
        let mut count = 0;
        for member in doc.children(index) {
            if Some(member.name.as_str()) != child {
                return Err(Error::UnsupportedConstruct);
            }
            count += 1;
        }
        if count < minimum || count > maximum {
            return Err(Error::Metadata);
        }
        if node.name == "Manufacturer" && node.required("RefId")? != "M-0083" {
            return Err(Error::Metadata);
        }
        if node.name != "Baggage" {
            continue;
        }
        // No nested TargetPath interpretation or FileIntegrity semantics are inferred.
        if node.attrs.get("TargetPath").map(String::as_str) != Some("") {
            return Err(Error::Resource);
        }
        let name = node.required("Name")?;
        if !name.ends_with(".png")
            || name.starts_with([' ', '.'])
            || !name
                .bytes()
                .all(|b| b.is_ascii_alphanumeric() || b" _-.".contains(&b))
        {
            return Err(Error::Resource);
        }
        let path = format!("M-0083/Baggages/{name}");
        if path.len() > 240 || !paths.insert(path.to_ascii_lowercase()) {
            return Err(Error::Resource);
        }
        let payload = members.get(&path).ok_or(Error::MissingMember)?;
        snapshot.external_resources.push(ExternalResource {
            declaration: doc.declaration(index)?,
            payload: SourceMember {
                name: path,
                sha256: sha256(payload),
                bytes: Arc::clone(payload),
            },
        });
    }
    for reference in &snapshot.external_resource_references {
        checkpoint(cancelled)?;
        if doc.by_id(&reference.target_id)?.name != "Baggage" {
            return Err(Error::Reference);
        }
    }
    snapshot.sources.push(SourceMember {
        name: MANIFEST.into(),
        sha256: sha256(source),
        bytes: Arc::clone(source),
    });
    Ok(())
}
