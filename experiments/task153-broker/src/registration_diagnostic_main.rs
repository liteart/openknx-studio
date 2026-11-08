// SPDX-License-Identifier: Apache-2.0
fn main() {
    #[cfg(target_os = "macos")]
    if let Err(error) = task153_broker_core::operator_browser::run_registration_diagnostic(
        std::env::args_os().skip(1).collect(),
    ) {
        eprintln!(
            "Synthetic registration diagnostic unavailable ({error:?}). No protected enrollment or KNX operation was requested."
        );
        std::process::exit(1);
    }
    #[cfg(not(target_os = "macos"))]
    {
        eprintln!("Synthetic registration diagnostic unavailable on this platform.");
        std::process::exit(1);
    }
}
