// SPDX-License-Identifier: Apache-2.0
fn main() {
    #[cfg(target_os = "macos")]
    if let Err(error) = task153_broker_core::operator_browser::run_enrollment_bootstrap(
        std::env::args_os().skip(1).collect(),
    ) {
        eprintln!("Enrollment bootstrap unavailable ({error:?}); no device authorization issued.");
        std::process::exit(1);
    }
    #[cfg(not(target_os = "macos"))]
    {
        eprintln!("Enrollment bootstrap unavailable on this platform.");
        std::process::exit(1);
    }
}
