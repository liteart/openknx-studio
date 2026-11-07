#!/usr/bin/env python3
# SPDX-License-Identifier: Apache-2.0
"""Prepare unsigned local development components; never install or launch them."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import plistlib
import re
import shutil
import stat
import subprocess
import sys
import xml.etree.ElementTree as ET

SOURCE = Path(__file__).resolve().parent
LABEL = "org.openknxstudio.operator"
COMPONENTS = {
    "helpers": ("/Library/PrivilegedHelperTools", 0o1755, {
        "task153-operator-service": LABEL,
        "task153-operator-browser": LABEL + "-browser",
        "task153-operator-enrollment": LABEL + "-enrollment",
    }),
    "daemon": ("/Library/LaunchDaemons", 0o755, {}),
}
ALLOWED_INPUT_ATTRIBUTES = {"com.apple.provenance"}
PINNED_FORK_REVISION = "4d296afb0906d17b6724b56c912f2e9e3f6680a5"
# Tree IDs independently read from the pinned Git commit objects, not a report.
PINNED_FORK_TREE = "619fd4ff1e49e6f064a1d54f772bffb6dbf9c5f9"
PINNED_EXACL_REVISION = "18275aa1a82aae9a786b108b0db0992a4d06e6e2"
PINNED_EXACL_TREE = "8a283b273ca09aa8b1b0eac9dbfc71a94895c1b6"
HELPER_NAMES = ("task153-operator-service", "task153-operator-browser",
                "task153-operator-enrollment", "task153-install-preflight")
GUARDED_QUALIFICATION = "VERIFIED_INSTALLED_STARTUP_AND_PROTECTED_JOURNAL_REQUIRED"
GUARDED_ENTRYPOINT = "REGISTERED; genuine protected passkey assertion required"


def run(arguments, *, cwd=None):
    process = subprocess.run(arguments, cwd=cwd, stdin=subprocess.DEVNULL,
                             stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                             timeout=90, check=False)
    if len(process.stdout) + len(process.stderr) > 131072:
        raise ValueError("tool output bound")
    if process.returncode:
        raise ValueError(f"tool failed: {Path(arguments[0]).name} ({process.returncode})")
    return (process.stdout + process.stderr).decode("utf-8", errors="strict")


def sha(path):
    with path.open("rb") as source:
        return hashlib.file_digest(source, "sha256").hexdigest()


def checked_input(path):
    """Check before copy: never launder quarantine or unknown security metadata."""
    if path.is_symlink() or not path.is_file() or path.stat().st_size > 134217728:
        raise ValueError("input rejected")
    attributes = set(run(["/usr/bin/xattr", "-s", str(path)]).splitlines())
    if attributes - ALLOWED_INPUT_ATTRIBUTES:
        raise ValueError("input security/unknown metadata rejected")
    return {"sha256": sha(path), "attribute_names": sorted(attributes)}


def copy_binary(source, target, expected_sha256=None):
    checked = checked_input(source)
    expected = checked["sha256"] if expected_sha256 is None else expected_sha256
    if checked["sha256"] != expected:
        raise ValueError("input changed after evidence verification")
    with source.open("rb") as file:
        if file.read(4) not in (b"\xcf\xfa\xed\xfe", b"\xfe\xed\xfa\xcf", b"\xca\xfe\xba\xbe"):
            raise ValueError("Mach-O executable required")
    shutil.copyfile(source, target)
    target.chmod(0o755)
    # Repeat source security-metadata checks after copying, and bind the actual
    # copied bytes to the earlier verified artifact before ad-hoc signing.
    after = checked_input(source)
    if after["sha256"] != expected or sha(target) != expected:
        raise ValueError("input changed while copying")
    architectures = set(run(["/usr/bin/lipo", "-archs", str(target)]).split())
    if not architectures or architectures - {"arm64", "x86_64"}:
        raise ValueError("unsupported executable architecture")
    # Explicit ad-hoc signing never consults a user signing identity/keychain.
    run(["/usr/bin/codesign", "--force", "--sign", "-", "--options", "runtime",
         "--timestamp=none", str(target)])
    run(["/usr/bin/codesign", "--verify", "--strict", str(target)])
    detail = run(["/usr/bin/codesign", "--display", "--verbose=4", str(target)])
    match = re.search(r"^CDHash=([0-9a-f]{40})$", detail, re.MULTILINE)
    if not match or "Signature=adhoc" not in detail or "runtime" not in detail:
        raise ValueError("unexpected code signature")
    return {"sha256": sha(target), "cdhash": match[1], "mode": "0755",
            "architectures": sorted(architectures), "input": checked}


def product_architectures(components):
    executables = [data for component in components.values()
                   for group in ("files", "scripts") for data in component[group].values()
                   if "cdhash" in data]
    common = set.intersection(*(set(data["architectures"]) for data in executables))
    if not common:
        raise ValueError("components have no common executable architecture")
    return sorted(common)


def enrollment_manifest_fragment(helpers):
    enrollment = helpers[LABEL + "-enrollment"]
    return {"enrollment_bootstrap": {
        "executable_sha256": list(bytes.fromhex(enrollment["sha256"])),
        "executable_cdhash": enrollment["cdhash"],
    }}


def archive(root, destination, entries):
    """Archive only named entries; do not modify filesystem xattrs or ACLs.

    bsdtar(1) documents no-mac-metadata/no-xattrs/no-acls. odc is the cpio
    format accepted by pkgutil's script/payload decoder. Source validation is
    separate; this omits only staging metadata from our generated archives.
    """
    run(["/usr/bin/tar", "--format=odc", "--uid", "0", "--gid", "0",
         "--uname", "root", "--gname", "wheel", "--no-mac-metadata",
         "--no-xattrs", "--no-acls", "--no-recursion", "-czf", str(destination),
         "-C", str(root), *entries])


def bom_rows(raw, expected):
    rows = []
    for line in raw.splitlines():
        path = line.split("\t", 1)[0]
        if path in expected:
            rows.append(line)
        elif path != "." and not Path(path).name.startswith("._"):
            raise ValueError("unexpected initial BOM entry")
    if {line.split("\t", 1)[0] for line in rows} != expected:
        raise ValueError("missing BOM entries")
    return "\n".join(rows) + "\n"


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError("duplicate evidence key")
        result[key] = value
    return result


def read_snapshot(path, maximum):
    """Read one bounded no-follow file snapshot through no-follow ancestors."""
    absolute = path.absolute()
    if ".." in absolute.parts:
        raise ValueError("evidence path rejected")
    descriptor = os.open("/", os.O_RDONLY | os.O_DIRECTORY | os.O_NOFOLLOW)
    try:
        for part in absolute.parts[1:-1]:
            child = os.open(part, os.O_RDONLY | os.O_DIRECTORY | os.O_NOFOLLOW, dir_fd=descriptor)
            os.close(descriptor)
            descriptor = child
        file = os.open(absolute.name, os.O_RDONLY | os.O_NOFOLLOW | os.O_NONBLOCK, dir_fd=descriptor)
        try:
            before = os.fstat(file)
            if not stat.S_ISREG(before.st_mode) or before.st_size > maximum:
                raise ValueError("evidence file rejected")
            data = bytearray()
            while len(data) <= maximum:
                block = os.read(file, min(65536, maximum + 1 - len(data)))
                if not block:
                    break
                data.extend(block)
            after = os.fstat(file)
            stamp = lambda value: (value.st_dev, value.st_ino, value.st_size, value.st_mtime_ns, value.st_ctime_ns)
            if len(data) > maximum or stamp(before) != stamp(after):
                raise ValueError("evidence file changed or exceeded bound")
            return bytes(data)
        finally:
            os.close(file)
    finally:
        os.close(descriptor)


def export_path(root, name):
    path = Path(name)
    if (not name or path.is_absolute() or path.as_posix() != name
            or any(part in (".", "..", ".git") for part in path.parts)
            or "\\" in name or ":" in name or len(name) > 1024):
        raise ValueError("export path rejected")
    return root / path


def git_tree(files):
    tree = {}
    for name, metadata in files.items():
        parent = tree
        parts = name.split("/")
        for part in parts[:-1]:
            child = parent.setdefault(part, {})
            if not isinstance(child, dict):
                raise ValueError("export tree collision")
            parent = child
        if parts[-1] in parent:
            raise ValueError("export tree collision")
        parent[parts[-1]] = (metadata["mode"], metadata["git_blob"])

    def hash_tree(entries):
        content = bytearray()
        for name, value in sorted(entries.items(), key=lambda item: (item[0] + ("/" if isinstance(item[1], dict) else "")).encode()):
            mode, identifier = ("40000", hash_tree(value)) if isinstance(value, dict) else value
            content.extend(mode.encode() + b" " + name.encode() + b"\0" + bytes.fromhex(identifier))
        return hashlib.sha1(b"tree " + str(len(content)).encode() + b"\0" + content).hexdigest()

    return hash_tree(tree)


def actual_export_files(root, maximum):
    pending, files, count = [root], set(), 0
    while pending:
        directory = pending.pop()
        for path in directory.iterdir():
            count += 1
            if count > maximum or path.is_symlink():
                raise ValueError("export directory bound or symlink rejected")
            metadata = path.stat(follow_symlinks=False)
            if stat.S_ISDIR(metadata.st_mode):
                pending.append(path)
            elif stat.S_ISREG(metadata.st_mode):
                files.add(path.relative_to(root).as_posix())
            else:
                raise ValueError("export object type rejected")
    return files


def validate_export(record, root):
    guarded = record.get("operator_service_artifact", {}).get("host_qualification") == GUARDED_QUALIFICATION
    build = ["cargo", "build", "--locked", "--offline"]
    build += [item for name in (HELPER_NAMES if guarded else HELPER_NAMES[:1]) for item in ("--bin", name)]
    expected_commands = [
        ["rustc", "-vV"], ["cargo", "check", "--offline", "--lib"],
        build,
        ["cargo", "build", "--locked", "--offline", "--no-default-features", "--features", "dpt", "--example", "connected_management_simulator"],
        ["cargo", "metadata", "--locked", "--offline", "--format-version", "1", "--filter-platform", "aarch64-apple-darwin"],
        ["cargo", "test", "--locked", "--offline", "--lib", "--", "--test-threads=1"],
        ["cargo", "clippy", "--locked", "--offline", "--lib", "--tests", "--", "-D", "warnings"],
    ]
    if (record.get("status") != "PASS; exact-source development integration, synthetic loopback only"
            or record.get("fork_revision") != PINNED_FORK_REVISION
            or record.get("exacl_revision") != PINNED_EXACL_REVISION
            or record.get("fork_features") != ["dpt"]
            or record.get("hardware") != "NOT RUN" or record.get("installed_boundary") != "NOT RUN"):
        raise ValueError("export outcome or pinned revision rejected")
    commands = record.get("commands")
    if not isinstance(commands, list) or len(commands) != len(expected_commands) + 1:
        raise ValueError("export command sequence rejected")
    logs = {}
    for index, command in enumerate(commands):
        if command.get("exit") != 0 or command.get("log") != f"command-{index:02d}.log":
            raise ValueError("export command result rejected")
        arguments = command.get("args")
        if index < len(expected_commands):
            if arguments != expected_commands[index]:
                raise ValueError("export build/test command rejected")
        elif (not isinstance(arguments, list) or len(arguments) != 3 or arguments[0] != "python3"
              or Path(arguments[1]).name != "validate_actor_logs.py" or Path(arguments[2]) != root):
            raise ValueError("export actor validation command rejected")
        data = read_snapshot(root / command["log"], 2_097_152)
        if index == 0 and not data.startswith(b"rustc 1.99.1 "):
            raise ValueError("export toolchain rejected")
        logs[command["log"]] = hashlib.sha256(data).hexdigest()
    compiled = record.get("compiled_source_sha256")
    if not isinstance(compiled, dict) or not 1 <= len(compiled) <= 256:
        raise ValueError("compiled source inventory rejected")
    if actual_export_files(root / "editor/experiments", 1024) != set(compiled):
        raise ValueError("compiled source inventory incomplete")
    for name, expected in compiled.items():
        data = read_snapshot(export_path(root / "editor/experiments", name), 262144)
        if hashlib.sha256(data).hexdigest() != expected:
            raise ValueError("compiled source changed")
    lock = read_snapshot(root / "editor/experiments/task153-broker/Cargo.lock", 262144)
    if hashlib.sha256(lock).hexdigest() != record.get("temporary_lock_sha256"):
        raise ValueError("compiled lock changed")
    for key, folder, expected_tree in (("fork_files", "pinned-fork", PINNED_FORK_TREE),
                                       ("exacl_files", "pinned-exacl", PINNED_EXACL_TREE)):
        files = record.get(key)
        if not isinstance(files, dict) or not 1 <= len(files) <= 4096:
            raise ValueError("dependency inventory rejected")
        if actual_export_files(root / folder, 8192) != set(files):
            raise ValueError("dependency inventory incomplete")
        total = 0
        for name, metadata in files.items():
            if metadata.get("mode") not in ("100644", "100755"):
                raise ValueError("dependency mode rejected")
            data = read_snapshot(export_path(root / folder, name), 4194304)
            mode = (root / folder / name).stat(follow_symlinks=False).st_mode & 0o777
            total += len(data)
            blob = hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()
            if (mode != int(metadata["mode"], 8) & 0o777
                    or total > 16777216 or hashlib.sha256(data).hexdigest() != metadata.get("sha256")
                    or blob != metadata.get("git_blob")):
                raise ValueError("dependency export changed")
        if git_tree(files) != expected_tree:
            raise ValueError("dependency export is not the pinned Git tree")
    return {"compiled_files": len(compiled), "dependency_git_trees": [PINNED_FORK_TREE, PINNED_EXACL_TREE],
            "command_log_sha256": logs,
            "execution_attestation": "NOT ESTABLISHED; report/log claims still require independent review"}


def verified_service_input(binary, evidence, binaries=None):
    if (binary is None) != (evidence is None):
        raise ValueError("service binary and exact-export evidence must be supplied together")
    if binary is None:
        return None, None
    evidence_bytes = read_snapshot(evidence, 1_048_576)
    record = json.loads(evidence_bytes, object_pairs_hook=unique_object)
    audit = validate_export(record, evidence.absolute().parent)
    artifact = record["operator_service_artifact"]
    artifact_path = Path(artifact["path"])
    if artifact_path.is_absolute() or ".." in artifact_path.parts:
        raise ValueError("service evidence path rejected")
    expected = evidence.absolute().parent / artifact_path
    if binary.absolute() != expected.absolute():
        raise ValueError("service evidence identifies another artifact")
    checked = checked_input(binary)
    binary_bytes = read_snapshot(binary, 134217728)
    built = read_snapshot(evidence.absolute().parent / "target/debug/task153-operator-service", 134217728)
    qualification = artifact.get("host_qualification")
    guarded = qualification == GUARDED_QUALIFICATION
    if (checked["sha256"] != hashlib.sha256(binary_bytes).hexdigest()
            or checked["sha256"] != artifact["sha256"] or binary_bytes != built
            or len(binary_bytes) != artifact["size_bytes"]
            or qualification not in ("UNCONSTRUCTIBLE", GUARDED_QUALIFICATION)
            or (guarded and artifact.get("execution_entrypoint") != GUARDED_ENTRYPOINT)
            or artifact["service_started"] != "NOT RUN"
            or artifact["installation"] != "NOT RUN"
            or artifact["hardware"] != "NOT RUN"):
        raise ValueError("service evidence/artifact mismatch or unsupported qualification")
    helpers = {}
    if guarded:
        root = evidence.absolute().parent
        if binaries is None or binaries.absolute() != root / "artifacts":
            raise ValueError("guarded helpers must come from the same exact export")
        records = record.get("helper_artifacts")
        if not isinstance(records, dict) or set(records) != set(HELPER_NAMES):
            raise ValueError("guarded helper inventory rejected")
        for name in HELPER_NAMES:
            item = records[name]
            if set(item) != {"path", "sha256", "size_bytes"} or item["path"] != "artifacts/" + name:
                raise ValueError("guarded helper artifact path/schema rejected")
            path = root / "artifacts" / name
            original = read_snapshot(path, 134217728)
            source_build = read_snapshot(root / "target/debug" / name, 134217728)
            inputs = checked_input(path)
            digest = hashlib.sha256(original).hexdigest()
            if (not original or original != source_build or digest != item["sha256"]
                    or digest != inputs["sha256"] or len(original) != item["size_bytes"]):
                raise ValueError("guarded helper artifact/build mismatch")
            helpers[name] = {"sha256": digest, "size_bytes": len(original)}
        if helpers["task153-operator-service"]["sha256"] != checked["sha256"]:
            raise ValueError("guarded service/helper inventory mismatch")
    result = {"evidence_sha256": hashlib.sha256(evidence_bytes).hexdigest(), "artifact_sha256": checked["sha256"],
                    "fork_revision": record["fork_revision"], "host_qualification": qualification,
                    "service_started": "NOT RUN", "installation": "NOT RUN", "hardware": "NOT RUN", "export_audit": audit}
    if guarded:
        result.update({"execution_entrypoint": GUARDED_ENTRYPOINT, "helper_artifacts": helpers})
    return binary, result


def build_component(name, binaries, output, inventory, service_binary=None, service_evidence=None,
                    component_specification=None):
    install_location, root_mode, files = COMPONENTS[name] if component_specification is None else component_specification
    work = output / name
    work.mkdir()
    payload, scripts = work / "staged-payload", work / "staged-scripts"
    payload.mkdir(); payload.chmod(root_mode)
    scripts.mkdir()
    record = {"install_location": install_location, "required_existing_root_mode": oct(root_mode),
              "files": {}, "scripts": {}}
    for binary, leaf in files.items():
        source = service_binary if binary == "task153-operator-service" and service_binary is not None else binaries / binary
        expected = service_evidence.get("helper_artifacts", {}).get(binary, {}).get("sha256") if service_evidence else None
        if expected is None and binary == "task153-operator-service" and service_evidence is not None:
            expected = service_evidence["artifact_sha256"]
        record["files"][leaf] = copy_binary(source, payload / leaf, expected)
    if name == "daemon":
        daemon = {"Label": LABEL,
                  "ProgramArguments": ["/Library/PrivilegedHelperTools/" + LABEL, "--operator-service-only"],
                  "UserName": "root", "GroupName": "wheel", "Disabled": True,
                  "RunAtLoad": False, "KeepAlive": False, "Umask": 0o077,
                  "WorkingDirectory": "/", "EnvironmentVariables": {"PATH": "/usr/bin:/bin:/usr/sbin:/sbin"}}
        plist = payload / (LABEL + ".plist")
        plist.write_bytes(plistlib.dumps(daemon)); plist.chmod(0o644)
        record["files"][plist.name] = {"sha256": sha(plist), "mode": "0644"}
        run(["/usr/bin/plutil", "-lint", str(plist)])
    preflight_sha = service_evidence.get("helper_artifacts", {}).get("task153-install-preflight", {}).get("sha256") if service_evidence else None
    record["scripts"]["preflight"] = copy_binary(binaries / "task153-install-preflight", scripts / "preflight", preflight_sha)
    template = SOURCE / "preinstall"
    record["preinstall_input"] = checked_input(template)
    text = template.read_text()
    if text.count("@COMPONENT@") != 1:
        raise ValueError("preinstall template mismatch")
    preinstall = scripts / "preinstall"
    preinstall.write_text(text.replace("@COMPONENT@", name)); preinstall.chmod(0o755)
    run(["/bin/sh", "-n", str(preinstall)])
    record["scripts"]["preinstall"] = {"sha256": sha(preinstall), "mode": "0755"}

    # pkgbuild provides PackageInfo and file checksums. Its metadata companions
    # are not accepted as payload; replace both archives and rebuild its BOM.
    initial = work / "initial.pkg"
    run(["/usr/bin/pkgbuild", "--root", str(payload), "--scripts", str(scripts),
         "--identifier", LABEL + ".development." + name, "--version", "0.0.0",
         "--install-location", install_location, "--ownership", "recommended", str(initial)])
    expanded = work / "expanded"
    run(["/usr/sbin/pkgutil", "--expand", str(initial), str(expanded)])
    # Keep no unverified intermediate installer that could be opened by mistake.
    initial.unlink()
    expected = {"."} | {"./" + leaf for leaf in record["files"]}
    clean_rows = bom_rows(run(["/usr/bin/lsbom", str(expanded / "Bom")]), expected)
    listing = work / "bom-input.txt"; listing.write_text(clean_rows)
    run(["/usr/bin/mkbom", "-i", str(listing), str(expanded / "Bom")])
    # Scripts from pkgutil --expand are a directory; remove only this new copy.
    if (expanded / "Scripts").is_dir():
        shutil.rmtree(expanded / "Scripts")
    else:
        (expanded / "Scripts").unlink()
    archive(payload, expanded / "Payload", [".", *sorted(record["files"])])
    archive(scripts, expanded / "Scripts", sorted(record["scripts"]))
    info = ET.parse(expanded / "PackageInfo")
    info.getroot().set("overwrite-permissions", "false")
    info.getroot().find("payload").set("numberOfFiles", str(len(expected)))
    info.write(expanded / "PackageInfo", encoding="utf-8", xml_declaration=True)
    final = output / (name + ".pkg")
    # pkgutil --flatten would re-wrap a raw Scripts archive as a script file.
    # Flat packages are XAR archives; retain the four exact component members.
    run(["/usr/bin/xar", "--compression", "none", "-cf", str(final),
         "Bom", "PackageInfo", "Payload", "Scripts"], cwd=expanded)
    verified = work / "verified"
    run(["/usr/sbin/pkgutil", "--expand-full", str(final), str(verified)])
    verified_info = ET.parse(verified / "PackageInfo").getroot()
    if (verified_info.get("overwrite-permissions") != "false"
            or verified_info.get("install-location") != install_location):
        raise ValueError("component install policy mismatch")
    actual = set(run(["/usr/sbin/pkgutil", "--payload-files", str(final)]).splitlines())
    if actual != expected:
        raise ValueError("component payload mismatch or metadata companion")
    rows = run(["/usr/bin/lsbom", "-p", "fmug", str(verified / "Bom")]).splitlines()
    actual_metadata = {x.split("\t")[0]: x.split("\t")[1:] for x in rows}
    expected_metadata = {".": [format(0o40000 | root_mode, "o"), "0", "0"]}
    expected_metadata.update({"./" + leaf: [format(0o100000 | int(data["mode"], 8), "o"), "0", "0"]
                              for leaf, data in record["files"].items()})
    if actual_metadata != expected_metadata:
        raise ValueError("component BOM metadata mismatch")
    for folder, records in [("Payload", record["files"]), ("Scripts", record["scripts"])]:
        actual_paths = {str(path.relative_to(verified / folder)) for path in (verified / folder).rglob("*")}
        if actual_paths != set(records):
            raise ValueError("unexpected expanded payload/script entry")
        for leaf, data in records.items():
            path = verified / folder / leaf
            if path.is_symlink() or not path.is_file() or sha(path) != data["sha256"]:
                raise ValueError("expanded file mismatch")
            if "cdhash" in data:
                run(["/usr/bin/codesign", "--verify", "--strict", str(path)])
    record.update({"package_sha256": sha(final), "archive_entries": sorted(actual),
                   "bom_metadata": actual_metadata, "script_archive_entries": sorted(record["scripts"]),
                   "overwrite_permissions": False, "appledouble_metadata_entries": []})
    inventory["components"][name] = record
    return final


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--bin-dir", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--service-bin", required=True, type=Path)
    parser.add_argument("--service-evidence", required=True, type=Path)
    args = parser.parse_args()
    if sys.platform != "darwin" or os.geteuid() == 0:
        raise ValueError("normal macOS development principal required; do not run as root")
    binaries = args.bin_dir.absolute()
    service_binary, service_evidence = verified_service_input(args.service_bin, args.service_evidence, binaries)
    output = args.output.absolute()
    output.mkdir(mode=0o700)
    inventory = {"schema": 2, "purpose": "local development / no device execution",
                 "installer_signature": "unsigned", "binary_signature": "ad-hoc hardened runtime; no publisher identity",
                 "installation": "NOT RUN", "host_qualification": "NOT RUN", "hardware": "NOT RUN",
                 "components": {}, "preparer_sha256": sha(Path(__file__))}
    if service_evidence is not None:
        inventory["exact_export_service"] = service_evidence
        if service_evidence["host_qualification"] == GUARDED_QUALIFICATION:
            inventory["purpose"] = "guarded physical write capable after configured exact-plan approval; local development"
            inventory["physical_execution"] = "Target 1.1.231 only; verified installed startup, protected journal/plan and genuine durable passkey assertion required. Installation and enrollment alone do not dispatch."
    packages = [build_component(name, binaries, output, inventory, service_binary, service_evidence) for name in COMPONENTS]
    inventory["manifest_pin_fragments"] = enrollment_manifest_fragment(inventory["components"]["helpers"]["files"])
    distribution = output / "Distribution"
    run(["/usr/bin/productbuild", "--synthesize", *[x for package in packages for x in ("--package", str(package))], str(distribution)])
    architectures = product_architectures(inventory["components"])
    distribution_tree = ET.parse(distribution)
    distribution_tree.getroot().find("options").set("hostArchitectures", ",".join(architectures))
    distribution_tree.write(distribution, encoding="utf-8", xml_declaration=True)
    final = output / "OpenKNX-Studio-Task153-Review.pkg"
    run(["/usr/bin/productbuild", "--distribution", str(distribution), "--package-path", str(output), str(final)])
    verified_product = output / "verified-product"
    run(["/usr/sbin/pkgutil", "--expand-full", str(final), str(verified_product)])
    if ET.parse(verified_product / "Distribution").getroot().find("options").get("hostArchitectures") != ",".join(architectures):
        raise ValueError("product architecture policy mismatch")
    for name in COMPONENTS:
        # productbuild embeds component packages; each is independently verified
        # above. Recheck actual embedded content against that verified export.
        embedded = verified_product / (name + ".pkg")
        for folder in ("Payload", "Scripts"):
            for leaf, data in inventory["components"][name]["files" if folder == "Payload" else "scripts"].items():
                if sha(embedded / folder / leaf) != data["sha256"]:
                    raise ValueError("product embedded component mismatch")
    inventory["package_sha256"] = sha(final)
    inventory["host_architectures"] = architectures
    inventory["existing_parent_preservation"] = "Static checks PASS: no ancestor entries; component root metadata matches mandatory preflight. Actual Installer execution NOT RUN."
    # Review material only: never an active root manifest or installation step.
    pin_bytes = (json.dumps(inventory["manifest_pin_fragments"], indent=2) + "\n").encode()
    inventory["manifest_pin_fragments_sha256"] = hashlib.sha256(pin_bytes).hexdigest()
    (output / "manifest-pin-fragments.json").write_bytes(pin_bytes)
    (output / "inventory.json").write_text(json.dumps(inventory, indent=2) + "\n")
    print(f"Prepared review package: {final}")
    print("No installation, account/service/profile changes, enrollment or hardware operation performed.")


if __name__ == "__main__":
    main()
