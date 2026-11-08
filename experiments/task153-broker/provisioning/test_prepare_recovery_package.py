# SPDX-License-Identifier: Apache-2.0
"""Recovery-addon policy checks; no installation or root recovery."""
import importlib.util
from pathlib import Path
import unittest

SPEC = importlib.util.spec_from_file_location("prepare_recovery", Path(__file__).with_name("prepare_recovery_package.py"))
RECOVERY_TOOL = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(RECOVERY_TOOL)
ADDON = RECOVERY_TOOL.ADDON


class RecoveryPackagePolicyTests(unittest.TestCase):
    def test_only_fixed_new_recovery_helper_and_preflight_are_built(self):
        profile = ADDON.RECOVERY
        self.assertEqual(profile.binary, "task153-operator-recover")
        self.assertEqual(profile.leaf, "org.openknxstudio.operator-recover")
        self.assertEqual(profile.component, "recovery")
        self.assertEqual(ADDON.build_arguments(profile)[-4:],
                         ["--bin", "task153-operator-recover", "--bin", "task153-install-preflight"])
        for existing in ("task153-operator-service", "task153-operator-browser",
                         "task153-operator-enrollment", "task153-operator-provision"):
            self.assertNotIn(existing, ADDON.build_arguments(profile))

    def test_recovery_pin_fragment_cannot_select_an_existing_helper(self):
        profile = ADDON.RECOVERY
        files = {profile.leaf: {"sha256": "12" * 32, "cdhash": "a" * 40},
                 ADDON.LEAF: {"sha256": "34" * 32, "cdhash": "b" * 40}}
        self.assertEqual(ADDON.make_pins(files, profile), {
            "recovery": {"executable_sha256": [0x12] * 32, "executable_cdhash": "a" * 40}})
        del files[profile.leaf]
        with self.assertRaises(KeyError):
            ADDON.make_pins(files, profile)

    def test_no_caller_selected_helper_profile_is_accepted(self):
        alternate = ADDON.RECOVERY._replace(binary="other", leaf="other")
        with self.assertRaisesRegex(ValueError, "unsupported fixed helper package"):
            ADDON.main(alternate)


if __name__ == "__main__":
    unittest.main()
