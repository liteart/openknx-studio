// SPDX-License-Identifier: Apache-2.0
//! Read-only installer refusal checks; no installation, repair or execution.
#[cfg(target_os = "macos")]
#[derive(Debug, PartialEq, Eq)]
struct Refusal {
    code: &'static str,
    path: &'static str,
}

#[cfg(target_os = "macos")]
fn destination_absent(path: &std::path::Path) -> Result<(), &'static str> {
    match std::fs::symlink_metadata(path) {
        Err(error) if error.kind() == std::io::ErrorKind::NotFound => Ok(()),
        Ok(_) => Err("destination_exists"),
        Err(_) => Err("destination_inspection_failed"),
    }
}

#[cfg(target_os = "macos")]
fn destinations(arguments: &[std::ffi::OsString]) -> Result<&'static [&'static str], Refusal> {
    match arguments {
        [argument] if argument == "--helpers" => Ok(&[
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator",
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator-browser",
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator-enrollment",
        ]),
        [argument] if argument == "--daemon" => {
            Ok(&["/Library/LaunchDaemons/org.openknxstudio.operator.plist"])
        }
        [argument] if argument == "--provisioner" => {
            Ok(&["/Library/PrivilegedHelperTools/org.openknxstudio.operator-provision"])
        }
        // An additive recovery package may only add its own fixed helper. It
        // does not replace any installed service or perform recovery itself.
        [argument] if argument == "--recovery" => {
            Ok(&["/Library/PrivilegedHelperTools/org.openknxstudio.operator-recover"])
        }
        [argument] if argument == "--service-upgrade" => Ok(&[
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator-service-upgrade",
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator-service-next",
        ]),
        _ => Err(Refusal {
            code: "arguments_rejected",
            path: "",
        }),
    }
}

#[cfg(target_os = "macos")]
fn check() -> Result<(), Refusal> {
    use std::{fs, os::unix::fs::MetadataExt, path::Path};
    let arguments: Vec<_> = std::env::args_os().skip(1).collect();
    let destinations = destinations(&arguments)?;
    if nix::unistd::geteuid().as_raw() != 0 || nix::unistd::getegid().as_raw() != 0 {
        return Err(Refusal {
            code: "root_wheel_required",
            path: "",
        });
    }
    // Component roots match these exact pre-existing modes. A package never
    // chooses a replacement mode for a differently configured host.
    for (path, mode) in [
        ("/Library", 0o755),
        ("/Library/PrivilegedHelperTools", 0o1755),
        ("/Library/LaunchDaemons", 0o755),
    ] {
        if task153_broker_core::macos_config::inspect_root_owned_directory(Path::new(path)).is_err()
        {
            return Err(Refusal {
                code: "parent_protection_rejected",
                path,
            });
        }
        let Ok(metadata) = fs::symlink_metadata(path) else {
            return Err(Refusal {
                code: "parent_inspection_failed",
                path,
            });
        };
        if metadata.gid() != 0 || metadata.mode() & 0o7777 != mode {
            return Err(Refusal {
                code: "parent_group_or_mode_mismatch",
                path,
            });
        }
    }
    for path in destinations {
        destination_absent(Path::new(path)).map_err(|code| Refusal { code, path })?;
    }
    Ok(())
}
fn main() {
    #[cfg(target_os = "macos")]
    match check() {
        Ok(()) => return,
        Err(refusal) => {
            eprintln!(
                "Installer preflight refused [{}]: {}. This preflight made no changes.",
                refusal.code, refusal.path
            );
            if refusal.code == "destination_exists" {
                eprintln!(
                    "Existing installed files are never overwritten. Compare the installed artifacts and package inventory before deciding whether installation is needed; do not delete files or retry blindly."
                );
            }
        }
    }
    #[cfg(not(target_os = "macos"))]
    eprintln!(
        "Installer preflight refused [platform_unsupported]. This preflight made no changes."
    );
    std::process::exit(1);
}

#[cfg(all(test, target_os = "macos"))]
mod tests {
    use super::*;
    use std::os::unix::fs::symlink;

    #[test]
    fn recovery_component_checks_only_its_fixed_new_helper() {
        assert_eq!(
            destinations(&["--recovery".into()]),
            Ok(&["/Library/PrivilegedHelperTools/org.openknxstudio.operator-recover"][..])
        );
        for arguments in [
            vec![],
            vec!["--recovery".into(), "extra".into()],
            vec!["/arbitrary/path".into()],
        ] {
            assert_eq!(
                destinations(&arguments),
                Err(Refusal {
                    code: "arguments_rejected",
                    path: ""
                })
            );
        }
    }

    #[test]
    fn destination_diagnostics_distinguish_missing_and_existing_objects_without_changes() {
        let directory = tempfile::tempdir().unwrap();
        let path = directory.path().join("destination");
        assert_eq!(destination_absent(&path), Ok(()));
        std::fs::write(&path, b"existing artifact").unwrap();
        assert_eq!(destination_absent(&path), Err("destination_exists"));
        let link = directory.path().join("link");
        symlink(&path, &link).unwrap();
        assert_eq!(destination_absent(&link), Err("destination_exists"));
        assert_eq!(
            destination_absent(directory.path()),
            Err("destination_exists")
        );
        assert_eq!(std::fs::read(&path).unwrap(), b"existing artifact");
    }
}
