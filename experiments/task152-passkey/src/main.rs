// SPDX-License-Identifier: Apache-2.0
//! Explicitly invoked temporary localhost development ceremony; no device code.

mod browser_server;

use std::path::PathBuf;
use std::process::ExitCode;

fn main() -> ExitCode {
    let arguments: Vec<_> = std::env::args_os().skip(1).collect();
    if arguments.len() != 3
        || arguments[0] != "--development-only"
        || arguments[1] != "--journal-dir"
    {
        eprintln!(
            "Usage: task152-browser-demo --development-only --journal-dir <existing-directory>"
        );
        return ExitCode::FAILURE;
    }
    let root = match std::fs::canonicalize(PathBuf::from(&arguments[2])) {
        Ok(root) => root,
        Err(_) => {
            eprintln!("Development journal directory is unavailable.");
            return ExitCode::FAILURE;
        }
    };
    if browser_server::run(&root).is_err() {
        eprintln!(
            "The local development test could not continue. No device authorization was issued."
        );
        return ExitCode::FAILURE;
    }
    ExitCode::SUCCESS
}
