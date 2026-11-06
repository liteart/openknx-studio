#!/usr/bin/env python3
"""Build/test the development adapter against an explicitly supplied local fork.

Cargo stays offline. Only the fork-owned synthetic loopback peer runs; this is
not a device executor, approval tool or release dependency configuration.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
from pathlib import PurePosixPath
import shutil
import subprocess
import sys
import tempfile

FORK_REVISION = "4d296afb0906d17b6724b56c912f2e9e3f6680a5"
SOURCE = Path(__file__).resolve().parent
ROOT = SOURCE.parents[1]
INPUTS = ("guard.rs", "dispatcher.rs", "dispatcher_tests.rs", "knust_adapter.rs", "actor_tests.rs")


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def git_bytes(path, *args):
    env = {key: os.environ[key] for key in ("PATH", "HOME", "SystemRoot", "WINDIR") if key in os.environ}
    env.update(GIT_CONFIG_NOSYSTEM="1", GIT_CONFIG_GLOBAL=os.devnull)
    return subprocess.check_output(
        ["git", "--no-replace-objects", "-c", "core.fsmonitor=false", "-C", str(path), *args],
        env=env, timeout=10,
    )


def inspect_fork(path):
    """Preflight only: never build or start a peer from another revision."""
    def git(*args):
        return git_bytes(path, *args).decode().strip()
    if Path(git("rev-parse", "--show-toplevel")).resolve() != path:
        raise ValueError("fork path must identify its repository root")
    if git("rev-parse", "HEAD") != FORK_REVISION:
        raise ValueError("fork revision does not match the exact development pin")
    if git("status", "--porcelain", "--untracked-files=normal"):
        raise ValueError("fork must have a clean worktree")
    for name in ("Cargo.toml", "Cargo.lock", "examples/connected_management_simulator.rs"):
        if not (path / name).is_file():
            raise ValueError(f"missing required fork file: {name}")


def export_pinned_tree(fork, destination, revision=FORK_REVISION):
    """Export raw pinned blobs, never worktree/index files or archive filters."""
    tree = git_bytes(fork, "ls-tree", "-r", "-l", "-z", "--full-tree", revision)
    if len(tree) > 1_048_576:
        raise ValueError("pinned tree metadata exceeds the export bound")
    entries = tree.rstrip(b"\0").split(b"\0")
    if len(entries) > 4096:
        raise ValueError("pinned tree file count exceeds the export bound")
    destination.mkdir()
    hashes = {}
    total = 0
    for entry in entries:
        header, raw_name = entry.split(b"\t", 1)
        mode, kind, oid, size = header.split()
        name = raw_name.decode("utf-8")
        path = PurePosixPath(name)
        if (mode not in (b"100644", b"100755") or kind != b"blob"
                or path.is_absolute() or path.as_posix() != name
                or any(part in (".", "..", ".git") for part in path.parts)
                or "\\" in name or ":" in name):
            raise ValueError("unsupported pinned tree path or mode")
        if name in (".cargo/config", ".cargo/config.toml"):
            raise ValueError("pinned build configuration requires separate review")
        length = int(size)
        total += length
        if length > 4_194_304 or total > 16_777_216:
            raise ValueError("pinned tree content exceeds the export bound")
        blob = git_bytes(fork, "cat-file", "blob", oid.decode("ascii"))
        actual = hashlib.sha1(b"blob " + str(length).encode() + b"\0" + blob).hexdigest()
        if len(blob) != length or actual.encode("ascii") != oid:
            raise ValueError("pinned Git blob hash/length mismatch")
        target = destination.joinpath(*path.parts)
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(blob)
        target.chmod(0o755 if mode == b"100755" else 0o644)
        hashes[name] = {"git_blob": oid.decode(), "mode": mode.decode(), "sha256": digest(target)}
    return hashes


def isolated_build_env(work):
    # Registry caches only: never copy user config, credentials or Git checkout
    # caches. Do not inherit CARGO_*, RUSTFLAGS, wrappers, compilers or loaders.
    old_home = Path(os.environ.get("CARGO_HOME", str(Path.home() / ".cargo")))
    cargo_home = work / "cargo-home"
    cargo_home.mkdir()
    registry = old_home / "registry"
    if not registry.is_dir():
        raise ValueError("cached Cargo registry is required for the offline check")
    (cargo_home / "registry").symlink_to(registry.resolve(), target_is_directory=True)
    for ancestor in (work, *work.parents):
        if any((ancestor / ".cargo" / name).exists() for name in ("config", "config.toml")):
            raise ValueError("ancestor Cargo configuration prevents an isolated check")
    env = {key: os.environ[key] for key in ("PATH", "HOME", "RUSTUP_HOME", "SystemRoot", "WINDIR") if key in os.environ}
    env.update(CARGO_HOME=str(cargo_home), CARGO_NET_OFFLINE="true",
               PYTHONDONTWRITEBYTECODE="1",
               CARGO_TARGET_DIR=str(work / "target"), RUSTUP_TOOLCHAIN="1.99.1",
               TASK020_TEST_ARTIFACT_DIR=str(work),
               TASK020_TEST_SIMULATOR_BIN=str(work / "target/debug/examples" / ("connected_management_simulator.exe" if os.name == "nt" else "connected_management_simulator")))
    return env


def run_check(fork):
    inspect_fork(fork)
    temp_root = Path(tempfile.gettempdir()).resolve()
    if temp_root == ROOT or ROOT in temp_root.parents:
        raise ValueError("temporary files must be outside the editor repository")
    work = Path(tempfile.mkdtemp(prefix="task020-adapter-", dir=temp_root))
    print(f"Development artifacts: {work}", flush=True)
    report = {
        "fork_revision": FORK_REVISION,
        "status": "IN PROGRESS",
        "hardware": "NOT RUN",
        "external_network": "NOT RUN",
        "production_approval": "ABSENT; grants are cfg(test) synthetic fixtures only",
        "source_sha256": {name: digest(SOURCE / name) for name in INPUTS},
        "commands": [],
    }
    def record():
        (work / "result.json").write_text(json.dumps(report, indent=2) + "\n")

    def execute(args, cwd=work):
        log = work / f"command-{len(report['commands']):02d}.log"
        with log.open("wb") as output:
            process = subprocess.run(args, cwd=cwd, env=env, stdout=output,
                                     stderr=subprocess.STDOUT, timeout=180)
        report["commands"].append({"args": args, "exit": process.returncode, "log": log.name})
        record()
        if process.returncode:
            raise RuntimeError(f"check failed ({process.returncode}); see {log}")
        return log.read_text()

    try:
        env = isolated_build_env(work)
        execute([sys.executable, str(SOURCE / "test_adapter_check.py")])
        exported = work / "pinned-fork"
        report["exported_files"] = export_pinned_tree(fork, exported)
        report["fork_simulator_sha256"] = digest(exported / "examples/connected_management_simulator.rs")
        report["rustc"] = execute(["rustc", "-vV"]).strip()
        if not report["rustc"].startswith("rustc 1.99.1 "):
            raise ValueError("the pinned Rust 1.99.1 toolchain is required")
        host = next(line.removeprefix("host: ") for line in report["rustc"].splitlines() if line.startswith("host: "))
        for name in INPUTS:
            shutil.copyfile(SOURCE / name, work / name)
        # Exact copies; the only core addition is this visible cfg(test)
        # descendant module. It can create synthetic grants inside private
        # tests, without opening the production approval type/API.
        with (work / "guard.rs").open("a") as guard:
            guard.write('\n#[cfg(test)]\n#[path = "actor_tests.rs"]\nmod actor_tests;\n')
        (work / "lib.rs").write_text('#![allow(dead_code)]\nmod guard;\nmod knust_adapter;\n')
        manifest = '''[package]
name = "task020-development-adapter-check"
version = "0.0.0"
edition = "2024"
rust-version = "1.99.1"
publish = false
[lib]
path = "lib.rs"
[dependencies]
serde = { version = "=1.0.229", features = ["derive"] }
serde_json = "=1.0.154"
sha2 = { version = "=0.11.0", default-features = false }
tokio = { version = "=1.53.1", default-features = false, features = ["rt", "sync", "time", "macros"] }
knust = { path = FORK_PATH, default-features = false, features = ["dpt"] }
[dev-dependencies]
tempfile = "=3.24.0"
'''
        # JSON strings also quote ordinary UTF-8 paths in TOML. Reject control
        # characters rather than permitting a generated dependency injection.
        if any(ord(char) < 32 or ord(char) == 127 for char in str(exported)):
            raise ValueError("fork path contains unsupported control characters")
        (work / "Cargo.toml").write_text(manifest.replace("FORK_PATH", json.dumps(str(exported), ensure_ascii=False)))
        # Preserve cached/yanked transitive resolution from the exact fork.
        # Only this temporary lock is adapted for the development harness.
        shutil.copyfile(exported / "Cargo.lock", work / "Cargo.lock")
        execute(["cargo", "check", "--offline", "--lib"])
        execute(["cargo", "build", "--locked", "--offline", "--no-default-features", "--features", "dpt", "--example", "connected_management_simulator"], exported)
        # Inspect the current host graph, not uncached unrelated target crates.
        metadata = json.loads(execute(["cargo", "metadata", "--locked", "--offline", "--format-version", "1", "--filter-platform", host]))
        knust_ids = [package["id"] for package in metadata["packages"] if package["name"] == "knust"]
        features = [node["features"] for node in metadata["resolve"]["nodes"] if node["id"] in knust_ids]
        if features != [["dpt"]]:
            raise ValueError("unexpected fork features; Secure must remain disabled")
        report["fork_features"] = features[0]
        execute(["cargo", "test", "--locked", "--offline", "--lib", "--", "--test-threads=1"])
        execute(["cargo", "clippy", "--locked", "--offline", "--lib", "--tests", "--", "-D", "warnings"])
        execute(["rustfmt", "--edition", "2024", "--check", "actor_tests.rs", "knust_adapter.rs", "lib.rs", "guard.rs"])
        execute([sys.executable, str(SOURCE / "validate_actor_logs.py"), str(work)])
        inspect_fork(fork)
        report["temporary_lock_sha256"] = digest(work / "Cargo.lock")
        report["status"] = "PASS; synthetic loopback only, independent review pending"
        record()
        print(report["status"], flush=True)
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        report["status"] = "FAIL"
        report["error"] = str(error)
        record()
        raise


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fork-path", required=True, type=Path,
                        help="clean local fork checkout at the exact documented revision")
    args = parser.parse_args()
    try:
        run_check(args.fork_path.resolve(strict=True))
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        parser.exit(1, f"Development check failed: {error}\n")


if __name__ == "__main__":
    main()
