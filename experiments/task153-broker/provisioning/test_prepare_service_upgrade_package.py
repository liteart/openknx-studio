# SPDX-License-Identifier: Apache-2.0
"""Upgrade package policy; no installation, root migration or KNX."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

SPEC = importlib.util.spec_from_file_location("prepare_upgrade", Path(__file__).with_name("prepare_service_upgrade_package.py"))
UPGRADE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(UPGRADE)


class UpgradePackagePolicyTests(unittest.TestCase):
    def test_payload_contains_only_two_new_fixed_leaves(self):
        self.assertEqual(UPGRADE.COMPONENT, ("/Library/PrivilegedHelperTools", 0o1755, {
            "task153-operator-service-upgrade": "org.openknxstudio.operator-service-upgrade",
            "task153-operator-service": "org.openknxstudio.operator-service-next"}))
        self.assertNotIn("org.openknxstudio.operator", UPGRADE.COMPONENT[2].values())
        self.assertEqual(UPGRADE.BUILD_ARGUMENTS[-4:], ["--bin", "task153-operator-service-upgrade", "--bin", "task153-install-preflight"])

    def test_receipt_pins_are_for_signed_upgrade_and_new_service_only(self):
        records = {UPGRADE.UPGRADER: {"sha256": "12" * 32, "cdhash": "a" * 40},
                   UPGRADE.NEXT: {"sha256": "34" * 32, "cdhash": "b" * 40}}
        self.assertEqual(UPGRADE.make_pins(records), {
            "upgrader": {"executable_sha256": [0x12] * 32, "executable_cdhash": "a" * 40},
            "new_service": {"executable_sha256": [0x34] * 32, "executable_cdhash": "b" * 40}})
        del records[UPGRADE.NEXT]
        with self.assertRaises(KeyError):
            UPGRADE.make_pins(records)

    def test_replacement_proof_requires_pinned_revision_and_exact_evidence_bytes(self):
        with tempfile.TemporaryDirectory(dir="/private/tmp") as directory:
            root = Path(directory)
            evidence, proof = root / "result.json", root / "source-proof.json"
            evidence.write_text('{}')
            record = {"schema": 1, "editor_revision": "unreviewed",
                      "runner_result_sha256": UPGRADE.hashlib.sha256(evidence.read_bytes()).hexdigest(),
                      "service_started": "NOT RUN", "hardware": "NOT RUN"}
            proof.write_text(json.dumps(record))
            with self.assertRaisesRegex(ValueError, "source proof rejected"):
                UPGRADE.verify_replacement(evidence, proof)
            record["editor_revision"] = UPGRADE.SERVICE_REVISION
            record["runner_result_sha256"] = "0" * 64
            proof.write_text(json.dumps(record))
            with self.assertRaisesRegex(ValueError, "source proof rejected"):
                UPGRADE.verify_replacement(evidence, proof)


if __name__ == "__main__":
    unittest.main()
