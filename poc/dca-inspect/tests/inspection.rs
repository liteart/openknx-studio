//! All normal fixtures below are owned synthetic content, never vendor bytes.
use dca_inspect::{
    Error, MAX_ARCHIVE, MAX_ENTRIES, MAX_FILE, MAX_XML, inspect, inspect_path, parse_manifest,
};
use std::{
    io::{Cursor, Write},
    path::Path,
    sync::atomic::AtomicBool,
};
use zip::{CompressionMethod, ZipWriter, write::SimpleFileOptions};
fn flag() -> AtomicBool {
    AtomicBool::new(false)
}
fn manifest() -> String {
    let apps = (1..=6)
        .map(|n| format!("<ApplicationProgram Id=\"M-0000_A-000{n}-10-0000\" />"))
        .collect::<String>();
    format!(
        r#"<?xml version="1.0" encoding="utf-8"?><Knxapp AppId="M0083-A0158" Icon="owned.png" AddInAssembly="Mdt-DaliDCAPlus.dll"><Configuration Is64BitCompatible="true" IsSingleInstanceAddIn="false" AddToContentSwitcher="false" AddToMainToolbar="false" HasConfigurationDialog="false"/><DisplayNames><DisplayName LanguageId="en-US" Name="Owned synthetic fixture"/></DisplayNames><AddInToolbarButtons/><HelpFiles><HelpFile LanguageId="en-US" Path="Help" Name="owned.txt"/></HelpFiles><DeviceConfiguration>{apps}</DeviceConfiguration></Knxapp>"#
    )
}
fn zip(entries: &[(&str, &[u8])], compression: CompressionMethod) -> Vec<u8> {
    let mut writer = ZipWriter::new(Cursor::new(Vec::new()));
    for (name, bytes) in entries {
        let opts = SimpleFileOptions::default().compression_method(compression);
        if name.ends_with('/') {
            writer.add_directory(*name, opts).unwrap();
        } else {
            writer.start_file(*name, opts).unwrap();
            writer.write_all(bytes).unwrap();
        }
    }
    writer.finish().unwrap().into_inner()
}
fn fixture(extra: &[(&str, &[u8])]) -> Vec<u8> {
    let xml = manifest();
    let mut entries = vec![
        ("AddInManifest.xml", xml.as_bytes()),
        ("MDT-DaliDcaPlus.dll", b"Owned Knx.Ets.Sdk token".as_slice()),
        (
            "MDT-DaliDcaPlus.dll.config",
            b"Owned Knx.Falcon token".as_slice(),
        ),
        ("Help/", b"".as_slice()),
    ];
    entries.extend_from_slice(extra);
    zip(&entries, CompressionMethod::Stored)
}
fn central(bytes: &[u8]) -> Vec<usize> {
    bytes
        .windows(4)
        .enumerate()
        .filter_map(|(i, b)| (b == b"PK\x01\x02").then_some(i))
        .collect()
}
fn put32(bytes: &mut [u8], at: usize, value: u32) {
    bytes[at..at + 4].copy_from_slice(&value.to_le_bytes());
}
#[test]
fn synthetic_inventory_separates_counts_and_preserves_spelling() {
    let bytes = fixture(&[("Help/owned.txt", b"owned")]);
    let report = inspect(&bytes, &flag()).unwrap();
    assert_eq!((report.files, report.directories), (4, 1));
    assert_eq!(report.entries.len(), 5);
    assert_eq!(report.manifest.applications.len(), 6);
    assert_eq!(report.manifest.assembly, "Mdt-DaliDCAPlus.dll");
    assert_eq!(report.actual_assembly, "MDT-DaliDcaPlus.dll");
    assert!(report.assembly_case_difference);
    assert_eq!(report.primary_byte_tokens, ["Knx.Ets.Sdk"]);
    assert_eq!(report.config_byte_tokens, ["Knx.Falcon"]);
    assert_eq!(
        report.to_string(),
        inspect(&bytes, &flag()).unwrap().to_string()
    );
    assert!(
        report
            .to_string()
            .contains("CLR_metadata=NOT_RUN; runtime=NOT_RUN")
    );
}
#[test]
fn rejects_traversal_absolute_windows_and_ambiguous_paths() {
    for name in [
        "../bad", "/bad", "a/../bad", "a//bad", "C:/bad", "a\\bad", "bad.", "bad ", "CON.txt",
        "LPT1", "a\0bad", "a\nbad", "é",
    ] {
        assert_eq!(
            inspect(&fixture(&[(name, b"x")]), &flag()),
            Err(Error::UnsafePath),
            "{name:?}"
        );
    }
}
#[test]
fn rejects_case_collisions_file_parent_and_component_case() {
    for extras in [
        vec![("name", b"x".as_slice()), ("NAME", b"x".as_slice())],
        vec![("name", b"x".as_slice()), ("name/child", b"x".as_slice())],
        vec![("help/child", b"x".as_slice())],
    ] {
        assert_eq!(inspect(&fixture(&extras), &flag()), Err(Error::Collision));
    }
}
#[test]
fn rejects_duplicate_central_names_instead_of_library_deduplication() {
    let mut bytes = fixture(&[("aaaa", b"x"), ("bbbb", b"y")]);
    for offset in bytes
        .windows(4)
        .enumerate()
        .filter_map(|(i, w)| (w == b"bbbb").then_some(i))
        .collect::<Vec<_>>()
    {
        bytes[offset..offset + 4].copy_from_slice(b"aaaa");
    }
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Integrity));
}
#[test]
fn rejects_symlink_and_special_modes() {
    let mut writer = ZipWriter::new(Cursor::new(Vec::new()));
    writer
        .add_symlink("owned-link", "target", SimpleFileOptions::default())
        .unwrap();
    let bytes = writer.finish().unwrap().into_inner();
    assert_eq!(inspect(&bytes, &flag()), Err(Error::SpecialFile));
    let mut bytes = fixture(&[]);
    let c = central(&bytes)[1];
    bytes[c + 5] = 3; // Unix origin, then FIFO type in the external attributes.
    put32(&mut bytes, c + 38, 0o010644 << 16);
    assert_eq!(inspect(&bytes, &flag()), Err(Error::SpecialFile));
}
#[test]
fn rejects_encryption_unsupported_compression_and_overlapping_headers() {
    let good = fixture(&[]);
    let offsets = central(&good);
    let mut bytes = good.clone();
    bytes[offsets[0] + 8] |= 1;
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Encryption));
    let mut bytes = good.clone();
    bytes[8..10].copy_from_slice(&99u16.to_le_bytes());
    bytes[offsets[0] + 10..offsets[0] + 12].copy_from_slice(&99u16.to_le_bytes());
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Compression));
    let mut bytes = good;
    put32(&mut bytes, offsets[1] + 42, 0);
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Integrity));
}
#[test]
fn rejects_crc_mismatch_and_local_central_disagreement() {
    let good = fixture(&[]);
    let mut bytes = good.clone();
    let at = bytes.windows(5).position(|w| w == b"Owned").unwrap();
    bytes[at] ^= 1;
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Integrity));
    let mut bytes = good;
    bytes[30] ^= 1;
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Integrity));
}
#[test]
fn enforces_archive_count_entry_total_ratio_and_xml_size_limits() {
    assert_eq!(
        inspect(&vec![0; MAX_ARCHIVE + 1], &flag()),
        Err(Error::ArchiveLimit)
    );
    let mut bytes = fixture(&[]);
    let at = bytes.len() - 22;
    bytes[at + 8..at + 10].copy_from_slice(&((MAX_ENTRIES + 1) as u16).to_le_bytes());
    bytes[at + 10..at + 12].copy_from_slice(&((MAX_ENTRIES + 1) as u16).to_le_bytes());
    assert_eq!(inspect(&bytes, &flag()), Err(Error::EntryLimit));
    let oversized = vec![0; MAX_FILE as usize + 1];
    assert_eq!(
        inspect(
            &zip(&[("owned", &oversized)], CompressionMethod::Deflated),
            &flag()
        ),
        Err(Error::FileLimit)
    );
    let ratio = vec![0; 1024 * 1024];
    assert_eq!(
        inspect(
            &zip(&[("owned", &ratio)], CompressionMethod::Deflated),
            &flag()
        ),
        Err(Error::RatioLimit)
    );
    // Deterministic noisy seed repeated 256 times keeps ratio below 200 while
    // five entries exceed the aggregate bound within the 8 MiB input bound.
    let mut seed = 1u32;
    let noise: Vec<u8> = (0..(MAX_FILE as usize / 256))
        .map(|_| {
            seed ^= seed << 13;
            seed ^= seed >> 17;
            seed ^= seed << 5;
            seed as u8
        })
        .collect();
    let data = noise.repeat(256);
    let bytes = zip(
        &[
            ("a", &data),
            ("b", &data),
            ("c", &data),
            ("d", &data),
            ("e", &data),
        ],
        CompressionMethod::Deflated,
    );
    assert_eq!(inspect(&bytes, &flag()), Err(Error::ExpandedLimit));
    assert_eq!(
        parse_manifest(&vec![b' '; MAX_XML + 1], &flag()),
        Err(Error::XmlLimit)
    );
    assert_eq!(
        inspect(
            &zip(
                &[("AddInManifest.xml", &vec![b' '; MAX_XML + 1])],
                CompressionMethod::Stored
            ),
            &flag()
        ),
        Err(Error::XmlLimit)
    );
}
#[test]
fn rejects_truncated_zip_trailing_data_zip64_and_false_directory_data() {
    let bytes = fixture(&[]);
    for length in [0, 21, bytes.len() - 1] {
        assert_eq!(inspect(&bytes[..length], &flag()), Err(Error::Archive));
    }
    let mut trailing = bytes.clone();
    trailing.push(0);
    assert_eq!(inspect(&trailing, &flag()), Err(Error::Archive));
    let mut zip64 = bytes.clone();
    let at = zip64.len() - 22;
    zip64[at + 12..at + 16].fill(255);
    assert_eq!(inspect(&zip64, &flag()), Err(Error::Archive));
    let mut data = fixture(&[("ownedx", b"x")]);
    for offset in data
        .windows(6)
        .enumerate()
        .filter_map(|(i, w)| (w == b"ownedx").then_some(i))
        .collect::<Vec<_>>()
    {
        data[offset + 5] = b'/';
    }
    assert_eq!(inspect(&data, &flag()), Err(Error::SpecialFile));
}
#[test]
fn rejects_xml_entities_doctype_unknown_fields_duplicates_and_malformed_tree() {
    let good = manifest();
    for bad in [
        good.replace("owned.png", "&amp;"),
        format!("<!DOCTYPE Knxapp [<!ENTITY x SYSTEM 'file:///private'>]>{good}"),
        good.replace("<Configuration ", "<Configuration Surprise=\"true\" "),
        good.replace(
            " Is64BitCompatible=\"true\"",
            " Is64BitCompatible=\"true\" Is64BitCompatible=\"false\"",
        ),
        good.replace("</Knxapp>", "</Wrong>"),
        good.replace("</Knxapp>", ""),
        good.replace("M0083-A0158", "unknown"),
        good.replace(
            "<AddInToolbarButtons/>",
            "<AddInToolbarButtons/><AddInToolbarButtons/>",
        ),
        good.replace("M-0000_A-0002", "M-0000_A-0001"),
        good.replace(
            "<AddInToolbarButtons/>",
            "<AddInToolbarButtons><Unknown/></AddInToolbarButtons>",
        ),
        good.replace("utf-8", "utf-16"),
        good.replace(
            "<AddInToolbarButtons/>",
            "<![CDATA[x]]><AddInToolbarButtons/>",
        ),
    ] {
        assert!(parse_manifest(bad.as_bytes(), &flag()).is_err(), "{bad}");
    }
}
#[test]
fn cancellation_and_missing_input_are_explicit() {
    assert_eq!(
        inspect(&fixture(&[]), &AtomicBool::new(true)),
        Err(Error::Cancelled)
    );
    assert_eq!(
        parse_manifest(manifest().as_bytes(), &AtomicBool::new(true)),
        Err(Error::Cancelled)
    );
    assert_eq!(
        inspect(&zip(&[("owned", b"x")], CompressionMethod::Stored), &flag()),
        Err(Error::MissingFile)
    );
    assert_eq!(
        inspect_path(Path::new("/this-owned-test-path-does-not-exist"), &flag()),
        Err(Error::Io)
    );
}
#[test]
fn resource_errors_have_stable_keys_and_english_fallbacks() {
    assert_eq!(Error::Manifest.key(), "dca_inspect.manifest");
    assert_eq!(
        Error::Manifest.to_string(),
        "dca_inspect.manifest: Manifest is malformed, duplicated or incomplete."
    );
}
#[test]
#[ignore = "requires the separately registered private archive via DCA_REFERENCE_INPUT"]
fn registered_reference_matches_inventory_and_remains_unchanged() {
    let path =
        std::env::var_os("DCA_REFERENCE_INPUT").expect("explicit private fixture path required");
    let path = Path::new(&path);
    let before = std::fs::read(path).unwrap();
    assert_eq!(
        dca_inspect::sha256(&before),
        "cd5bbe33e8b13ba92d7575d33b3ca8f895589f5ec5d87706f847f997cddc3ec9"
    );
    let report = inspect_path(path, &flag()).unwrap();
    assert_eq!(
        (
            report.archive_bytes,
            report.entries.len(),
            report.files,
            report.directories,
            report.expanded_bytes
        ),
        (1730085, 112, 108, 4, 3393880)
    );
    assert_eq!(
        report.manifest.applications,
        [
            "M-0083_A-0154-10-0000",
            "M-0083_A-0155-10-0000",
            "M-0083_A-0154-11-0000",
            "M-0083_A-0155-11-0000",
            "M-0083_A-0154-40-0000",
            "M-0083_A-0155-40-0000"
        ]
    );
    assert_eq!(report.manifest.assembly, "Mdt-DaliDCAPlus.dll");
    assert_eq!(report.actual_assembly, "MDT-DaliDcaPlus.dll");
    assert!(report.assembly_case_difference && report.manifest.is_64_bit_compatible);
    let primary = report
        .entries
        .iter()
        .find(|e| e.name == report.actual_assembly)
        .unwrap();
    assert_eq!(
        (primary.bytes, primary.sha256.as_str()),
        (
            452608,
            "4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d"
        )
    );
    assert_eq!(
        report.primary_byte_tokens,
        [
            ".NETFramework,Version=v4.6.1",
            "Knx.Ets.Sdk",
            "PresentationFramework",
            "WindowsBase",
            "IDeviceConfigurationAddIn"
        ]
    );
    assert!(report.config_byte_tokens.contains(&"Knx.Falcon".to_owned()));
    assert_eq!(std::fs::read(path).unwrap(), before);
}

#[test]
fn accepts_selected_utf8_bom_without_accepting_other_encodings() {
    let xml = manifest();
    let mut bytes = b"\xef\xbb\xbf".to_vec();
    bytes.extend_from_slice(xml.as_bytes());
    assert_eq!(
        parse_manifest(&bytes, &flag()),
        parse_manifest(xml.as_bytes(), &flag())
    );
    bytes[0] = 0xff;
    assert_eq!(parse_manifest(&bytes, &flag()), Err(Error::Manifest));
}

#[test]
fn rejects_malformed_attribute_text_and_declaration_ambiguity() {
    let xml = manifest();
    for bad in [
        xml.replace("owned.png", "bad<value"),
        xml.replace(
            " encoding=\"utf-8\"",
            " encoding=\"utf-8\" encoding=\"utf-16\"",
        ),
        format!(" \n{xml}"),
        xml.replace("version=\"1.0\"", "version=\"1.0\" unknown=\"yes\""),
    ] {
        assert!(parse_manifest(bad.as_bytes(), &flag()).is_err());
    }
}

#[test]
fn rejects_real_header_data_overlap_with_individually_valid_entries() {
    use std::io::Read;
    let inner = zip(&[("nested", b"x")], CompressionMethod::Stored);
    let local = &inner[..central(&inner)[0]];
    let mut bytes = zip(
        &[("outer", local), ("nested", b"x")],
        CompressionMethod::Stored,
    );
    let second = central(&bytes)[1];
    // The embedded local header is byte-identical to nested's ordinary header.
    put32(&mut bytes, second + 42, (30 + "outer".len()) as u32);
    let mut archive = zip::ZipArchive::new(Cursor::new(&bytes)).unwrap();
    for (index, expected) in [(0, local), (1, b"x".as_slice())] {
        let mut data = Vec::new();
        archive
            .by_index(index)
            .unwrap()
            .read_to_end(&mut data)
            .unwrap();
        assert_eq!(data, expected); // Both payloads and CRCs are valid in isolation.
    }
    assert!(archive.has_overlapping_entries().unwrap());
    assert_eq!(inspect(&bytes, &flag()), Err(Error::Integrity));
}

#[test]
fn bounds_xml_events_even_when_each_element_is_supported() {
    let many = "<HelpFile LanguageId=\"\" Path=\"\" Name=\"\"/>".repeat(256);
    let xml = manifest().replace("<HelpFiles>", &format!("<HelpFiles>{many}"));
    assert!(xml.len() < MAX_XML);
    assert_eq!(
        parse_manifest(xml.as_bytes(), &flag()),
        Err(Error::XmlLimit)
    );
}
