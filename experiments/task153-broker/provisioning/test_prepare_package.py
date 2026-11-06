# SPDX-License-Identifier: Apache-2.0
"""Focused regressions for the installer defects; no installation or root use."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location("prepare_package", Path(__file__).with_name("prepare_package.py"))
PACKAGE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PACKAGE)


class PackagePolicyTests(unittest.TestCase):
    def test_security_metadata_is_rejected_before_copy_or_sign(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "input"
            source.write_bytes(b"\xcf\xfa\xed\xfe" + b"synthetic")
            target = Path(directory) / "output"
            for attribute in ("com.apple.quarantine", "com.apple.macl", "unknown.security"):
                with patch.object(PACKAGE, "run", return_value=attribute + "\n"):
                    with self.assertRaisesRegex(ValueError, "metadata rejected before copy"):
                        PACKAGE.copy_binary(source, target)
                self.assertFalse(target.exists())

    def test_provenance_is_recorded_without_mutating_input(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "input"
            source.write_bytes(b"synthetic")
            with patch.object(PACKAGE, "run", return_value="com.apple.provenance\n") as run:
                result = PACKAGE.checked_input(source)
            self.assertEqual(result["attribute_names"], ["com.apple.provenance"])
            self.assertEqual(source.read_bytes(), b"synthetic")
            self.assertEqual(run.call_args_list[0].args[0], ["/usr/bin/xattr", "-s", str(source)])
            self.assertEqual(len(run.call_args_list), 1)

    def test_bom_filter_rejects_unexpected_payload_but_discards_metadata_companions(self):
        original = ".\t41755\t0/0\n./helper\t100755\t0/0\t1\t123\n./._helper\t100755\t0/0\t0\t0\n"
        filtered = PACKAGE.bom_rows(original, {".", "./helper"})
        self.assertNotIn("._", filtered)
        self.assertIn("41755", filtered)
        with self.assertRaisesRegex(ValueError, "unexpected initial BOM"):
            PACKAGE.bom_rows(original + "./surprise\t100755\t0/0\t1\t123\n", {".", "./helper"})
        with self.assertRaisesRegex(ValueError, "missing BOM"):
            PACKAGE.bom_rows(".\t41755\t0/0\n", {".", "./missing"})

    def test_components_have_only_leaf_install_roots_with_explicit_parent_modes(self):
        self.assertEqual(PACKAGE.COMPONENTS["helpers"][:2], ("/Library/PrivilegedHelperTools", 0o1755))
        self.assertEqual(PACKAGE.COMPONENTS["daemon"][:2], ("/Library/LaunchDaemons", 0o755))
        self.assertEqual(len(PACKAGE.COMPONENTS), 2)

    def test_distribution_architectures_are_intersection_of_actual_executables(self):
        components = {"example": {"files": {
            "one": {"cdhash": "one", "architectures": ["arm64", "x86_64"]},
            "two": {"cdhash": "two", "architectures": ["arm64"]},
        }, "scripts": {"preflight": {"cdhash": "three", "architectures": ["arm64"]}}}}
        self.assertEqual(PACKAGE.product_architectures(components), ["arm64"])
        components["example"]["scripts"]["preflight"]["architectures"] = ["x86_64"]
        with self.assertRaisesRegex(ValueError, "no common executable architecture"):
            PACKAGE.product_architectures(components)


if __name__ == "__main__":
    unittest.main()
