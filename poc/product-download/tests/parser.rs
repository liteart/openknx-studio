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
    let snapshot = inspect_archive(&data, APPLICATION_ID, &AtomicBool::new(false)).unwrap();
    assert_eq!(snapshot.sources.len(), 3);
    assert!(snapshot.external_resources.is_empty());
    assert!(snapshot.external_resource_references.is_empty());
}
#[test]
fn display_timeout_public_encoding_cannot_be_unlocked_by_snapshot_labels_or_blockers() {
    use knx_selected_product_parser::{
        GLASS_APPLICATION_ID, GLASS_REGISTERED_SHA256, ParameterEncodingError,
        encode_display_timeout,
    };
    let mut snapshot = inspect_archive(
        &archive(&application(""), None),
        APPLICATION_ID,
        &AtomicBool::new(false),
    )
    .unwrap();
    assert_eq!(
        encode_display_timeout(&snapshot, 21, &AtomicBool::new(false)),
        Err(ParameterEncodingError::UnsupportedReference)
    );
    // Deliberately forged labels are a negative case, never an original fixture.
    snapshot.application_id = GLASS_APPLICATION_ID.into();
    snapshot.archive_sha256 = GLASS_REGISTERED_SHA256.into();
    snapshot.compilation_blockers.clear();
    assert_eq!(
        encode_display_timeout(&snapshot, 21, &AtomicBool::new(false)),
        Err(ParameterEncodingError::UnverifiedReferenceContext)
    );
    assert_eq!(
        encode_display_timeout(&snapshot, 61, &AtomicBool::new(false)),
        Err(ParameterEncodingError::ValueOutOfRange)
    );
    assert_eq!(
        encode_display_timeout(&snapshot, 21, &AtomicBool::new(true)),
        Err(ParameterEncodingError::Cancelled)
    );
}

#[test]
#[ignore = "requires the private registered Glass archive via KNXPROD_GLASS_FIXTURE"]
fn registered_glass_display_timeout_context_rejects_original_encoding() {
    use knx_selected_product_parser::{
        GLASS_REGISTERED_SHA256, ParameterEncodingError, encode_display_timeout, sha256,
    };
    let path =
        std::env::var_os("KNXPROD_GLASS_FIXTURE").expect("explicit private fixture path required");
    let bytes = std::fs::read(path).unwrap();
    assert_eq!(sha256(&bytes), GLASS_REGISTERED_SHA256);
    let snapshot = inspect_glass(&bytes).unwrap();
    let original = snapshot.clone();
    let segment = snapshot
        .segments
        .iter()
        .find(|s| s.declaration.id == "M-0083_A-018B-31-369F_AS-4690")
        .unwrap();
    let baseline = segment.data.as_ref().unwrap();
    assert_eq!(baseline.bytes.len(), 1_968);
    assert_eq!(
        baseline.sha256,
        "66d62fbcc9a0a32c8475a69bb7ce3c1ac695c62718741c599c146fb54b26cc30"
    );
    assert_eq!(baseline.bytes[1_522], 20);
    for value in [0, 20, 21, 60] {
        assert_eq!(
            encode_display_timeout(&snapshot, value, &AtomicBool::new(false)),
            Err(ParameterEncodingError::UnverifiedReferenceContext)
        );
    }
    assert_eq!(
        encode_display_timeout(&snapshot, 61, &AtomicBool::new(false)),
        Err(ParameterEncodingError::ValueOutOfRange)
    );
    assert_eq!(snapshot, original);
    assert!(!snapshot.compilation_blockers.is_empty());
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

fn glass_archive(app: &str, extra: Option<(&str, &[u8])>) -> Vec<u8> {
    use knx_selected_product_parser::{GLASS_APPLICATION_ID, GLASS_APPLICATION_MEMBER};
    use std::io::Read;
    let original = archive(app, extra);
    let mut input = zip::ZipArchive::new(Cursor::new(original)).unwrap();
    let mut writer = zip::ZipWriter::new(Cursor::new(Vec::new()));
    for i in 0..input.len() {
        let mut member = input.by_index(i).unwrap();
        let mut bytes = Vec::new();
        member.read_to_end(&mut bytes).unwrap();
        let original_name = member.name().unwrap();
        let name = if original_name == knx_selected_product_parser::APPLICATION_MEMBER {
            GLASS_APPLICATION_MEMBER
        } else {
            original_name.as_ref()
        };
        let content = if original_name.ends_with(".xml") {
            String::from_utf8(bytes)
                .unwrap()
                .replace(
                    CATALOG,
                    "M-0083_H-3.2E38-1_HP-018B-31-369F_CI-BE.2DGT20x.2E02-1",
                )
                .replace(PRODUCT, "M-0083_H-3.2E38-1_P-BE.2DGT20x.2E02")
                .replace(HARDWARE, "M-0083_H-3.2E38-1_HP-018B-31-369F")
                .replace(APPLICATION_ID, GLASS_APPLICATION_ID)
                .replace("BE-04000.02", "BE-GT20x.02")
                .replace("ApplicationNumber=\"179\"", "ApplicationNumber=\"395\"")
                .replace("ApplicationVersion=\"50\"", "ApplicationVersion=\"49\"")
                .into_bytes()
        } else {
            bytes
        };
        writer
            .start_file(
                name,
                SimpleFileOptions::default().compression_method(zip::CompressionMethod::Stored),
            )
            .unwrap();
        writer.write_all(&content).unwrap();
    }
    writer.finish().unwrap().into_inner()
}
fn inspect_glass(
    bytes: &[u8],
) -> Result<knx_selected_product_parser::Snapshot, knx_selected_product_parser::Error> {
    use knx_selected_product_parser::{
        GLASS_APPLICATION_ID, InspectionProfile, inspect_archive_with_profile,
    };
    inspect_archive_with_profile(
        bytes,
        GLASS_APPLICATION_ID,
        InspectionProfile::GlassV31,
        &AtomicBool::new(false),
    )
}
fn baggage_manifest(declarations: &str) -> String {
    xml(&format!("<Baggages>{declarations}</Baggages>"))
}
const BAGGAGE_DECLARATION: &str = r#"<Baggage Id="external" TargetPath="" Name="owned symbol.png"><FileInfo TimeInfo="2026-10-22T00:00:00Z"/></Baggage>"#;
fn glass_resource_archive(app: &str, manifest: &str, payloads: &[(&str, &[u8])]) -> Vec<u8> {
    let mut writer = zip::ZipWriter::new_append(Cursor::new(glass_archive(app, None))).unwrap();
    for (name, content) in std::iter::once(("M-0083/Baggages.xml", manifest.as_bytes()))
        .chain(payloads.iter().copied())
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
fn glass_external_reference_requires_baggage_document() {
    use knx_selected_product_parser::Error;
    let app = application(r#"<Extension><Baggage RefId="external"/></Extension>"#);
    assert_eq!(
        inspect_glass(&glass_archive(&app, None)),
        Err(Error::MissingMember)
    );
}
#[test]
fn glass_external_resources_preserve_identity_lengths_hashes_and_reference_sources() {
    use knx_selected_product_parser::{UnsupportedFeature, sha256};
    let app = application(r#"<Extension><Baggage RefId="external"/></Extension>"#)
        .replace("</ParameterTypes>", r#"<ParameterType Id="picture"><TypePicture RefId="external"/></ParameterType></ParameterTypes>"#);
    let manifest = baggage_manifest(BAGGAGE_DECLARATION);
    let payload = b"uninterpreted owned fixture";
    let bytes = glass_resource_archive(
        &app,
        &manifest,
        &[("M-0083/Baggages/owned symbol.png", payload)],
    );
    let snapshot = inspect_glass(&bytes).unwrap();
    assert_eq!(snapshot, inspect_glass(&bytes).unwrap());
    assert_eq!(snapshot.archive_sha256, sha256(&bytes));
    assert_eq!(snapshot.sources.len(), 4);
    assert_eq!(snapshot.sources[3].sha256, sha256(manifest.as_bytes()));
    let resource = &snapshot.external_resources[0];
    assert_eq!(resource.declaration.id, "external");
    assert_eq!(resource.declaration.module_scope, None);
    assert_eq!(resource.payload.bytes.len(), payload.len());
    assert_eq!(resource.payload.bytes.as_ref(), payload);
    assert_eq!(resource.payload.sha256, sha256(payload));
    assert_eq!(
        &snapshot.sources[3].bytes[resource.declaration.source_range.clone()],
        BAGGAGE_DECLARATION.as_bytes()
    );
    assert_eq!(snapshot.external_resource_references.len(), 2);
    for reference in &snapshot.external_resource_references {
        assert_eq!(reference.target_id, "external");
        assert_eq!(reference.source.member_sha256, snapshot.sources[2].sha256);
        let raw = &snapshot.sources[2].bytes[reference.source.source_range.clone()];
        assert!(
            std::str::from_utf8(raw)
                .unwrap()
                .contains("RefId=\"external\"")
        );
    }
    assert!(
        snapshot.compilation_blockers.iter().any(|b| b.feature
            == UnsupportedFeature::ExternalResourceReference
            && b.occurrences == 2)
    );
    // No PNG decoding is implied; an empty payload still has an exact identity.
    let empty = inspect_glass(&glass_resource_archive(
        &app,
        &manifest,
        &[("M-0083/Baggages/owned symbol.png", b"")],
    ))
    .unwrap();
    assert_eq!(empty.external_resources[0].payload.bytes.len(), 0);
    assert_eq!(empty.external_resources[0].payload.sha256, sha256(b""));
    assert_ne!(snapshot.archive_sha256, empty.archive_sha256);
}
#[test]
fn glass_external_references_reject_missing_wrong_kind_and_missing_payload() {
    use knx_selected_product_parser::Error;
    let manifest = baggage_manifest(BAGGAGE_DECLARATION);
    for kind in ["Baggage", "TypePicture"] {
        let app = if kind == "Baggage" {
            application(r#"<Extension><Baggage RefId="external"/></Extension>"#)
        } else {
            application("").replace("</ParameterTypes>", r#"<ParameterType Id="picture"><TypePicture RefId="external"/></ParameterType></ParameterTypes>"#)
        };
        assert!(matches!(
            inspect_glass(&glass_resource_archive(&app, &manifest, &[])),
            Err(Error::MissingMember)
        ));
        for target in ["absent", "p", "t"] {
            assert!(matches!(
                inspect_glass(&glass_resource_archive(
                    &app.replace("RefId=\"external\"", &format!("RefId=\"{target}\"")),
                    &manifest,
                    &[("M-0083/Baggages/owned symbol.png", b"fixture")],
                )),
                Err(Error::Reference)
            ));
        }
    }
}
#[test]
fn glass_baggage_declarations_reject_ambiguous_id_and_payload_mapping() {
    use knx_selected_product_parser::Error;
    let app = application(r#"<Extension><Baggage RefId="external"/></Extension>"#);
    for (declarations, error) in [
        (BAGGAGE_DECLARATION.repeat(2), Error::DuplicateId),
        (
            format!(
                "{BAGGAGE_DECLARATION}{}",
                BAGGAGE_DECLARATION.replace("Id=\"external\"", "Id=\"second\"")
            ),
            Error::Resource,
        ),
        (
            format!(
                "{BAGGAGE_DECLARATION}{}",
                BAGGAGE_DECLARATION
                    .replace("Id=\"external\"", "Id=\"second\"")
                    .replace("owned symbol.png", "Owned symbol.png")
            ),
            Error::Resource,
        ),
    ] {
        assert!(matches!(inspect_glass(&glass_resource_archive(
            &app, &baggage_manifest(&declarations),
            &[("M-0083/Baggages/owned symbol.png", b"fixture")],
        )), Err(actual) if actual == error));
    }
}
#[test]
fn glass_baggage_mapping_is_only_empty_target_path_and_safe_flat_png() {
    use knx_selected_product_parser::Error;
    let app = application(r#"<Extension><Baggage RefId="external"/></Extension>"#);
    for declaration in [
        BAGGAGE_DECLARATION.replace("TargetPath=\"\"", "TargetPath=\"nested\""),
        BAGGAGE_DECLARATION.replace(" TargetPath=\"\"", ""),
        BAGGAGE_DECLARATION.replace("owned symbol.png", "nested/owned.png"),
        BAGGAGE_DECLARATION.replace("owned symbol.png", "../owned.png"),
        BAGGAGE_DECLARATION.replace("owned symbol.png", " owned.png"),
        BAGGAGE_DECLARATION.replace("owned symbol.png", "owned.png "),
        BAGGAGE_DECLARATION.replace("owned symbol.png", "owned.bin"),
        BAGGAGE_DECLARATION.replace("owned symbol.png", "owned\\symbol.png"),
    ] {
        assert!(matches!(
            inspect_glass(&glass_resource_archive(
                &app,
                &baggage_manifest(&declaration),
                &[("M-0083/Baggages/owned symbol.png", b"fixture")],
            )),
            Err(Error::Resource)
        ));
    }
    // Case-sensitive lookup: a differently cased declaration cannot resolve.
    assert!(matches!(
        inspect_glass(&glass_resource_archive(
            &app,
            &baggage_manifest(&BAGGAGE_DECLARATION.replace("owned symbol.png", "Owned symbol.png")),
            &[("M-0083/Baggages/owned symbol.png", b"fixture")],
        )),
        Err(Error::MissingMember)
    ));
}
#[test]
fn glass_baggage_xml_reuses_xml_safety_and_rejects_unobserved_structure() {
    use knx_selected_product_parser::Error;
    let app = application(r#"<Extension><Baggage RefId="external"/></Extension>"#);
    let manifest = baggage_manifest(BAGGAGE_DECLARATION);
    for (invalid, error) in [
        (
            manifest.replace("project/20", "project/21"),
            Error::Namespace,
        ),
        (
            format!("<!DOCTYPE KNX [<!ENTITY ext SYSTEM 'file:///private/input'>]>{manifest}"),
            Error::Xml,
        ),
        (manifest.replace("</Baggages>", "</Wrong>"), Error::Xml),
        (
            manifest.replace("RefId=\"M-0083\"", "RefId=\"M-0084\""),
            Error::Metadata,
        ),
        (
            manifest.replace("Name=", "Unknown=\"1\" Name="),
            Error::UnsupportedConstruct,
        ),
        (
            manifest.replace("Name=", "FileIntegrity=\"00000000\" Name="),
            Error::UnsupportedConstruct,
        ),
        (
            manifest.replace("<FileInfo", "<Unknown"),
            Error::UnsupportedConstruct,
        ),
        (
            manifest.replace("<FileInfo", "<FileInfo Version=\"1.0.0.0\""),
            Error::UnsupportedConstruct,
        ),
        (
            manifest.replace("<Baggages>", "<Baggages>text"),
            Error::Metadata,
        ),
        (
            manifest.replace("<Baggages>", "<Baggages><Baggages/>"),
            Error::UnsupportedConstruct,
        ),
        (
            manifest.replace("<FileInfo TimeInfo=\"2026-10-22T00:00:00Z\"/>", ""),
            Error::Metadata,
        ),
    ] {
        assert!(
            matches!(inspect_glass(&glass_resource_archive(
            &app, &invalid, &[("M-0083/Baggages/owned symbol.png", b"fixture")],
        )), Err(actual) if actual == error),
            "expected {error:?}"
        );
    }
}
#[test]
fn glass_is_explicit_and_identity_is_exact() {
    use knx_selected_product_parser::{
        Error, GLASS_APPLICATION_ID, InspectionProfile, inspect_archive_with_profile,
    };
    let bytes = glass_archive(&application(""), None);
    let snapshot = inspect_glass(&bytes).unwrap();
    assert_eq!(snapshot.ir_version, 3);
    assert_eq!(snapshot.application_id, GLASS_APPLICATION_ID);
    assert_eq!(snapshot.application_number, 395);
    assert_eq!(snapshot.application_version, 49);
    assert_eq!(
        inspect_archive(&bytes, GLASS_APPLICATION_ID, &AtomicBool::new(false)),
        Err(Error::UnsupportedApplication)
    );
    for id in [APPLICATION_ID, "M-0083_A-018A-31-EEE8"] {
        assert_eq!(
            inspect_archive_with_profile(
                &bytes,
                id,
                InspectionProfile::GlassV31,
                &AtomicBool::new(false)
            ),
            Err(Error::UnsupportedApplication)
        );
    }
    for (from, to) in [
        (APPLICATION_ID, "M-0083_A-018A-31-EEE8"),
        ("ApplicationNumber=\"179\"", "ApplicationNumber=\"394\""),
        ("ApplicationVersion=\"50\"", "ApplicationVersion=\"48\""),
        ("MV-0705", "MV-other"),
    ] {
        assert_eq!(
            inspect_glass(&glass_archive(&application("").replace(from, to), None)),
            Err(Error::UnsupportedApplication)
        );
    }
}
#[test]
fn glass_preserves_union_order_membership_memory_ownership_and_scope() {
    use knx_selected_product_parser::{MemoryOwner, UnionMemberKind, UnsupportedFeature};
    let union = r#"<Union SizeInBit="8"><Memory CodeSegment="s" Offset="1" BitOffset="0"/><Parameter Id="u2" ParameterType="t" Value="2"/><Parameter Id="u1" ParameterType="t" Value="1"/></Union>"#;
    let module_union = union.replace("u2", "mu2").replace("u1", "mu1");
    let module = format!(
        r#"<ModuleDefs><ModuleDef Id="m"><Static><Parameters>{module_union}</Parameters></Static></ModuleDef></ModuleDefs>"#
    );
    let app = application("")
        .replace("</Parameters>", &format!("{union}</Parameters>"))
        .replace("</Static><Dynamic>", &format!("</Static>{module}<Dynamic>"));
    let snapshot = inspect_glass(&glass_archive(&app, None)).unwrap();
    assert_eq!(snapshot.unions.len(), 2);
    assert_eq!(snapshot.unions[0].module_scope, None);
    assert_eq!(snapshot.unions[1].module_scope.as_deref(), Some("m"));
    assert_eq!(
        snapshot.unions[0]
            .members
            .iter()
            .map(|m| m.kind)
            .collect::<Vec<_>>(),
        [
            UnionMemberKind::Memory,
            UnionMemberKind::Parameter,
            UnionMemberKind::Parameter
        ]
    );
    assert_eq!(snapshot.unions[0].members[1].attributes["Id"], "u2");
    assert_eq!(snapshot.unions[0].members[2].attributes["Id"], "u1");
    for union in &snapshot.unions {
        assert_eq!(union.source.member_sha256, snapshot.sources[2].sha256);
        let source = &snapshot.sources[2].bytes[union.source.source_range.clone()];
        assert!(source.starts_with(b"<Union "));
        assert!(
            union
                .members
                .windows(2)
                .all(|p| p[0].source.source_range.end <= p[1].source.source_range.start)
        );
    }
    assert_eq!(
        snapshot.parameters[1].union_source.as_ref(),
        Some(&snapshot.unions[0].source)
    );
    assert_eq!(
        snapshot.parameters[3].union_source.as_ref(),
        Some(&snapshot.unions[1].source)
    );
    assert_eq!(
        snapshot.memory_declarations[0].owner,
        MemoryOwner::Parameter("p".into())
    );
    assert_eq!(
        snapshot.memory_declarations[1].owner,
        MemoryOwner::Union(snapshot.unions[0].source.clone())
    );
    assert_eq!(snapshot.memory_declarations[1].owner_id, None);
    assert_eq!(
        snapshot.memory_declarations[2].module_scope.as_deref(),
        Some("m")
    );
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::Union && b.occurrences == 2)
    );
}
#[test]
fn glass_additions_are_opaque_blocked_and_isolated_from_default() {
    use knx_selected_product_parser::{Error, UnsupportedFeature};
    let app = application(r#"<Extension><Baggage RefId="external"/></Extension><Options TextParameterEncoding="UTF-8"/>"#)
        .replace("</ParameterTypes>", r#"<ParameterType Id="picture"><TypePicture RefId="external"/></ParameterType><ParameterType Id="float"><TypeFloat Encoding="DPT 9" Increment="1"/></ParameterType></ParameterTypes>"#)
        .replace("<ComObjectRef Id=", "<ComObjectRef InternalDescription=\"opaque\" Id=")
        .replace("<Dynamic><ParameterRefRef", r#"<Dynamic><ParameterBlock Id="pb"/><Channel Id="c"><choose ParamRefId="pr"><when test="1"><ParameterBlock Id="nested"><ComObjectRefRef RefId="or"/></ParameterBlock><Rename Id="rename" RefId="pb" Text="opaque"/></when></choose></Channel><ParameterRefRef"#);
    let snapshot = inspect_glass(&glass_resource_archive(
        &app,
        &baggage_manifest(BAGGAGE_DECLARATION),
        &[("M-0083/Baggages/owned symbol.png", b"opaque owned payload")],
    ))
    .unwrap();
    assert_eq!(
        snapshot
            .opaque_declarations
            .iter()
            .map(|d| d.name.as_str())
            .collect::<Vec<_>>(),
        ["TypePicture", "Baggage", "Rename"]
    );
    assert_eq!(snapshot.opaque_declarations[2].attributes["Text"], "opaque");
    assert!(
        snapshot.compilation_blockers.iter().any(|b| b.feature
            == UnsupportedFeature::ExternalResourceReference
            && b.occurrences == 2)
    );
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::TextRename)
    );
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::UnverifiedLoadProcedure)
    );
    assert_eq!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::UnsupportedConstruct)
    );
    let wrong_kind = app.replace("RefId=\"pb\"", "RefId=\"p\"");
    assert_eq!(
        inspect_glass(&glass_archive(&wrong_kind, None)),
        Err(Error::Reference)
    );
}
#[test]
fn glass_unknown_constructs_placements_and_reference_scopes_fail_closed() {
    use knx_selected_product_parser::Error;
    for app in [
        application("<Future/>"),
        application("<Rename Id=\"r\" RefId=\"p\"/>"),
        application("<Baggage RefId=\"external\"/>"),
        application("").replace("<LdCtrlConnect/>", "<LdCtrlGuess/>"),
        application("").replace("<LdCtrlConnect/>", "<LdCtrlConnect Future=\"1\"/>"),
        application("").replace("<ParameterRef Id=", "<ParameterRef Future=\"1\" Id="),
    ] {
        assert!(matches!(
            inspect_glass(&glass_archive(&app, None)),
            Err(Error::UnsupportedConstruct | Error::Metadata | Error::Xml)
        ));
    }
    let module = r#"<ModuleDefs><ModuleDef Id="m"><Static><Parameters><Parameter Id="mp" ParameterType="t"/></Parameters><ParameterRefs><ParameterRef Id="mpr" RefId="mp"/></ParameterRefs></Static></ModuleDef></ModuleDefs>"#;
    let app = application("")
        .replace("</Static><Dynamic>", &format!("</Static>{module}<Dynamic>"))
        .replace("RefId=\"pr\"", "RefId=\"mpr\"");
    assert_eq!(
        inspect_glass(&glass_archive(&app, None)),
        Err(Error::Reference)
    );
}
#[test]
fn glass_absent_data_and_opaque_load_attributes_are_preserved() {
    let app = application("").replace("<Data>AQI=</Data>", "").replace("<LdCtrlConnect/>", r#"<LdCtrlConnect/><LdCtrlAbsSegment Address="1792" Size="666" LsmIdx="3" Access="0" MemType="2" SegFlags="0" SegType="0"/>"#);
    let snapshot = inspect_glass(&glass_archive(&app, None)).unwrap();
    assert!(snapshot.segments[0].data.is_none());
    assert_eq!(snapshot.load_steps[1].attributes["Size"], "666");
    assert!(!snapshot.compilation_blockers.is_empty());
    assert_eq!(
        &snapshot.sources[2].bytes[snapshot.load_steps[1].source_range.clone()][..17],
        b"<LdCtrlAbsSegment"
    );
}
#[test]
#[ignore = "requires the private registered Glass archive via KNXPROD_GLASS_FIXTURE"]
fn registered_glass_private_fixture_matches_independent_inventory() {
    use knx_selected_product_parser::{GLASS_REGISTERED_SHA256, MemoryOwner, UnsupportedFeature};
    let path =
        std::env::var_os("KNXPROD_GLASS_FIXTURE").expect("explicit private fixture path required");
    let bytes = std::fs::read(path).unwrap();
    assert_eq!(bytes.len(), 900_549);
    let mut inventory = zip::ZipArchive::new(Cursor::new(&bytes)).unwrap();
    assert_eq!(inventory.len(), 57);
    assert_eq!(
        (0..inventory.len())
            .map(|i| inventory.by_index(i).unwrap().size())
            .sum::<u64>(),
        21_193_051
    );
    assert_eq!(
        knx_selected_product_parser::sha256(&bytes),
        GLASS_REGISTERED_SHA256
    );
    let snapshot = inspect_glass(&bytes).unwrap();
    assert_eq!(snapshot.sources[2].bytes.len(), 9_861_680);
    assert_eq!(
        snapshot.sources[2].sha256,
        "f5bfbf9edb0ec799d2182f6962b20195b72d19268bbbe4c7dbe3458b7c0e0f6e"
    );
    assert_eq!(snapshot.sources[3].name, "M-0083/Baggages.xml");
    assert_eq!(snapshot.sources[3].bytes.len(), 9_400);
    assert_eq!(
        snapshot.sources[3].sha256,
        "550d7f910f7091a5cfc78c9dc4bfe3bd7ce08fb7f934f28d3846212325e0d6e1"
    );
    assert_eq!(snapshot.external_resources.len(), 48);
    assert_eq!(snapshot.external_resource_references.len(), 93);
    assert_eq!(
        snapshot
            .external_resource_references
            .iter()
            .filter(|r| r.element_name == "TypePicture")
            .count(),
        48
    );
    assert_eq!(
        snapshot
            .external_resource_references
            .iter()
            .filter(|r| r.element_name == "Baggage")
            .count(),
        45
    );
    assert_eq!(
        snapshot
            .external_resources
            .iter()
            .map(|r| r.payload.bytes.len())
            .sum::<usize>(),
        56_378
    );
    // Independently observed Python ZIP/XML inventory; every identity/length/hash
    // contributes, sorted by exact baggage ID with a final LF on each row.
    let inventory: std::collections::BTreeMap<_, _> = snapshot
        .external_resources
        .iter()
        .map(|r| {
            (
                r.declaration.id.as_str(),
                format!(
                    "{}\t{}\t{}\t{}\n",
                    r.declaration.id,
                    r.payload.name,
                    r.payload.bytes.len(),
                    r.payload.sha256
                ),
            )
        })
        .collect();
    let vector = inventory.values().map(String::as_str).collect::<String>();
    assert_eq!(vector.len(), 7_090);
    assert_eq!(
        knx_selected_product_parser::sha256(vector.as_bytes()),
        "97fb12d42558d742119cc098cfdcd8ec693f29accf1fa00222db51fd9c9959ab"
    );
    assert_eq!(snapshot.element_counts.values().sum::<usize>(), 90_988);
    assert_eq!(snapshot.parameter_types.len(), 213);
    assert_eq!(snapshot.parameters.len(), 1174);
    assert_eq!(snapshot.parameter_refs.len(), 2920);
    assert_eq!(snapshot.communication_objects.len(), 89);
    assert_eq!(snapshot.communication_object_refs.len(), 650);
    assert_eq!(snapshot.unions.len(), 98);
    assert_eq!(snapshot.load_steps.len(), 22);
    assert_eq!(snapshot.memory_declarations.len(), 539);
    assert_eq!(
        snapshot
            .memory_declarations
            .iter()
            .filter(|m| matches!(m.owner, MemoryOwner::Union(_)))
            .count(),
        98
    );
    assert_eq!(
        snapshot
            .memory_declarations
            .iter()
            .filter(|m| matches!(m.owner, MemoryOwner::Parameter(_)))
            .count(),
        441
    );
    assert!(snapshot.memory_declarations.iter().all(|m| {
        m.code_segment
            .as_ref()
            .is_none_or(|s| s.ends_with("AS-4690"))
    }));
    assert_eq!(
        snapshot
            .segments
            .iter()
            .map(|s| s.data.as_ref().map(|d| d.bytes.len()))
            .collect::<Vec<_>>(),
        [Some(513), Some(511), None, Some(656), None, Some(1968)]
    );
    assert_eq!(snapshot.opaque_declarations.len(), 105);
    assert_eq!(snapshot.unverified_scope_references.len(), 7310);
    let mut scope_counts = std::collections::BTreeMap::new();
    for edge in &snapshot.unverified_scope_references {
        assert_eq!(edge.target_module_scope, None);
        *scope_counts
            .entry(edge.source_module_scope.as_deref().unwrap())
            .or_insert(0) += 1;
    }
    assert_eq!(scope_counts["M-0083_A-018B-31-369F_MD-1"], 3706);
    assert_eq!(scope_counts["M-0083_A-018B-31-369F_MD-2"], 3604);
    assert_eq!(snapshot.element_counts["ModuleDef"], 2);
    assert!(
        snapshot
            .compilation_blockers
            .iter()
            .any(|b| b.feature == UnsupportedFeature::UnverifiedLoadProcedure)
    );
    println!(
        "offline=true compiler=unsupported elements={} parameters={} unions={} memories={} opaque={} blockers={:?}",
        snapshot.element_counts.values().sum::<usize>(),
        snapshot.parameters.len(),
        snapshot.unions.len(),
        snapshot.memory_declarations.len(),
        snapshot.opaque_declarations.len(),
        snapshot.compilation_blockers
    );
}

#[test]
fn glass_rename_target_must_share_module_scope() {
    use knx_selected_product_parser::Error;
    let module = r#"<ModuleDefs><ModuleDef Id="m"><Static/><Dynamic><ParameterBlock Id="mpb"/><Channel Id="mc"><choose ParamRefId="mpr"><when test="1"><Rename Id="rename" RefId="mpb" Text="opaque"/></when></choose></Channel></Dynamic></ModuleDef></ModuleDefs>"#
        .replace("<Static/>", r#"<Static><Parameters><Parameter Id="mp" ParameterType="t"/></Parameters><ParameterRefs><ParameterRef Id="mpr" RefId="mp"/></ParameterRefs></Static>"#);
    let app = application("").replace(
        "</Static><Dynamic>",
        &format!("</Static>{module}<Dynamic><ParameterBlock Id=\"global-block\"/>"),
    );
    assert!(inspect_glass(&glass_archive(&app, None)).is_ok());
    let cross = app.replace("RefId=\"mpb\"", "RefId=\"global-block\"");
    assert_eq!(
        inspect_glass(&glass_archive(&cross, None)),
        Err(Error::Reference)
    );
}
#[test]
fn glass_internal_space_baggage_path_allowance_is_narrow() {
    use knx_selected_product_parser::Error;
    let name = "M-0083/Baggages/owned symbol.png";
    assert!(inspect_glass(&glass_archive(&application(""), Some((name, b"opaque")))).is_ok());
    assert_eq!(
        inspect_archive(
            &archive(&application(""), Some((name, b"opaque"))),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::ArchivePath)
    );
    for name in [
        "M-0083/owned symbol.png",
        "M-0083/Baggages/nested/owned symbol.png",
        "M-0083/Baggages/owned symbol.bin",
        "M-0083/Baggages/../owned symbol.png",
        "M-0083/Baggages/owned symbol.png ",
        "M-0083/Baggages/ owned.png",
    ] {
        assert_eq!(
            inspect_glass(&glass_archive(&application(""), Some((name, b"opaque")))),
            Err(Error::ArchivePath)
        );
    }
}
#[test]
fn glass_limits_are_fixed_separate_and_enforced() {
    use knx_selected_product_parser::{Error, InspectionLimits, InspectionProfile};
    assert_eq!(
        InspectionProfile::GlassV31.limits(),
        InspectionLimits {
            archive_bytes: 2 * 1024 * 1024,
            member_bytes: 12 * 1024 * 1024,
            total_bytes: 32 * 1024 * 1024,
            members: 64,
            elements: 100_000,
            depth: 32
        }
    );
    assert_eq!(
        InspectionProfile::Be04000.limits(),
        InspectionLimits {
            archive_bytes: 2 * 1024 * 1024,
            member_bytes: 2 * 1024 * 1024,
            total_bytes: 8 * 1024 * 1024,
            members: 16,
            elements: 20_000,
            depth: 32
        }
    );
    assert_eq!(
        inspect_glass(&vec![0; 2 * 1024 * 1024 + 1]),
        Err(Error::Limit)
    );
    let app = application(&"<Options/>".repeat(20_000));
    assert!(inspect_glass(&glass_archive(&app, None)).is_ok());
    assert_eq!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Limit)
    );
    let app = application(&"<Options/>".repeat(100_000));
    assert_eq!(inspect_glass(&glass_archive(&app, None)), Err(Error::Limit));
    let deep = application(&format!(
        "{}{}",
        "<Dynamic>".repeat(33),
        "</Dynamic>".repeat(33)
    ));
    assert_eq!(
        inspect_glass(&glass_archive(&deep, None)),
        Err(Error::Limit)
    );
    for (count, size) in [
        (17, 0),
        (65, 0),
        (1, 12 * 1024 * 1024 + 1),
        (3, 12 * 1024 * 1024),
    ] {
        let mut writer = zip::ZipWriter::new(Cursor::new(Vec::new()));
        let mut seed = 0x1234abcd_u32;
        let random: Vec<u8> = (0..2 * 1024 * 1024)
            .map(|_| {
                seed ^= seed << 13;
                seed ^= seed >> 17;
                seed ^= seed << 5;
                seed as u8
            })
            .collect();
        let bytes: Vec<u8> = (0..size)
            .map(|i| {
                if i % 131072 < 4096 {
                    random[(i / 131072 * 4096 + i % 131072) % random.len()]
                } else {
                    0
                }
            })
            .collect();
        for i in 0..count {
            writer
                .start_file(
                    format!("f{i}"),
                    SimpleFileOptions::default()
                        .compression_method(zip::CompressionMethod::Deflated),
                )
                .unwrap();
            writer.write_all(&bytes).unwrap();
        }
        let input = writer.finish().unwrap().into_inner();
        if count == 3 {
            assert!(input.len() < 2 * 1024 * 1024);
            let mut check = zip::ZipArchive::new(Cursor::new(&input)).unwrap();
            let entry = check.by_index(0).unwrap();
            assert!(entry.size() <= entry.compressed_size() * 64);
        }
        assert_eq!(
            inspect_glass(&input),
            Err(if count == 17 {
                Error::MissingMember
            } else {
                Error::Limit
            })
        );
    }
}

#[test]
fn glass_module_global_conditions_remain_explicitly_unverified() {
    use knx_selected_product_parser::{Error, ScopeReferenceTargetKind, UnsupportedFeature};
    let module = r#"<ModuleDefs><ModuleDef Id="m"><Static/><Dynamic><Channel Id="c"><choose ParamRefId="pr"><when test="1"/></choose></Channel></Dynamic></ModuleDef></ModuleDefs>"#;
    let app = application("").replace("</Static><Dynamic>", &format!("</Static>{module}<Dynamic>"));
    let snapshot = inspect_glass(&glass_archive(&app, None)).unwrap();
    assert_eq!(snapshot.unverified_scope_references.len(), 1);
    let edge = &snapshot.unverified_scope_references[0];
    assert_eq!(edge.node_name, "choose");
    assert_eq!(edge.attribute, "ParamRefId");
    assert_eq!(edge.target_id, "pr");
    assert_eq!(
        edge.target_kind,
        ScopeReferenceTargetKind::ParameterReference
    );
    assert_eq!(edge.source_module_scope.as_deref(), Some("m"));
    assert_eq!(edge.target_module_scope, None);
    assert_eq!(edge.source.member_sha256, snapshot.sources[2].sha256);
    assert!(snapshot.sources[2].bytes[edge.source.source_range.clone()].starts_with(b"<choose "));
    assert!(
        snapshot.compilation_blockers.iter().any(|b| b.feature
            == UnsupportedFeature::UnverifiedGlobalCondition
            && b.occurrences == 1)
    );
    assert_eq!(
        inspect_archive(
            &archive(&app, None),
            APPLICATION_ID,
            &AtomicBool::new(false)
        ),
        Err(Error::Reference)
    );
    for target in ["missing", "p", "or"] {
        assert_eq!(
            inspect_glass(&glass_archive(
                &app.replace("ParamRefId=\"pr\"", &format!("ParamRefId=\"{target}\"")),
                None
            )),
            Err(Error::Reference)
        );
    }
    let other = r#"<ModuleDef Id="other"><Static><Parameters><Parameter Id="op" ParameterType="t"/></Parameters><ParameterRefs><ParameterRef Id="opr" RefId="op"/></ParameterRefs></Static></ModuleDef>"#;
    let cross = app
        .replace("</ModuleDefs>", &format!("{other}</ModuleDefs>"))
        .replace("ParamRefId=\"pr\"", "ParamRefId=\"opr\"");
    assert_eq!(
        inspect_glass(&glass_archive(&cross, None)),
        Err(Error::Reference)
    );
}

#[test]
fn module_scope_requires_a_nonempty_definition_identity() {
    use knx_selected_product_parser::Error;
    let module = r#"<ModuleDefs><ModuleDef Id="m"><Static><Parameters><Union SizeInBit="8"><Memory CodeSegment="s" Offset="1" BitOffset="0"/><Parameter Id="mp" ParameterType="t"/></Union></Parameters></Static></ModuleDef></ModuleDefs>"#;
    for (from, to, error) in [
        (" Id=\"m\"", "", Error::Metadata),
        ("Id=\"m\"", "Id=\"\"", Error::DuplicateId),
    ] {
        let app = application("").replace(
            "</Static><Dynamic>",
            &format!("</Static>{}<Dynamic>", module.replace(from, to)),
        );
        assert_eq!(inspect_glass(&glass_archive(&app, None)), Err(error));
        assert_eq!(
            inspect_archive(
                &archive(&app, None),
                APPLICATION_ID,
                &AtomicBool::new(false)
            ),
            Err(error)
        );
    }
}
