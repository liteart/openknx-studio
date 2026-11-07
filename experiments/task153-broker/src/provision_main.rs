// SPDX-License-Identifier: Apache-2.0
fn main() {
    #[cfg(target_os = "macos")]
    match task153_broker_core::provisioning::run(std::env::args_os().skip(1).collect()) {
        Ok(()) => return,
        Err(error) => eprintln!(
            "Provisioning refused or outcome uncertain ({error:?}); inspect fixed protected paths before retrying. No device operation is available."
        ),
    }
    #[cfg(not(target_os = "macos"))]
    eprintln!("Provisioning unavailable on this platform.");
    std::process::exit(1);
}
