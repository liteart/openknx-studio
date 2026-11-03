#![forbid(unsafe_code)]

use std::{path::Path, process::ExitCode, sync::atomic::AtomicBool};
fn main() -> ExitCode {
    let mut args = std::env::args_os().skip(1);
    let Some(path) = args.next() else {
        eprintln!("dca_inspect.usage: Usage: dca-inspect <local.etsapp>");
        return ExitCode::from(2);
    };
    if args.next().is_some() {
        eprintln!("dca_inspect.usage: Usage: dca-inspect <local.etsapp>");
        return ExitCode::from(2);
    }
    match dca_inspect::inspect_path(Path::new(&path), &AtomicBool::new(false)) {
        Ok(report) => {
            print!("{report}");
            ExitCode::SUCCESS
        }
        Err(error) => {
            eprintln!("{error}");
            ExitCode::FAILURE
        }
    }
}
