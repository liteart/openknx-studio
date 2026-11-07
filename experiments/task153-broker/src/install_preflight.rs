// SPDX-License-Identifier: Apache-2.0
//! Read-only installer refusal checks; no installation, repair or execution.
#[cfg(target_os = "macos")]
fn allowed() -> bool {
    use std::{fs, os::unix::fs::MetadataExt, path::Path};
    let arguments: Vec<_> = std::env::args_os().skip(1).collect();
    let destinations: &[&str] = match arguments.as_slice() {
        [argument] if argument == "--helpers" => &[
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator",
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator-browser",
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator-enrollment",
        ],
        [argument] if argument == "--daemon" => {
            &["/Library/LaunchDaemons/org.openknxstudio.operator.plist"]
        }
        _ => return false,
    };
    if nix::unistd::geteuid().as_raw() != 0 || nix::unistd::getegid().as_raw() != 0 {
        return false;
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
            return false;
        }
        let Ok(metadata) = fs::symlink_metadata(path) else {
            return false;
        };
        if metadata.gid() != 0 || metadata.mode() & 0o7777 != mode {
            return false;
        }
    }
    for path in destinations {
        match fs::symlink_metadata(path) {
            Err(error) if error.kind() == std::io::ErrorKind::NotFound => (),
            _ => return false,
        }
    }
    true
}
fn main() {
    #[cfg(target_os = "macos")]
    if allowed() {
        return;
    }
    eprintln!(
        "Installer preflight refused; protected parents and absent destinations are required. This preflight made no changes."
    );
    std::process::exit(1);
}
