#!/usr/bin/env python3
# SPDX-License-Identifier: Apache-2.0
"""Build and package only the additive fixed provisioning helper; never install."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from typing import NamedTuple

SOURCE = Path(__file__).resolve().parent
BROKER = SOURCE.parent
REPOSITORY = BROKER.parents[1]
SPEC = importlib.util.spec_from_file_location("task153_package", SOURCE / "prepare_package.py")
PACKAGE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PACKAGE)
BINARY = "task153-operator-provision"
LEAF = "org.openknxstudio.operator-provision"
PREFLIGHT = "task153-install-preflight"
COMPONENT = ("/Library/PrivilegedHelperTools", 0o1755, {BINARY: LEAF})


class HelperPackage(NamedTuple):
    binary: str
    leaf: str
    component: str
    pin_key: str
    filename: str
    purpose: str


PROVISIONER = HelperPackage(BINARY, LEAF, "provisioner", "provisioner",
                            "OpenKNX-Studio-Task153-Provisioner-Review.pkg",
                            "additive fixed provisioning helper; no replacement of installed helpers")
RECOVERY = HelperPackage("task153-operator-recover", "org.openknxstudio.operator-recover",
                         "recovery", "recovery", "OpenKNX-Studio-Task153-Recovery-Review.pkg",
                         "additive fixed recovery helper; no replacement of installed helpers")


def build_arguments(profile):
    return ["build", "--offline", "--locked", "--manifest-path",
            "experiments/task153-broker/Cargo.toml", "--bin", profile.binary, "--bin", PREFLIGHT]


BUILD_ARGUMENTS = build_arguments(PROVISIONER)


def source_paths():
    paths = [REPOSITORY / "rust-toolchain.toml"]
    for directory in (BROKER, BROKER.parent / "task152-passkey"):
        paths.extend(directory / name for name in ("Cargo.toml", "Cargo.lock"))
        for path in (directory / "src").rglob("*"):
            if path.is_symlink():
                raise ValueError("source symlink rejected")
            if not path.is_dir():
                paths.append(path)
    paths.extend((BROKER.parent / "task020-guard").glob("*.rs"))
    if not 1 <= len(paths) <= 256:
        raise ValueError("source inventory bound")
    return sorted(paths)


def snapshot(paths):
    return {path.relative_to(REPOSITORY).as_posix(): hashlib.sha256(PACKAGE.read_snapshot(path, 262144)).hexdigest()
            for path in paths}


def reviewed_source_snapshot():
    paths = source_paths()
    names = [path.relative_to(REPOSITORY).as_posix() for path in paths]
    revision = PACKAGE.run(["/usr/bin/git", "-C", str(REPOSITORY), "rev-parse", "HEAD"]).strip()
    if not re.fullmatch("[0-9a-f]{40}", revision):
        raise ValueError("source revision rejected")
    # Review is performed by the coordinator, not inferred from a clean tree.
    # Reject source dirt/untracked inputs instead of quietly packaging it.
    if PACKAGE.run(["/usr/bin/git", "-C", str(REPOSITORY), "status", "--porcelain=v1", "--untracked-files=all", "--", *names]):
        raise ValueError("compiled source must be committed before package preparation")
    PACKAGE.run(["/usr/bin/git", "-C", str(REPOSITORY), "ls-files", "--error-unmatch", "--", *names])
    return paths, revision, snapshot(paths)


def build_environment():
    cargo_home = REPOSITORY / "private/toolchains/cargo"
    rustup_home = REPOSITORY / "private/toolchains/rustup"
    for directory in (REPOSITORY, *REPOSITORY.parents, cargo_home):
        config_directory = directory if directory == cargo_home else directory / ".cargo"
        if any((config_directory / name).exists() for name in ("config", "config.toml")):
            raise ValueError("Cargo configuration requires separate review")
    if not (cargo_home / "registry").is_dir() or not rustup_home.is_dir():
        raise ValueError("pinned offline toolchain/cache unavailable")
    environment = {"PATH": str(cargo_home / "bin") + ":/usr/bin:/bin:/usr/sbin:/sbin",
                   "CARGO_HOME": str(cargo_home), "RUSTUP_HOME": str(rustup_home),
                   "RUSTUP_TOOLCHAIN": "1.99.1", "CARGO_NET_OFFLINE": "true",
                   "CARGO_TARGET_DIR": str(BROKER / "target"), "PYTHONDONTWRITEBYTECODE": "1"}
    if "HOME" in os.environ:
        environment["HOME"] = os.environ["HOME"]
    return cargo_home / "bin/cargo", environment


def make_pins(files, profile=PROVISIONER):
    helper = files[profile.leaf]
    return {profile.pin_key: {"executable_sha256": list(bytes.fromhex(helper["sha256"])),
                            "executable_cdhash": helper["cdhash"]}}


def main(profile=PROVISIONER, invocation_file=None):
    if profile not in (PROVISIONER, RECOVERY):
        raise ValueError("unsupported fixed helper package")
    parser = argparse.ArgumentParser(description="Build one fixed additive helper package; never install.")
    parser.add_argument("--output", required=True, type=Path)
    arguments = parser.parse_args()
    if sys.platform != "darwin" or os.geteuid() == 0:
        raise ValueError("normal macOS development principal required")
    paths, revision, before = reviewed_source_snapshot()
    cargo, environment = build_environment()
    output = arguments.output.absolute()
    output.mkdir(mode=0o700)
    log = output / "build.log"
    command = build_arguments(profile)
    with log.open("xb") as stream:
        result = subprocess.run([str(cargo), *command], cwd=REPOSITORY, env=environment,
                                stdin=subprocess.DEVNULL, stdout=stream, stderr=subprocess.STDOUT,
                                timeout=360, check=False)
    if result.returncode or log.stat().st_size > 2097152:
        raise ValueError("offline helper build failed; inspect bounded build log")
    if snapshot(paths) != before:
        raise ValueError("source changed during build")
    binaries = BROKER / "target/debug"
    inputs = {}
    for name in (profile.binary, PREFLIGHT):
        data = PACKAGE.checked_input(binaries / name)
        original = PACKAGE.read_snapshot(binaries / name, 134217728)
        if hashlib.sha256(original).hexdigest() != data["sha256"]:
            raise ValueError("built helper changed while being inspected")
        inputs[name] = {"sha256": data["sha256"], "attribute_names": data["attribute_names"]}
    inventory = {"schema": 1, "purpose": profile.purpose,
                 "installation": "NOT RUN", "root_helper_started": "NOT RUN", "hardware": "NOT RUN",
                 "installer_signature": "unsigned", "binary_signature": "ad-hoc hardened runtime; no publisher identity",
                 "components": {}, "source_revision": revision, "source_sha256": before,
                 "build_command": ["cargo", *command], "build_exit": result.returncode,
                 "build_log_sha256": PACKAGE.sha(log), "preparer_sha256": PACKAGE.sha(Path(__file__)),
                 "component_preparer_sha256": PACKAGE.sha(SOURCE / "prepare_package.py"),
                 "source_build_claim": "Snapshot consistency only; not cryptographic execution attestation or independent review",
                 "original_inputs": inputs}
    if invocation_file is not None:
        inventory["invocation_preparer_sha256"] = PACKAGE.sha(invocation_file)
    # Only these two hashes feed the existing copy-before-signing guard.
    evidence = {"helper_artifacts": inputs}
    specification = ("/Library/PrivilegedHelperTools", 0o1755, {profile.binary: profile.leaf})
    component = PACKAGE.build_component(profile.component, binaries, output, inventory,
                                        service_evidence=evidence, component_specification=specification)
    distribution = output / "Distribution"
    PACKAGE.run(["/usr/bin/productbuild", "--synthesize", "--package", str(component), str(distribution)])
    architectures = PACKAGE.product_architectures(inventory["components"])
    tree = ET.parse(distribution)
    tree.getroot().find("options").set("hostArchitectures", ",".join(architectures))
    tree.write(distribution, encoding="utf-8", xml_declaration=True)
    final = output / profile.filename
    PACKAGE.run(["/usr/bin/productbuild", "--distribution", str(distribution), "--package-path", str(output), str(final)])
    expanded = output / "verified-product"
    PACKAGE.run(["/usr/sbin/pkgutil", "--expand-full", str(final), str(expanded)])
    if ET.parse(expanded / "Distribution").getroot().find("options").get("hostArchitectures") != ",".join(architectures):
        raise ValueError("product architecture mismatch")
    component_record = inventory["components"][profile.component]
    for folder, key in (("Payload", "files"), ("Scripts", "scripts")):
        expected = component_record[key]
        actual = expanded / (profile.component + ".pkg") / folder
        if {str(path.relative_to(actual)) for path in actual.rglob("*")} != set(expected):
            raise ValueError("unexpected additive archive entry")
        for name, record in expected.items():
            if PACKAGE.sha(actual / name) != record["sha256"]:
                raise ValueError("additive product content changed")
    if snapshot(paths) != before:
        raise ValueError("source changed during packaging")
    pins = make_pins(component_record["files"], profile)
    pin_bytes = (json.dumps(pins, indent=2) + "\n").encode()
    inventory.update({"package_sha256": PACKAGE.sha(final), "host_architectures": architectures,
                      "receipt_pin_fragment": pins, "receipt_pin_fragment_sha256": hashlib.sha256(pin_bytes).hexdigest(),
                      "parent_preservation": "No ancestors in payload; component-root metadata matches mandatory preflight. Actual installation NOT RUN."})
    (output / (profile.component + "-pin-fragment.json")).write_bytes(pin_bytes)
    (output / "inventory.json").write_text(json.dumps(inventory, indent=2) + "\n")
    print(f"Prepared additive review package: {final}")
    print("No installation, privileged helper, service, credential or hardware operation performed.")


if __name__ == "__main__":
    main()
