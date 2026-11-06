# SPDX-License-Identifier: Apache-2.0
"""Focused regressions for the installer defects; no installation or root use."""
import importlib.util
import json
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
                    with self.assertRaisesRegex(ValueError, "metadata rejected"):
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

    def test_service_override_is_bound_to_original_exact_export_artifact(self):
        with tempfile.TemporaryDirectory(dir="/private/tmp") as directory:
            root = Path(directory)
            (root / "artifacts").mkdir()
            binary = root / "artifacts/service"
            binary.write_bytes(b"synthetic")
            (root / "target/debug").mkdir(parents=True)
            (root / "target/debug/task153-operator-service").write_bytes(b"synthetic")
            evidence = root / "result.json"
            artifact = {"path": "artifacts/service", "sha256": PACKAGE.sha(binary),
                        "size_bytes": binary.stat().st_size, "host_qualification": "UNCONSTRUCTIBLE",
                        "service_started": "NOT RUN", "installation": "NOT RUN", "hardware": "NOT RUN"}
            evidence.write_text(json.dumps({"operator_service_artifact": artifact, "fork_revision": "synthetic"}))
            with patch.object(PACKAGE, "run", return_value="com.apple.provenance\n"), patch.object(PACKAGE, "validate_export", return_value={}):
                selected, record = PACKAGE.verified_service_input(binary, evidence)
                self.assertEqual(selected, binary)
                self.assertEqual(record["artifact_sha256"], PACKAGE.sha(binary))
                binary.write_bytes(b"modified")
                with self.assertRaisesRegex(ValueError, "artifact mismatch"):
                    PACKAGE.verified_service_input(binary, evidence)
            with self.assertRaisesRegex(ValueError, "supplied together"):
                PACKAGE.verified_service_input(binary, None)
            with patch.object(PACKAGE, "run", return_value="com.apple.quarantine\n"), patch.object(PACKAGE, "validate_export", return_value={}):
                with self.assertRaisesRegex(ValueError, "metadata rejected"):
                    PACKAGE.verified_service_input(binary, evidence)

    def test_failed_missing_or_wrong_revision_evidence_is_rejected(self):
        valid = {"status": "PASS; exact-source development integration, synthetic loopback only",
                 "fork_revision": PACKAGE.PINNED_FORK_REVISION, "exacl_revision": PACKAGE.PINNED_EXACL_REVISION,
                 "fork_features": ["dpt"], "hardware": "NOT RUN", "installed_boundary": "NOT RUN"}
        for field, value in (("status", "FAIL"), ("status", None), ("fork_revision", "arbitrary"),
                             ("exacl_revision", "arbitrary"), ("fork_features", ["secure"])):
            record = dict(valid); record[field] = value
            with self.assertRaisesRegex(ValueError, "outcome or pinned revision"):
                PACKAGE.validate_export(record, Path("/private/tmp/unused"))
        valid["commands"] = []
        with self.assertRaisesRegex(ValueError, "command sequence"):
            PACKAGE.validate_export(valid, Path("/private/tmp/unused"))

    def test_evidence_duplicates_and_symlink_ancestors_are_rejected(self):
        with self.assertRaisesRegex(ValueError, "duplicate evidence key"):
            json.loads('{"status":"FAIL","status":"PASS"}', object_pairs_hook=PACKAGE.unique_object)
        with tempfile.TemporaryDirectory(dir="/private/tmp") as directory:
            root = Path(directory)
            (root / "real").mkdir()
            (root / "real/record").write_bytes(b"synthetic")
            (root / "alias").symlink_to("real", target_is_directory=True)
            with self.assertRaises(OSError):
                PACKAGE.read_snapshot(root / "alias/record", 1024)
            self.assertEqual(PACKAGE.read_snapshot(root / "real/record", 1024), b"synthetic")

    def test_service_changed_after_verification_or_during_copy_is_never_signed(self):
        with tempfile.TemporaryDirectory(dir="/private/tmp") as directory:
            root = Path(directory)
            source, target = root / "source", root / "target"
            original = b"\xcf\xfa\xed\xfe" + b"reviewed service"
            source.write_bytes(original)
            verified_sha = PACKAGE.sha(source)
            source.write_bytes(b"\xcf\xfa\xed\xfe" + b"changed after verification")
            with patch.object(PACKAGE, "run", return_value="com.apple.provenance\n") as run:
                with self.assertRaisesRegex(ValueError, "changed after evidence verification"):
                    PACKAGE.copy_binary(source, target, verified_sha)
                self.assertFalse(target.exists())
                self.assertTrue(all(call.args[0][0] == "/usr/bin/xattr" for call in run.call_args_list))
            source.write_bytes(original)
            with patch.object(PACKAGE, "run", return_value="com.apple.provenance\n") as run, \
                    patch.object(PACKAGE.shutil, "copyfile", side_effect=lambda _source, destination: destination.write_bytes(b"substituted copy")):
                with self.assertRaisesRegex(ValueError, "changed while copying"):
                    PACKAGE.copy_binary(source, target, verified_sha)
                self.assertTrue(all(call.args[0][0] == "/usr/bin/xattr" for call in run.call_args_list))

    def test_new_security_metadata_during_copy_is_rejected_before_signing(self):
        with tempfile.TemporaryDirectory(dir="/private/tmp") as directory:
            root = Path(directory)
            source, target = root / "source", root / "target"
            source.write_bytes(b"\xcf\xfa\xed\xfe" + b"synthetic")
            with patch.object(PACKAGE, "run", side_effect=["com.apple.provenance\n", "com.apple.quarantine\n"]) as run:
                with self.assertRaisesRegex(ValueError, "metadata rejected"):
                    PACKAGE.copy_binary(source, target, PACKAGE.sha(source))
                self.assertTrue(all(call.args[0][0] == "/usr/bin/xattr" for call in run.call_args_list))


if __name__ == "__main__":
    unittest.main()
