// SPDX-License-Identifier: Apache-2.0
//! Explicit macOS operator diagnostic. No service installation or KNX access.

#[cfg(target_os = "macos")]
mod diagnostic;

fn main() {
    #[cfg(target_os = "macos")]
    if let Err(error) = diagnostic::run(std::env::args_os().skip(1).collect()) {
        eprintln!(
            "Operator diagnostic unavailable ({error:?}); no approval or device authorization issued."
        );
        std::process::exit(1);
    }
    #[cfg(not(target_os = "macos"))]
    {
        eprintln!("This diagnostic requires macOS; no service was started.");
        std::process::exit(1);
    }
}
