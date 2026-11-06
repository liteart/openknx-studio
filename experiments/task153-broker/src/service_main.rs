// SPDX-License-Identifier: Apache-2.0
fn main() {
    #[cfg(target_os = "macos")]
    if let Err(error) =
        task153_broker_core::macos_service::run(std::env::args_os().skip(1).collect())
    {
        eprintln!("Operator service unavailable ({error:?}); no device authorization issued.");
        std::process::exit(1);
    }
    #[cfg(not(target_os = "macos"))]
    {
        eprintln!("Operator service unavailable on this platform.");
        std::process::exit(1);
    }
}
