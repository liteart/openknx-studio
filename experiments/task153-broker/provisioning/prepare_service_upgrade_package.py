#!/usr/bin/env python3
# SPDX-License-Identifier: Apache-2.0
"""Prepare only two new service-upgrade leaves; never install, migrate or start."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET

SOURCE = Path(__file__).resolve().parent
SPEC = importlib.util.spec_from_file_location("task153_additive", SOURCE / "prepare_provisioner_package.py")
ADDON = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(ADDON)
PACKAGE = ADDON.PACKAGE
UPGRADE_REVISION = "f4a9f13cc9f6b98e82bb8510cbb6d353e41adf51"
SERVICE_REVISION = "de571a6a582c87a0af8386c7431968037e8d1ba5"
SERVICE_SHA256 = "fd0334e9ec364da7dbed7ba08d575d4cc792186cf61e83c09210d84d99b80d9c"
BINARY = "task153-operator-service-upgrade"
UPGRADER = "org.openknxstudio.operator-service-upgrade"
NEXT = "org.openknxstudio.operator-service-next"
COMPONENT = ("/Library/PrivilegedHelperTools", 0o1755, {
    BINARY: UPGRADER, "task153-operator-service": NEXT,
})
BUILD_ARGUMENTS = ["build", "--offline", "--locked", "--manifest-path",
                   "experiments/task153-broker/Cargo.toml", "--bin", BINARY, "--bin", ADDON.PREFLIGHT]


def read_json(path):
    data = PACKAGE.read_snapshot(path, 1048576)
    return data, json.loads(data, object_pairs_hook=PACKAGE.unique_object)


def verify_replacement(evidence_path, proof_path):
    evidence_bytes, report = read_json(evidence_path)
    proof_bytes, proof = read_json(proof_path)
    if (proof.get("schema") != 1 or proof.get("editor_revision") != SERVICE_REVISION
            or proof.get("runner_result_sha256") != hashlib.sha256(evidence_bytes).hexdigest()
            or proof.get("service_started") != "NOT RUN" or proof.get("hardware") != "NOT RUN"):
        raise ValueError("replacement source proof rejected")
    work = Path(proof["export_workdir"])
    if not work.is_absolute():
        raise ValueError("replacement export root rejected")
    original = PACKAGE.read_snapshot(work / "result.json", 1048576)
    if original != evidence_bytes:
        raise ValueError("preserved replacement evidence differs from original export")
    service, validated = PACKAGE.verified_service_input(
        work / "artifacts/task153-operator-service", work / "result.json", work / "artifacts")
    inputs = proof.get("source_inputs_checked_against_committed_git_blobs")
    if not isinstance(inputs, dict) or inputs != report.get("source_sha256") or not 1 <= len(inputs) <= 256:
        raise ValueError("replacement source proof inventory rejected")
    for name, expected in inputs.items():
        PACKAGE.export_path(work, name)
        if Path(name).parts[0] not in ("task153-broker", "task152-passkey", "task020-guard"):
            raise ValueError("replacement source component rejected")
        text = PACKAGE.run(["/usr/bin/git", "-C", str(ADDON.REPOSITORY), "cat-file", "blob",
                            SERVICE_REVISION + ":experiments/" + name])
        if hashlib.sha256(text.encode()).hexdigest() != expected:
            raise ValueError("replacement source differs from pinned committed blob")
    preserved = evidence_path.absolute().parent / "artifacts/task153-operator-service"
    source_bytes = PACKAGE.read_snapshot(service, 134217728)
    preserved_bytes = PACKAGE.read_snapshot(preserved, 134217728)
    checked = PACKAGE.checked_input(preserved)
    if (source_bytes != preserved_bytes or checked["sha256"] != SERVICE_SHA256
            or validated["artifact_sha256"] != SERVICE_SHA256):
        raise ValueError("replacement artifact differs from reviewed service")
    validated.update({"preserved_result_sha256": hashlib.sha256(evidence_bytes).hexdigest(),
                      "source_proof_sha256": hashlib.sha256(proof_bytes).hexdigest(),
                      "editor_revision": SERVICE_REVISION,
                      "source_inputs_checked_against_committed_git_blobs": inputs})
    return preserved, validated


def make_pins(files):
    return {key: {"executable_sha256": list(bytes.fromhex(files[leaf]["sha256"])),
                  "executable_cdhash": files[leaf]["cdhash"]}
            for key, leaf in (("upgrader", UPGRADER), ("new_service", NEXT))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--service-evidence", required=True, type=Path)
    parser.add_argument("--service-proof", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    arguments = parser.parse_args()
    if sys.platform != "darwin" or os.geteuid() == 0:
        raise ValueError("normal macOS development principal required")
    paths, revision, before = ADDON.reviewed_source_snapshot()
    if revision != UPGRADE_REVISION:
        raise ValueError("upgrade helper must be built from the reviewed pinned revision")
    service, service_evidence = verify_replacement(arguments.service_evidence, arguments.service_proof)
    cargo, environment = ADDON.build_environment()
    output = arguments.output.absolute()
    output.mkdir(mode=0o700)
    log = output / "build.log"
    with log.open("xb") as stream:
        result = subprocess.run([str(cargo), *BUILD_ARGUMENTS], cwd=ADDON.REPOSITORY, env=environment,
                                stdin=subprocess.DEVNULL, stdout=stream, stderr=subprocess.STDOUT,
                                timeout=360, check=False)
    if result.returncode or log.stat().st_size > 2097152 or ADDON.snapshot(paths) != before:
        raise ValueError("upgrade helper build failed or source changed")
    binaries = ADDON.BROKER / "target/debug"
    inputs = {"task153-operator-service": {"sha256": SERVICE_SHA256}}
    for name in (BINARY, ADDON.PREFLIGHT):
        checked = PACKAGE.checked_input(binaries / name)
        original = PACKAGE.read_snapshot(binaries / name, 134217728)
        if hashlib.sha256(original).hexdigest() != checked["sha256"]:
            raise ValueError("built upgrade input changed")
        inputs[name] = {"sha256": checked["sha256"], "attribute_names": checked["attribute_names"]}
    inventory = {"schema": 1, "purpose": "additive service upgrade helper and staged replacement; no installed path replaced by Installer",
                 "installation": "NOT RUN", "migration": "NOT RUN", "service_started": "NOT RUN",
                 "hardware": "NOT RUN", "installer_signature": "unsigned",
                 "binary_signature": "ad-hoc hardened runtime; no publisher identity", "components": {},
                 "source_revision": revision, "source_sha256": before, "build_command": ["cargo", *BUILD_ARGUMENTS],
                 "build_exit": result.returncode, "build_log_sha256": PACKAGE.sha(log),
                 "preparer_sha256": PACKAGE.sha(Path(__file__)),
                 "component_preparer_sha256": PACKAGE.sha(SOURCE / "prepare_package.py"),
                 "replacement_evidence": service_evidence, "original_inputs": inputs,
                 "runtime_authority": "The separately invoked root helper can migrate the fixed unused service and protected manifest under an owner-reviewed receipt. The staged replacement becomes guarded physical-write capable only after migration, verified service startup and genuine exact-plan passkey approval. Installation performs none of these actions.",
                 "source_build_claim": "Snapshot consistency, not cryptographic execution attestation; independent review required"}
    component = PACKAGE.build_component("service-upgrade", binaries, output, inventory, service_binary=service,
                                        service_evidence={"helper_artifacts": inputs}, component_specification=COMPONENT)
    distribution = output / "Distribution"
    PACKAGE.run(["/usr/bin/productbuild", "--synthesize", "--package", str(component), str(distribution)])
    architectures = PACKAGE.product_architectures(inventory["components"])
    tree = ET.parse(distribution)
    tree.getroot().find("options").set("hostArchitectures", ",".join(architectures))
    tree.write(distribution, encoding="utf-8", xml_declaration=True)
    final = output / "OpenKNX-Studio-Task153-Service-Upgrade-Review.pkg"
    PACKAGE.run(["/usr/bin/productbuild", "--distribution", str(distribution), "--package-path", str(output), str(final)])
    expanded = output / "verified-product"
    PACKAGE.run(["/usr/sbin/pkgutil", "--expand-full", str(final), str(expanded)])
    if ET.parse(expanded / "Distribution").getroot().find("options").get("hostArchitectures") != ",".join(architectures):
        raise ValueError("upgrade product architecture mismatch")
    record = inventory["components"]["service-upgrade"]
    for folder, key in (("Payload", "files"), ("Scripts", "scripts")):
        actual = expanded / "service-upgrade.pkg" / folder
        if {str(path.relative_to(actual)) for path in actual.rglob("*")} != set(record[key]):
            raise ValueError("unexpected upgrade product entry")
        for name, data in record[key].items():
            if PACKAGE.sha(actual / name) != data["sha256"]:
                raise ValueError("upgrade embedded content changed")
    if ADDON.snapshot(paths) != before or PACKAGE.checked_input(service)["sha256"] != SERVICE_SHA256:
        raise ValueError("upgrade inputs changed during packaging")
    pin_bytes = (json.dumps(make_pins(record["files"]), indent=2) + "\n").encode()
    inventory.update({"package_sha256": PACKAGE.sha(final), "host_architectures": architectures,
                      "receipt_pin_fragment": make_pins(record["files"]),
                      "receipt_pin_fragment_sha256": hashlib.sha256(pin_bytes).hexdigest(),
                      "parent_preservation": "No ancestors in payload; component root matches mandatory preflight. Actual installation NOT RUN."})
    (output / "upgrade-pin-fragment.json").write_bytes(pin_bytes)
    (output / "inventory.json").write_text(json.dumps(inventory, indent=2) + "\n")
    print(f"Prepared additive upgrade review package: {final}")
    print("No installation, root migration, service start, credential or hardware operation performed.")


if __name__ == "__main__":
    main()
