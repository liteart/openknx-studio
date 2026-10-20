#![forbid(unsafe_code)]

use std::io::{self, Write};

fn main() -> io::Result<()> {
    writeln!(
        io::stdout().lock(),
        "toolchain-smoke offline=true protocol=none"
    )
}
