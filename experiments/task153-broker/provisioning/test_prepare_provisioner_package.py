# SPDX-License-Identifier: Apache-2.0
"""Additive-package policy tests; no build, installation or root action."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location("prepare_provisioner", Path(__file__).with_name("prepare_provisioner_package.py"))
ADDON = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(ADDON)


class AdditivePolicyTests(unittest.TestCase):
    def test_payload_only_adds_fixed_provisioner_leaf(self):
        self.assertEqual(ADDON.COMPONENT, ("/Library/PrivilegedHelperTools", 0o1755,
                                         {"task153-operator-provision": "org.openknxstudio.operator-provision"}))
        self.assertNotIn("task153-operator-service", ADDON.BUILD_ARGUMENTS)
        self.assertNotIn("task153-operator-browser", ADDON.BUILD_ARGUMENTS)
        self.assertEqual(ADDON.BUILD_ARGUMENTS[-4:], ["--bin", "task153-operator-provision", "--bin", "task153-install-preflight"])

    def test_receipt_pin_fragment_uses_only_archived_provisioner(self):
        fragment = ADDON.make_pins({ADDON.LEAF: {"sha256": "12" * 32, "cdhash": "a" * 40}})
        self.assertEqual(fragment, {"provisioner": {"executable_sha256": [0x12] * 32,
                                                   "executable_cdhash": "a" * 40}})
        with self.assertRaises(KeyError):
            ADDON.make_pins({"other": {"sha256": "34" * 32, "cdhash": "b" * 40}})

    def test_uncommitted_compiled_source_is_rejected_before_build(self):
        source = ADDON.REPOSITORY / "rust-toolchain.toml"
        with patch.object(ADDON, "source_paths", return_value=[source]), \
                patch.object(ADDON.PACKAGE, "run", side_effect=["a" * 40 + "\n", " M rust-toolchain.toml\n"]):
            with self.assertRaisesRegex(ValueError, "must be committed"):
                ADDON.reviewed_source_snapshot()

    def test_source_snapshot_detects_content_changes_and_rejects_symlinks(self):
        with tempfile.TemporaryDirectory(dir="/private/tmp") as directory:
            root = Path(directory)
            file = root / "source.rs"; file.write_bytes(b"first")
            with patch.object(ADDON, "REPOSITORY", root):
                before = ADDON.snapshot([file]); file.write_bytes(b"second")
                self.assertNotEqual(before, ADDON.snapshot([file]))
                link = root / "link.rs"; link.symlink_to(file)
                with self.assertRaises(OSError):
                    ADDON.snapshot([link])


if __name__ == "__main__":
    unittest.main()
