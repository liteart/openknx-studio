"""Focused offline regressions for immutable exports and optimization-proof checks."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

import run_adapter_check as runner
import validate_actor_logs as validator


class RunnerTests(unittest.TestCase):
    def test_export_uses_raw_pinned_blobs_instead_of_modified_worktree(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            checkout = root / "checkout"
            checkout.mkdir()
            (checkout / "source.rs").write_text("unreviewed hidden-index modification")
            (checkout / ".cargo").mkdir()
            (checkout / ".cargo/config.toml").write_text("ignored build override")
            blob = b"reviewed pinned source\n"
            oid = hashlib.sha1(b"blob " + str(len(blob)).encode() + b"\0" + blob).hexdigest()
            metadata = f"100755 blob {oid} {len(blob)}\tsource.rs\0".encode()
            def git(path, *args):
                self.assertEqual(path, checkout)
                if args[0] == "ls-tree":
                    self.assertEqual(args[-1], runner.FORK_REVISION)
                    return metadata
                self.assertEqual(args, ("cat-file", "blob", oid))
                return blob
            with patch.object(runner, "git_bytes", side_effect=git):
                runner.export_pinned_tree(checkout, root / "export")
            self.assertEqual((root / "export/source.rs").read_bytes(), blob)
            self.assertEqual((root / "export/source.rs").stat().st_mode & 0o777, 0o755)
            self.assertFalse((root / "export/.cargo").exists())

    def test_export_rejects_symlink_mode_and_blob_mismatch(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for index, record in enumerate((b"120000 blob " + b"0" * 40 + b" 1\tlink\0", b"100644 blob " + b"0" * 40 + b" 1\tsource.rs\0")):
                with patch.object(runner, "git_bytes", side_effect=[record, b"x"]):
                    with self.assertRaises(ValueError):
                        runner.export_pinned_tree(root, root / str(index))

    def test_build_environment_excludes_cargo_and_rust_overrides(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            cache = root / "cache"
            (cache / "registry").mkdir(parents=True)
            work = root / "work"
            work.mkdir()
            with patch.dict(os.environ, {"CARGO_HOME": str(cache), "RUSTFLAGS": "unreviewed", "RUSTC_WRAPPER": "unreviewed", "CARGO_BUILD_RUSTC": "unreviewed", "PYTHONOPTIMIZE": "1"}):
                env = runner.isolated_build_env(work)
            for key in ("RUSTFLAGS", "RUSTC_WRAPPER", "CARGO_BUILD_RUSTC", "PYTHONOPTIMIZE"):
                self.assertNotIn(key, env)
            self.assertEqual(Path(env["CARGO_HOME"]), work / "cargo-home")
            self.assertFalse((work / "cargo-home/config.toml").exists())


class ValidatorTests(unittest.TestCase):
    def fixture(self, root):
        for case, (reads, writes, final, held) in validator.EXPECTED.items():
            events = ["GATEWAY_CONNECTED synthetic=true", "DEVICE_CONNECTED"]
            operations = [("read", value) for value in reads[:2]]
            if case == "normal":
                operations += [("write", "04"), ("read", "04"), ("write", "00"), ("read", "00")]
            elif writes:
                operations += [("write", "04")]
            ordinal = 0
            for operation, value in operations:
                if operation == "read":
                    events += ["DESCRIPTOR_READ mask=0705", f"PROPERTY_READ value={value}"]
                else:
                    ordinal += 1
                    events.append(f"WRITE_APPLIED ordinal={ordinal} value={value}")
            if held:
                events.append(f"{held} ordinal=1 value=04")
            events += ["DEVICE_DISCONNECTED", "GATEWAY_DISCONNECTED device_open=false"]
            lines = ["SIMULATOR_READY endpoint=127.0.0.1:12345 synthetic=true"]
            lines += [f"SIM_EVENT id={index + 1} kind={event}" for index, event in enumerate(events)]
            lines += [f"SIMULATOR_DONE reason=gateway_closed value={final} writes={len(writes)} device_open=false synthetic=true"]
            (root / f"e2e-{case}.stderr.log").write_text("")
            (root / f"e2e-{case}.sim.log").write_text("\n".join(lines) + "\n")
            journal = root / f"e2e-{case}-journal-fixture"
            journal.mkdir()
            suffixes = ["intent", "consumed-grant"] + (["complete", "cleanup"] if case == "normal" else ["unresolved-route"])
            for index, suffix in enumerate(suffixes):
                (journal / f"{index:064x}-{suffix}.json").write_text(json.dumps({}))

    def test_corrupted_log_is_rejected_with_python_optimization(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.fixture(root)
            command = [sys.executable, "-O", str(Path(validator.__file__)), str(root)]
            good = subprocess.run(command, capture_output=True, text=True, timeout=10)
            self.assertEqual(good.returncode, 0, good.stderr)
            log = root / "e2e-normal.sim.log"
            log.write_text(log.read_text().replace("PROPERTY_READ value=00", "PROPERTY_READ value=ff", 1))
            bad = subprocess.run(command, capture_output=True, text=True, timeout=10)
            self.assertNotEqual(bad.returncode, 0)
            self.assertIn("property read values", bad.stderr)
            self.assertNotIn('"status": "PASS"', bad.stdout)

    def test_oversized_input_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            log = Path(directory) / "large.log"
            log.write_bytes(b"x" * 65_537)
            with self.assertRaisesRegex(ValueError, "oversized"):
                validator.bounded_read(log)


if __name__ == "__main__":
    unittest.main()
