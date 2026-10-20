"""Synthetic offline regressions for the Task-004 development input inventory."""

import hashlib
import json
from pathlib import Path
import tempfile
import unittest

from check_reference_inputs import INPUT_KINDS, MAX_FILE_BYTES, MAX_MANIFEST_BYTES, validate


class ReferenceInputChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="knx-reference-inputs-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "editor"
        (self.root / "config").mkdir(parents=True)
        self.items = [{
            "id": identity, "kind": kind, "status": "Missing",
            "identity": "UNVERIFIED: synthetic reference",
            "owner": "Project maintainer", "source": "Synthetic test fixture",
            "evidence": "No actual hardware or vendor input", "missing_reason": "Not supplied",
        } for identity, kind in INPUT_KINDS.items()]
        self.manifest = self.root / "config/reference-inputs.toml"

    def write_manifest(self):
        lines = ["schema_version = 1"]
        for item in self.items:
            lines.append("\n[[inputs]]")
            for key, value in item.items():
                lines.append(f"{key} = {json.dumps(value)}")
        self.manifest.write_text("\n".join(lines) + "\n", encoding="utf-8")

    def available_file(self):
        path = self.root / "private/fixtures/synthetic.bin"
        path.parent.mkdir(parents=True)
        path.write_bytes(b"synthetic")
        item = self.items[0]
        item.pop("missing_reason")
        item.update(status="Available", identity=path.name,
                    path="private/fixtures/synthetic.bin", bytes=9,
                    sha256=hashlib.sha256(b"synthetic").hexdigest())
        return path, item

    def test_complete_inventory_with_missing_inputs_is_not_qualification(self):
        self.write_manifest()
        self.assertEqual(validate(self.root), [])
        errors = validate(self.root, ("isolated-bench", "human-write-grant"))
        self.assertIn("isolated-bench: required input is Missing", errors)
        self.assertIn("human-write-grant: required input is Missing", errors)

    def test_available_file_hash_and_size_are_checked(self):
        self.available_file()
        self.write_manifest()
        self.assertEqual(validate(self.root, ("dali-dca",)), [])

    def test_same_size_corruption_is_rejected(self):
        path, _ = self.available_file()
        path.write_bytes(b"corrupted")
        self.write_manifest()
        self.assertIn("dali-dca: SHA-256 mismatch", validate(self.root))

    def test_missing_available_file_and_wrong_size_are_rejected(self):
        path, _ = self.available_file()
        self.write_manifest()
        path.write_bytes(b"short")
        self.assertIn("dali-dca: file size mismatch", validate(self.root))
        path.unlink()
        self.assertTrue(any("cannot read available input" in e for e in validate(self.root)))

    def test_unknown_required_input_is_rejected(self):
        self.write_manifest()
        self.assertEqual(validate(self.root, ("not-an-input",)),
                         ["not-an-input: unknown required input"])

    def test_manifest_requires_every_input_once(self):
        self.items.pop()
        self.write_manifest()
        self.assertIn("knxproj: missing inventory entry", validate(self.root))
        self.items.append(self.items[0].copy())
        self.write_manifest()
        self.assertIn("dali-dca: duplicate input ID", validate(self.root))

    def test_missing_reason_owner_and_status_are_required(self):
        for field, value in (("missing_reason", ""), ("owner", ""), ("status", ["Available"])):
            with self.subTest(field=field):
                original = self.items[0][field]
                self.items[0][field] = value
                self.write_manifest()
                self.assertTrue(validate(self.root))
                self.items[0][field] = original

    def test_traversal_absolute_and_nonprivate_paths_are_rejected(self):
        _, item = self.available_file()
        for path in ("private/fixtures/../secret.bin", "/tmp/secret.bin",
                     "docs/synthetic.bin", "private\\fixtures\\synthetic.bin",
                     "private/fixtures/C:synthetic.bin", "private/fixtures/\x00.bin"):
            with self.subTest(path=path):
                item["path"] = path
                self.write_manifest()
                self.assertTrue(any("path" in e for e in validate(self.root)))

    def test_symlink_file_and_parent_escape_are_rejected(self):
        path, item = self.available_file()
        outside = Path(self.temp.name) / "outside.bin"
        outside.write_bytes(b"synthetic")
        path.unlink()
        path.symlink_to(outside)
        self.write_manifest()
        self.assertIn("dali-dca: symlink file paths are not accepted", validate(self.root))
        path.unlink()
        link = path.parent / "external"
        link.symlink_to(outside.parent, target_is_directory=True)
        item.update(path="private/fixtures/external/outside.bin", identity="outside.bin")
        self.write_manifest()
        self.assertIn("dali-dca: symlink file paths are not accepted", validate(self.root))

    def test_oversized_file_declaration_is_rejected_before_read(self):
        _, item = self.available_file()
        item["bytes"] = MAX_FILE_BYTES + 1
        self.write_manifest()
        self.assertTrue(any("bytes must be between" in e for e in validate(self.root)))

    def test_malformed_and_oversized_manifests_are_rejected(self):
        self.manifest.write_text("not valid = [", encoding="utf-8")
        self.assertTrue(any("cannot load manifest" in e for e in validate(self.root)))
        self.manifest.write_bytes(b" " * (MAX_MANIFEST_BYTES + 1))
        self.assertEqual(validate(self.root),
                         ["reference-inputs.toml: manifest exceeds the 128 KiB bound"])


if __name__ == "__main__":
    unittest.main()
