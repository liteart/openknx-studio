// SPDX-License-Identifier: Apache-2.0
use crate::{
    Error, GLASS_LITE_APPLICATION_ID, GLASS_LITE_APPLICATION_MEMBER, GLASS_LITE_NAMESPACE,
    GLASS_LITE_REGISTERED_SHA256, InspectionProfile, ParameterEncodingError, UnsupportedFeature,
    encode_display_timeout, inspect_archive_with_profile, sha256,
};
use std::io::{Cursor, Write};
use std::sync::atomic::AtomicBool;
use zip::write::SimpleFileOptions;

const PRODUCT: &str = "M-0083_H-142-1_P-BE.2DGTL4Tx.2E01";
const HARDWARE: &str = "M-0083_H-142-1_HP-0090-13-D12F";
const CATALOG: &str = "M-0083_H-142-1_HP-0090-13-D12F_CI-BE.2DGTL4Tx.2E01-1";

fn xml(body: &str) -> String {
    format!(
        "<KNX xmlns=\"{GLASS_LITE_NAMESPACE}\"><ManufacturerData><Manufacturer RefId=\"M-0083\">{body}</Manufacturer></ManufacturerData></KNX>"
    )
}
fn application() -> String {
    xml(&format!(
        r#"<ApplicationPrograms><ApplicationProgram Id="{GLASS_LITE_APPLICATION_ID}" ApplicationNumber="144" ApplicationVersion="19" MaskVersion="MV-0705" LoadProcedureStyle="ProductProcedure"><Static><Code><AbsoluteSegment Id="s" Address="10" Size="2"><Data>AQI=</Data></AbsoluteSegment><AbsoluteSegment Id="ram" Address="20" Size="2" MemoryType="RAM"/></Code><ParameterTypes><ParameterType Id="t"><TypeNumber SizeInBit="8" Type="unsignedInt"/></ParameterType><ParameterType Id="text"><TypeText SizeInBit="8" Pattern=".*"/></ParameterType></ParameterTypes><Parameters><Parameter Id="p" ParameterType="t" Value="1"><Memory CodeSegment="s" Offset="0" BitOffset="0"/></Parameter></Parameters><ParameterRefs><ParameterRef Id="pr" RefId="p"/></ParameterRefs><ComObjectTable CodeSegment="s" Offset="0"><ComObject Id="o" Number="0" ObjectSize="1 Bit"/></ComObjectTable><ComObjectRefs><ComObjectRef Id="or" RefId="o"/></ComObjectRefs><AddressTable CodeSegment="s" Offset="0" MaxEntries="1"/><AssociationTable CodeSegment="s" Offset="0" MaxEntries="1"/><LoadProcedures><LoadProcedure><LdCtrlConnect/><LdCtrlAbsSegment Access="0" Address="20" LsmIdx="1" MemType="0" SegFlags="0" SegType="0" Size="2"/></LoadProcedure></LoadProcedures></Static><Dynamic><ChannelIndependentBlock><choose ParamRefId="pr"><when test="1"><ParameterBlock Id="block"><ParameterRefRef RefId="pr"/><ComObjectRefRef RefId="or"/><ParameterSeparator Id="separator" Access="None" Text="Unevaluated"/></ParameterBlock><ParameterBlockRename Id="rename" RefId="block" Text="Unevaluated rename"/></when></choose></ChannelIndependentBlock></Dynamic></ApplicationProgram></ApplicationPrograms>"#
    ))
}
fn archive(app: &str, catalog: &str, hardware: &str, extra: Option<(&str, &[u8])>) -> Vec<u8> {
    let mut writer = zip::ZipWriter::new(Cursor::new(Vec::new()));
    for (name, content) in [
        ("M-0083/Catalog.xml", catalog.as_bytes()),
        ("M-0083/Hardware.xml", hardware.as_bytes()),
        (GLASS_LITE_APPLICATION_MEMBER, app.as_bytes()),
    ]
    .into_iter()
    .chain(extra)
    {
        writer
            .start_file(
                name,
                SimpleFileOptions::default().compression_method(zip::CompressionMethod::Stored),
            )
            .unwrap();
        writer.write_all(content).unwrap();
    }
    writer.finish().unwrap().into_inner()
}
fn catalog() -> String {
    xml(&format!(
        r#"<Catalog><CatalogItem Id="{CATALOG}" ProductRefId="{PRODUCT}" Hardware2ProgramRefId="{HARDWARE}"/></Catalog>"#
    ))
}
fn hardware() -> String {
    xml(&format!(
        r#"<Hardware><Products><Product Id="{PRODUCT}" OrderNumber="BE-GTL4Tx.01"/></Products><Hardware2Programs><Hardware2Program Id="{HARDWARE}"><ApplicationProgramRef RefId="{GLASS_LITE_APPLICATION_ID}"/></Hardware2Program></Hardware2Programs></Hardware>"#
    ))
}
fn inspect(bytes: &[u8]) -> Result<crate::Snapshot, Error> {
    crate::inspect_archive_structure(
        bytes,
        GLASS_LITE_APPLICATION_ID,
        InspectionProfile::GlassLiteV13,
        &AtomicBool::new(false),
    )
}
#[test]
fn public_lite_profile_rejects_unregistered_bytes_even_with_matching_structure() {
    let bytes = archive(&application(), &catalog(), &hardware(), None);
    assert_eq!(
        inspect(&bytes).unwrap().application_id,
        GLASS_LITE_APPLICATION_ID
    );
    assert!(
        inspect_archive_with_profile(
            &bytes,
            GLASS_LITE_APPLICATION_ID,
            InspectionProfile::GlassLiteV13,
            &AtomicBool::new(false),
        )
        .is_err()
    );
}
#[test]
fn lite_inspection_is_deterministic_and_retains_absent_data_and_unevaluated_content() {
    let data = archive(&application(), &catalog(), &hardware(), None);
    let snapshot = inspect(&data).unwrap();
    assert_eq!(snapshot, inspect(&data).unwrap());
    assert_eq!(snapshot.namespace, GLASS_LITE_NAMESPACE);
    assert_eq!(snapshot.application_version, 19);
    assert_eq!(
        snapshot.segments[0].data.as_ref().unwrap().bytes.as_ref(),
        [1, 2]
    );
    assert!(snapshot.segments[1].data.is_none());
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::Conditional)
    );
    assert!(
        snapshot
            .opaque_declarations
            .iter()
            .any(|d| d.name == "ParameterBlockRename")
    );
    assert!(
        snapshot
            .opaque_declarations
            .iter()
            .any(|d| d.name == "choose")
    );
    let source = &snapshot.sources[2].bytes;
    for declaration in &snapshot.opaque_declarations {
        assert!(
            std::str::from_utf8(&source[declaration.source_range.clone()])
                .unwrap()
                .starts_with('<')
        );
    }
}
#[test]
fn lite_module_declarations_remain_opaque_and_do_not_activate_configuration() {
    let app = application()
        .replace("</ApplicationProgram>", "<ModuleDefs><ModuleDef Id=\"md\" Name=\"Unevaluated\"><Static/><Dynamic/></ModuleDef></ModuleDefs></ApplicationProgram>")
;
    let app = app.replace("<ChannelIndependentBlock>", "<Channel Id=\"channel\" Name=\"Synthetic\" Number=\"1\"><Module Id=\"instance\" RefId=\"md\"/></Channel><ChannelIndependentBlock>");
    let snapshot = inspect(&archive(&app, &catalog(), &hardware(), None)).unwrap();
    for name in ["ModuleDef", "Module"] {
        assert!(snapshot.opaque_declarations.iter().any(|d| d.name == name));
    }
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::ModuleInstance)
    );
}
#[test]
fn lite_inspection_cannot_enable_existing_compiler() {
    let mut snapshot = inspect(&archive(&application(), &catalog(), &hardware(), None)).unwrap();
    for erase_blockers in [false, true] {
        if erase_blockers {
            snapshot.compilation_blockers.clear();
        }
        assert_eq!(
            encode_display_timeout(&snapshot, 20, &AtomicBool::new(false)),
            Err(ParameterEncodingError::UnsupportedReference)
        );
    }
}
#[test]
fn lite_wrong_application_product_link_and_namespace_fail() {
    for (app, cat, hw) in [
        (
            application().replace("ApplicationVersion=\"19\"", "ApplicationVersion=\"20\""),
            catalog(),
            hardware(),
        ),
        (
            application(),
            catalog().replace(PRODUCT, "wrong-product"),
            hardware(),
        ),
        (
            application(),
            catalog(),
            hardware().replace(GLASS_LITE_APPLICATION_ID, "wrong-application"),
        ),
        (
            application().replace("project/11", "project/20"),
            catalog(),
            hardware(),
        ),
        (
            application(),
            catalog(),
            hardware().replace("BE-GTL4Tx.01", "BE-GTL2Tx.01"),
        ),
    ] {
        assert!(inspect(&archive(&app, &cat, &hw, None)).is_err());
    }
}
#[test]
fn lite_missing_references_unknown_content_and_prohibited_markup_fail() {
    for app in [
        application().replace("CodeSegment=\"s\"", "CodeSegment=\"missing\""),
        application().replace("RefId=\"block\"", "RefId=\"p\""),
        application().replace("<LdCtrlConnect/>", "<LdCtrlInvented/>"),
        application().replace("<Static>", "<Static invented=\"yes\">"),
        format!(
            "<!DOCTYPE KNX [<!ENTITY x SYSTEM 'file:///private/secret'>]>{}",
            application()
        ),
    ] {
        assert!(inspect(&archive(&app, &catalog(), &hardware(), None)).is_err());
    }
}
#[test]
fn lite_archive_paths_and_profile_limits_are_enforced() {
    assert_eq!(
        inspect(&archive(
            &application(),
            &catalog(),
            &hardware(),
            Some(("../escape", b"x"))
        )),
        Err(Error::ArchivePath)
    );
    let too_large = vec![0; InspectionProfile::GlassLiteV13.limits().archive_bytes + 1];
    assert_eq!(inspect(&too_large), Err(Error::Limit));
    let data = archive(&application(), &catalog(), &hardware(), None);
    assert_eq!(
        inspect_archive_with_profile(
            &data,
            GLASS_LITE_APPLICATION_ID,
            InspectionProfile::GlassLiteV13,
            &AtomicBool::new(true)
        ),
        Err(Error::Cancelled)
    );
    assert!(
        inspect_archive_with_profile(
            &data,
            GLASS_LITE_APPLICATION_ID,
            InspectionProfile::GlassV31,
            &AtomicBool::new(false)
        )
        .is_err()
    );
}
#[test]
#[ignore = "requires private registered Glass Lite archive via KNXPROD_GLASS_LITE_FIXTURE"]
fn registered_lite_private_fixture_matches_independent_inventory() {
    let bytes = std::fs::read(
        std::env::var_os("KNXPROD_GLASS_LITE_FIXTURE")
            .expect("explicit private fixture path required"),
    )
    .unwrap();
    assert_eq!(sha256(&bytes), GLASS_LITE_REGISTERED_SHA256);
    let snapshot = inspect_archive_with_profile(
        &bytes,
        GLASS_LITE_APPLICATION_ID,
        InspectionProfile::GlassLiteV13,
        &AtomicBool::new(false),
    )
    .unwrap();
    let mut changed = bytes.clone();
    changed[0] ^= 1;
    assert_eq!(
        inspect_archive_with_profile(
            &changed,
            GLASS_LITE_APPLICATION_ID,
            InspectionProfile::GlassLiteV13,
            &AtomicBool::new(false),
        ),
        Err(Error::UnsupportedApplication)
    );
    assert_eq!(
        snapshot.sources[2].sha256,
        "8b4142ac7d695e0d8536525a23781ecd45ddb52650d88d90627f2782eeb272a8"
    );
    assert_eq!(snapshot.parameters.len(), 698);
    assert_eq!(snapshot.parameter_types.len(), 105);
    assert_eq!(snapshot.parameter_refs.len(), 2893);
    assert_eq!(snapshot.communication_objects.len(), 88);
    assert_eq!(snapshot.communication_object_refs.len(), 971);
    assert_eq!(snapshot.memory_declarations.len(), 239);
    assert_eq!(snapshot.segments.len(), 6);
    assert_eq!(
        snapshot
            .segments
            .iter()
            .filter(|s| s.data.is_none())
            .count(),
        2
    );
    let mut absent: Vec<_> = snapshot
        .segments
        .iter()
        .filter(|s| s.data.is_none())
        .map(|s| (s.address, s.declared_size))
        .collect();
    absent.sort();
    assert_eq!(absent, [(0x700, 316), (0x83c, 1)]);
    assert_eq!(
        encode_display_timeout(&snapshot, 20, &AtomicBool::new(false)),
        Err(ParameterEncodingError::UnsupportedReference)
    );
}
