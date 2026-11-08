#!/usr/bin/env python3
# SPDX-License-Identifier: Apache-2.0
"""Build only the fixed additive UUID-recovery helper; never install or recover."""
import importlib.util
from pathlib import Path

SOURCE = Path(__file__).resolve()
SPEC = importlib.util.spec_from_file_location("task153_additive", SOURCE.with_name("prepare_provisioner_package.py"))
ADDON = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(ADDON)

if __name__ == "__main__":
    ADDON.main(ADDON.RECOVERY, invocation_file=SOURCE)
