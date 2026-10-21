use knx_selected_product_parser::{APPLICATION_ID, inspect_archive};
use std::io::{Cursor, Write};
use std::sync::atomic::AtomicBool;
use zip::write::SimpleFileOptions;

const PRODUCT: &str = "M-0083_H-179-0_P-BE.2D04000.2E02";
const HARDWARE: &str = "M-0083_H-179-0_HP-00B3-32-723A";
const CATALOG: &str = "M-0083_H-179-0_HP-00B3-32-723A_CI-BE.2D04000.2E02-1";
fn xml(body: &str) -> String {
    format!(
        r#"<KNX xmlns="http://knx.org/xml/project/20"><ManufacturerData><Manufacturer RefId="M-0083">{body}</Manufacturer></ManufacturerData></KNX>"#
    )
}
fn application(extra: &str) -> String {
    xml(&format!(
        r#"<ApplicationPrograms><ApplicationProgram Id="{APPLICATION_ID}" ApplicationNumber="179" ApplicationVersion="50" MaskVersion="MV-0705" LoadProcedureStyle="ProductProcedure"><Static><Code><AbsoluteSegment Id="s" Address="10" Size="2"><Data>AQI=</Data></AbsoluteSegment></Code><ParameterTypes><ParameterType Id="t"><TypeNumber SizeInBit="8" Type="unsignedInt"/></ParameterType></ParameterTypes><Parameters><Parameter Id="p" ParameterType="t" Value="1"><Memory CodeSegment="s" Offset="0" BitOffset="0"/></Parameter></Parameters><ParameterRefs><ParameterRef Id="pr" RefId="p"/></ParameterRefs><ComObjectTable CodeSegment="s" Offset="0"><ComObject Id="o" Number="0" ObjectSize="1 Bit"/></ComObjectTable><ComObjectRefs><ComObjectRef Id="or" RefId="o"/></ComObjectRefs><AddressTable CodeSegment="s" Offset="0" MaxEntries="1"/><AssociationTable CodeSegment="s" Offset="0" MaxEntries="1"/><LoadProcedures><LoadProcedure><LdCtrlConnect/></LoadProcedure></LoadProcedures>{extra}</Static><Dynamic><ParameterRefRef RefId="pr"/><ComObjectRefRef RefId="or"/></Dynamic></ApplicationProgram></ApplicationPrograms>"#
    ))
}
fn archive(app: &str, extra: Option<(&str, &[u8])>) -> Vec<u8> {
    let catalog = xml(&format!(
        r#"<Catalog><CatalogItem Id="{CATALOG}" ProductRefId="{PRODUCT}" Hardware2ProgramRefId="{HARDWARE}"/></Catalog>"#
    ));
    let hardware = xml(&format!(
        r#"<Hardware><Products><Product Id="{PRODUCT}" OrderNumber="BE-04000.02"/></Products><Hardware2Programs><Hardware2Program Id="{HARDWARE}"><ApplicationProgramRef RefId="{APPLICATION_ID}"/></Hardware2Program></Hardware2Programs></Hardware>"#
    ));
    let mut writer = zip::ZipWriter::new(Cursor::new(Vec::new()));
    for (name, content) in [
        ("M-0083/Catalog.xml", catalog.as_bytes()),
        ("M-0083/Hardware.xml", hardware.as_bytes()),
        ("M-0083/M-0083_A-00B3-32-723A.xml", app.as_bytes()),
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
#[test]
fn selected_static_application_can_be_inspected() {
    let data = archive(&application(""), None);
    assert!(inspect_archive(&data, APPLICATION_ID, &AtomicBool::new(false)).is_ok());
}
#[test]
fn alternative_application_is_rejected() {
    assert!(
        inspect_archive(
            &archive(&application(""), None),
            "M-0083_A-00B0-32-0DFC",
            &AtomicBool::new(false)
        )
        .is_err()
    );
}
#[test]
fn missing_segment_reference_is_rejected() {
    let app = application("").replace("CodeSegment=\"s\"", "CodeSegment=\"absent\"");
    assert!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        )
        .is_err()
    );
}
#[test]
fn unexpected_namespace_is_rejected() {
    let app = application("").replace("project/20", "project/21");
    assert!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        )
        .is_err()
    );
}
#[test]
fn external_entity_and_traversal_are_rejected() {
    let app = format!(
        "<!DOCTYPE KNX [<!ENTITY ext SYSTEM 'file:///private/input'>]>{}",
        application("")
    );
    assert!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        )
        .is_err()
    );
    assert!(
        inspect_archive(
            &archive(&application(""), Some(("../escape", b"x"))),
            APPLICATION_ID,
            &AtomicBool::new(false)
        )
        .is_err()
    );
}

#[test]
fn static_model_retains_declarations_bytes_and_unresolved_semantics() {
    use knx_selected_product_parser::{TableKind, UnsupportedFeature, sha256};
    let input = archive(&application(""), None);
    let snapshot = inspect_archive(&input, APPLICATION_ID, &AtomicBool::new(false)).unwrap();
    assert_eq!(snapshot.archive_sha256, sha256(&input));
    assert_eq!(snapshot.parameters[0].declaration.id, "p");
    assert_eq!(snapshot.parameters[0].default_value.as_deref(), Some("1"));
    assert_eq!(snapshot.parameter_refs[0].target, "p");
    assert_eq!(snapshot.communication_object_refs[0].target, "o");
    assert_eq!(&*snapshot.segments[0].data.as_ref().unwrap().bytes, &[1, 2]);
    assert_eq!(snapshot.tables[0].kind, TableKind::CommunicationObjects);
    assert_eq!(
        snapshot.memory_declarations[0].owner_id.as_deref(),
        Some("p")
    );
    assert_eq!(snapshot.load_steps[0].name, "LdCtrlConnect");
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::UnverifiedLoadProcedure)
    );
    let source = snapshot
        .sources
        .iter()
        .find(|s| s.name.ends_with("723A.xml"))
        .unwrap();
    assert_eq!(&*source.bytes, application("").as_bytes());
    let range = snapshot.parameters[0].declaration.source_range.clone();
    assert!(
        std::str::from_utf8(&source.bytes[range])
            .unwrap()
            .starts_with("<Parameter Id=\"p\"")
    );
    assert_eq!(
        snapshot,
        inspect_archive(&input, APPLICATION_ID, &AtomicBool::new(false)).unwrap()
    );
}
#[test]
fn unknown_elements_attributes_and_load_controls_fail_closed() {
    use knx_selected_product_parser::Error;
    for app in [
        application("<FutureLayout/>"),
        application("").replace("<LdCtrlConnect/>", "<LdCtrlGuess/>"),
        application("").replace("Value=\"1\"", "Value=\"1\" FutureOffset=\"3\""),
    ] {
        assert_eq!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(Error::UnsupportedConstruct)
        );
    }
}
#[test]
fn duplicate_wrong_kind_and_unresolved_dynamic_references_fail() {
    use knx_selected_product_parser::Error;
    for (app, expected) in [
        (
            application("").replace("Id=\"pr\"", "Id=\"p\""),
            Error::DuplicateId,
        ),
        (
            application("").replace("ParameterType=\"t\"", "ParameterType=\"s\""),
            Error::Reference,
        ),
        (
            application("").replace("RefId=\"pr\"", "RefId=\"missing\""),
            Error::Reference,
        ),
        (
            application("").replace("RefId=\"or\"", "RefId=\"pr\""),
            Error::Reference,
        ),
    ] {
        assert_eq!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(expected)
        );
    }
}
#[test]
fn sparse_resources_are_preserved_and_invalid_base64_or_length_is_rejected() {
    use knx_selected_product_parser::Error;
    let sparse = application("").replace("<Data>AQI=</Data>", "");
    assert!(
        inspect_archive(
            &archive(&sparse, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        )
        .unwrap()
        .segments[0]
            .data
            .is_none()
    );
    for data in ["AQI", "AQJ=", "AR==", "AQI=AQI=", "!!!!", "AQID"] {
        let app = application("").replace("AQI=", data);
        assert_eq!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(Error::Resource)
        );
    }
}
#[test]
fn offsets_are_bounds_checked_and_memory_width_is_not_inferred() {
    use knx_selected_product_parser::Error;
    for (app, expected) in [
        (
            application("").replace("Offset=\"0\"", "Offset=\"2\""),
            Error::Resource,
        ),
        (
            application("").replace("BitOffset=\"0\"", "BitOffset=\"8\""),
            Error::Metadata,
        ),
        (
            application("").replace("Address=\"10\"", "Address=\"4294967295\""),
            Error::Resource,
        ),
    ] {
        assert_eq!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(expected)
        );
    }
}
#[test]
fn cancellation_precedes_parsing() {
    use knx_selected_product_parser::Error;
    assert_eq!(
        inspect_archive(b"not a zip", APPLICATION_ID, &AtomicBool::new(true)),
        Err(Error::Cancelled)
    );
}
#[test]
fn case_collisions_shadowing_and_noncanonical_paths_are_rejected() {
    use knx_selected_product_parser::Error;
    for name in [
        "m-0083/CATALOG.XML",
        "M-0083",
        "M-0083//file",
        "M-0083/./file",
        "C:/file",
        "/file",
        "folder\\file",
        "file.",
    ] {
        assert_eq!(
            inspect_archive(
                &archive(&application(""), Some((name, b"x"))),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(Error::ArchivePath),
            "{name}"
        );
    }
}
#[test]
fn crc_is_checked_even_on_an_unselected_member() {
    use knx_selected_product_parser::Error;
    let mut bytes = archive(
        &application(""),
        Some(("unselected.bin", b"unique crc payload")),
    );
    let index = bytes
        .windows(18)
        .position(|w| w == b"unique crc payload")
        .unwrap();
    bytes[index] ^= 1;
    assert_eq!(
        inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)),
        Err(Error::Archive)
    );
}
#[test]
fn archive_size_and_xml_depth_and_attribute_limits_are_enforced() {
    use knx_selected_product_parser::{Error, MAX_ARCHIVE_BYTES};
    assert_eq!(
        inspect_archive(
            &vec![0; MAX_ARCHIVE_BYTES + 1],
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Limit)
    );
    let deep = application(&format!(
        "{}{}",
        "<Dynamic>".repeat(33),
        "</Dynamic>".repeat(33)
    ));
    assert_eq!(
        inspect_archive(
            &archive(&deep, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Limit)
    );
    let attributes: String = (0..65).map(|i| format!(" a{i}=\"1\"")).collect();
    let wide = application(&format!("<Options{attributes}/>"));
    assert_eq!(
        inspect_archive(
            &archive(&wide, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Limit)
    );
}
#[test]
fn namespace_rebinding_malformed_xml_and_unknown_entities_are_rejected() {
    use knx_selected_product_parser::Error;
    for app in [
        application("").replace("<Static>", "<Static xmlns=\"urn:other\">"),
        application("").replace("<Static>", "<x:Static>"),
        application("").replace("<Data>AQI=</Data>", "<Data>&external;</Data>"),
        application("").replace("</Static>", "</Code>"),
        application("").replace("Id=\"p\"", "Id=\"p\" Id=\"q\""),
    ] {
        assert!(matches!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(Error::Namespace | Error::Xml)
        ));
    }
}
#[test]
fn selected_identity_metadata_and_link_must_match() {
    use knx_selected_product_parser::Error;
    for (from, to) in [
        ("ApplicationNumber=\"179\"", "ApplicationNumber=\"180\""),
        ("MaskVersion=\"MV-0705\"", "MaskVersion=\"MV-other\""),
        (APPLICATION_ID, "alternative"),
    ] {
        assert_eq!(
            inspect_archive(
                &archive(&application("").replace(from, to), None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(Error::UnsupportedApplication)
        );
    }
}
#[test]
#[ignore = "requires the private registered manufacturer archive via KNXPROD_FIXTURE"]
fn registered_private_fixture_matches_independent_inventory() {
    use knx_selected_product_parser::{REGISTERED_SHA256, UnsupportedFeature};
    let path = std::env::var_os("KNXPROD_FIXTURE").expect("explicit private fixture path required");
    let bytes = std::fs::read(path).unwrap();
    let snapshot = inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)).unwrap();
    assert_eq!(bytes.len(), 393923);
    assert_eq!(snapshot.archive_sha256, REGISTERED_SHA256);
    assert_eq!(
        snapshot
            .sources
            .iter()
            .map(|s| s.sha256.as_str())
            .collect::<Vec<_>>(),
        [
            "67b07512e9359403fe97d1f2255e9ae25d66eb0a1c16a3127ba4acab2c341431",
            "8de875c288b4e6cf6e47efc04c625ee0bcd9b472adf81485dd00ed6af274ffa8",
            "dcf00cc75720b44040c4c7482e67332487c518e330851e088644224321742f3a",
        ]
    );
    let app_source = &snapshot.sources[2].bytes;
    let first_parameter = &snapshot.parameters[0];
    assert!(
        std::str::from_utf8(&app_source[first_parameter.declaration.source_range.clone()])
            .unwrap()
            .starts_with("<Parameter ")
    );
    assert_eq!(
        snapshot
            .segments
            .iter()
            .filter_map(|s| s.data.as_ref().map(|d| d.sha256.as_str()))
            .collect::<Vec<_>>(),
        [
            "d11e1f70de4adad7ca9b283b9eba59c58dc50f63e9d8033a57c0e34d43e65b56",
            "026c2d50590664ed05021a8bcbea863d43f1b8cca51fc9f0bc4bb7b7cb3f20d9",
            "61fc4ec1528c5db9749aec719eac0dfa981699c18e64ee5787632c16108d412d",
            "4f43c25b0b20e13529c911cfe66550957007510b22acdf14d698d8bfc4b8458e",
        ]
    );
    assert_eq!(
        snapshot.segments[0].mask.as_ref().unwrap().sha256,
        "3a305b8b1e6e1debba3daf71a1ff213a3f22aeaee2322ec8fe08850afb7dca84"
    );
    assert_eq!(snapshot.parameter_types.len(), 95);
    assert_eq!(snapshot.parameters.len(), 266);
    assert_eq!(snapshot.parameter_refs.len(), 1016);
    assert_eq!(snapshot.communication_objects.len(), 16);
    assert_eq!(snapshot.communication_object_refs.len(), 301);
    assert_eq!(snapshot.segments.len(), 6);
    assert_eq!(snapshot.element_counts["ModuleDef"], 3);
    assert_eq!(snapshot.element_counts["choose"], 214);
    assert_eq!(snapshot.element_counts["when"], 640);
    assert_eq!(snapshot.element_counts["ParameterRefRef"], 1082);
    assert_eq!(snapshot.element_counts["ComObjectRefRef"], 330);
    assert_eq!(snapshot.application_number, 179);
    assert_eq!(snapshot.application_version, 50);
    assert_eq!(snapshot.mask_version, "MV-0705");
    assert_eq!(
        snapshot
            .segments
            .iter()
            .map(|s| s.declared_size)
            .collect::<Vec<_>>(),
        [513, 511, 136, 152, 1, 270]
    );
    assert_eq!(
        snapshot
            .segments
            .iter()
            .map(|s| s.data.as_ref().map(|d| d.bytes.len()))
            .collect::<Vec<_>>(),
        [Some(513), Some(511), None, Some(152), None, Some(270)]
    );
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::ModuleDefinition && b.occurrences == 3)
    );
    println!(
        "offline=true compiler=unsupported sha256={} types={} parameters={} parameter_refs={} objects={} object_refs={} segments={} blockers={:?}",
        snapshot.archive_sha256,
        snapshot.parameter_types.len(),
        snapshot.parameters.len(),
        snapshot.parameter_refs.len(),
        snapshot.communication_objects.len(),
        snapshot.communication_object_refs.len(),
        snapshot.segments.len(),
        snapshot.compilation_blockers
    );
}

#[test]
fn symlinks_and_missing_application_member_are_rejected() {
    use knx_selected_product_parser::Error;
    let mut writer = zip::ZipWriter::new(Cursor::new(Vec::new()));
    writer
        .add_symlink("link", "../outside", SimpleFileOptions::default())
        .unwrap();
    let bytes = writer.finish().unwrap().into_inner();
    assert_eq!(
        inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)),
        Err(Error::ArchivePath)
    );
    let bytes = zip::ZipWriter::new(Cursor::new(Vec::new()))
        .finish()
        .unwrap()
        .into_inner();
    assert_eq!(
        inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)),
        Err(Error::MissingMember)
    );
}
#[test]
fn member_count_size_ratio_total_and_element_limits_are_enforced() {
    use knx_selected_product_parser::{Error, MAX_ELEMENTS, MAX_MEMBER_BYTES};
    for (count, size, method) in [
        (17, 0, zip::CompressionMethod::Stored),
        (1, MAX_MEMBER_BYTES + 1, zip::CompressionMethod::Deflated),
        (1, 100_000, zip::CompressionMethod::Deflated),
        (5, MAX_MEMBER_BYTES, zip::CompressionMethod::Deflated),
    ] {
        let mut writer = zip::ZipWriter::new(Cursor::new(Vec::new()));
        // The varied input stays below the compression-ratio bound, so the last
        // case independently exercises total expanded size.
        let mut seed = 0x1234abcd_u32;
        let block: Vec<u8> = (0..131072)
            .map(|_| {
                seed ^= seed << 13;
                seed ^= seed >> 17;
                seed ^= seed << 5;
                seed as u8
            })
            .collect();
        let input: Vec<u8> = (0..size)
            .map(|i| {
                if count == 5 || size > MAX_MEMBER_BYTES {
                    if i % 65536 < 8192 {
                        block[(i / 65536 * 8192 + i % 65536) % block.len()]
                    } else {
                        0
                    }
                } else {
                    0
                }
            })
            .collect();
        for i in 0..count {
            writer
                .start_file(
                    format!("f{i}"),
                    SimpleFileOptions::default().compression_method(method),
                )
                .unwrap();
            writer.write_all(&input).unwrap();
        }
        let bytes = writer.finish().unwrap().into_inner();
        if count == 5 || size > MAX_MEMBER_BYTES {
            assert!(bytes.len() < knx_selected_product_parser::MAX_ARCHIVE_BYTES);
            let mut check = zip::ZipArchive::new(Cursor::new(&bytes)).unwrap();
            let entry = check.by_index(0).unwrap();
            assert!(entry.size() <= entry.compressed_size() * 64);
        }
        assert_eq!(
            inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)),
            Err(Error::Limit)
        );
    }
    let app = application(&"<Options/>".repeat(MAX_ELEMENTS));
    assert_eq!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Limit)
    );
}

#[test]
fn module_scope_is_retained_and_cross_scope_references_are_rejected() {
    use knx_selected_product_parser::{Error, UnsupportedFeature};
    let module = r#"<ModuleDefs><ModuleDef Id="m" Name="owned module"><Static><Parameters><Parameter Id="mp" ParameterType="t" Value="0"/></Parameters><ParameterRefs><ParameterRef Id="mpr" RefId="mp"/></ParameterRefs></Static><Dynamic><ParameterRefRef RefId="mpr"/></Dynamic></ModuleDef></ModuleDefs>"#;
    let app = application("").replace("</Static><Dynamic>", &format!("</Static>{module}<Dynamic>"));
    let snapshot = inspect_archive(
        &archive(&app, None),
        APPLICATION_ID,
        &AtomicBool::new(false),
    )
    .unwrap();
    assert_eq!(
        snapshot.parameters[1].declaration.module_scope.as_deref(),
        Some("m")
    );
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::ModuleDefinition && b.occurrences == 1)
    );
    let cross_scope = app.replace("RefId=\"pr\"", "RefId=\"mpr\"");
    assert_eq!(
        inspect_archive(
            &archive(&cross_scope, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Reference)
    );
}

#[test]
fn duplicate_central_names_and_local_header_mismatches_are_rejected() {
    use knx_selected_product_parser::{APPLICATION_MEMBER, Error};
    let app = application("");
    let alias = "m-0083/m-0083_a-00b3-32-723a.xml";
    let mut bytes = archive(&app, Some((alias, app.as_bytes())));
    let positions: Vec<_> = bytes
        .windows(alias.len())
        .enumerate()
        .filter_map(|(i, b)| (b == alias.as_bytes()).then_some(i))
        .collect();
    assert_eq!(positions.len(), 2);
    for index in positions {
        bytes[index..index + alias.len()].copy_from_slice(APPLICATION_MEMBER.as_bytes());
    }
    assert_eq!(
        inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)),
        Err(Error::Archive)
    );
    let mut bytes = archive(&app, None);
    let index = bytes
        .windows(APPLICATION_MEMBER.len())
        .position(|b| b == APPLICATION_MEMBER.as_bytes())
        .unwrap();
    bytes[index] = b'X';
    assert_eq!(
        inspect_archive(&bytes, APPLICATION_ID, &AtomicBool::new(false)),
        Err(Error::Archive)
    );
}

#[test]
fn source_ranges_include_utf8_bom_offset() {
    let app = format!("\u{feff}{}", application(""));
    let snapshot = inspect_archive(
        &archive(&app, None),
        APPLICATION_ID,
        &AtomicBool::new(false),
    )
    .unwrap();
    let source = &snapshot.sources[2].bytes;
    let range = snapshot.parameters[0].declaration.source_range.clone();
    assert!(
        std::str::from_utf8(&source[range])
            .unwrap()
            .starts_with("<Parameter ")
    );
}

#[test]
fn invalid_xml_characters_and_document_prolog_are_rejected() {
    use knx_selected_product_parser::Error;
    for app in [
        application("").replace("Value=\"1\"", "Value=\"&#1;\""),
        application("").replace("AQI=", "&#1;"),
        format!("<!--before--><?xml version=\"1.0\"?>{}", application("")),
        format!("<![CDATA[ ]]> {}", application("")),
        format!(
            "<?xml version=\"1.0\"?><?xml version=\"1.0\"?>{}",
            application("")
        ),
    ] {
        assert_eq!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(Error::Xml)
        );
    }
}
