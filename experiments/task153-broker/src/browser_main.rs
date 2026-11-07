// SPDX-License-Identifier: Apache-2.0
fn main() {
    #[cfg(target_os = "macos")]
    if let Err(error) =
        task153_broker_core::operator_browser::run(std::env::args_os().skip(1).collect())
    {
        // No request, token, credential, bootstrap path or service reply is logged.
        eprintln!(
            "Operator browser unavailable ({error:?}); operation status may be unknown. Inspect protected state; do not retry automatically."
        );
        std::process::exit(1);
    }
    #[cfg(not(target_os = "macos"))]
    {
        eprintln!("Operator browser unavailable on this platform.");
        std::process::exit(1);
    }
}
