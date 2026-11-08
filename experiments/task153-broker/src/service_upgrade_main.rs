// SPDX-License-Identifier: Apache-2.0
fn main() {
    #[cfg(target_os = "macos")]
    match task153_broker_core::service_upgrade::run(std::env::args_os().skip(1).collect()) {
        Ok(()) => return,
        Err(error) => eprintln!(
            "Service upgrade refused or outcome uncertain ({error:?}); inspect the fixed preservation paths before any retry. No service or device operation was started."
        ),
    }
    #[cfg(not(target_os = "macos"))]
    eprintln!("Service upgrade unavailable on this platform.");
    std::process::exit(1);
}
