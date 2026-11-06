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
import subprocess
import sys
import xml.etree.ElementTree as ET

SOURCE = Path(__file__).resolve().parent
LABEL = "org.openknxstudio.operator"
COMPONENTS = {
    "helpers": ("/Library/PrivilegedHelperTools", 0o1755, {
        "task153-operator-service": LABEL,
        "task153-operator-browser": LABEL + "-browser",
    }),
    "daemon": ("/Library/LaunchDaemons", 0o755, {}),
}
ALLOWED_INPUT_ATTRIBUTES = {"com.apple.provenance"}


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
        raise ValueError("input security/unknown metadata rejected before copy")
    return {"sha256": sha(path), "attribute_names": sorted(attributes)}


def copy_binary(source, target):
    checked = checked_input(source)
    with source.open("rb") as file:
        if file.read(4) not in (b"\xcf\xfa\xed\xfe", b"\xfe\xed\xfa\xcf", b"\xca\xfe\xba\xbe"):
            raise ValueError("Mach-O executable required")
    shutil.copyfile(source, target)
    target.chmod(0o755)
    if sha(source) != checked["sha256"] or sha(target) != checked["sha256"]:
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


def build_component(name, binaries, output, inventory):
    install_location, root_mode, files = COMPONENTS[name]
    work = output / name
    work.mkdir()
    payload, scripts = work / "staged-payload", work / "staged-scripts"
    payload.mkdir(); payload.chmod(root_mode)
    scripts.mkdir()
    record = {"install_location": install_location, "required_existing_root_mode": oct(root_mode),
              "files": {}, "scripts": {}}
    for binary, leaf in files.items():
        record["files"][leaf] = copy_binary(binaries / binary, payload / leaf)
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
    record["scripts"]["preflight"] = copy_binary(binaries / "task153-install-preflight", scripts / "preflight")
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
    args = parser.parse_args()
    if sys.platform != "darwin" or os.geteuid() == 0:
        raise ValueError("normal macOS development principal required; do not run as root")
    binaries = args.bin_dir.resolve(strict=True)
    output = args.output.absolute()
    output.mkdir(mode=0o700)
    inventory = {"schema": 2, "purpose": "local development / no device execution",
                 "installer_signature": "unsigned", "binary_signature": "ad-hoc hardened runtime; no publisher identity",
                 "installation": "NOT RUN", "host_qualification": "NOT RUN", "hardware": "NOT RUN",
                 "components": {}, "preparer_sha256": sha(Path(__file__))}
    packages = [build_component(name, binaries, output, inventory) for name in COMPONENTS]
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
    (output / "inventory.json").write_text(json.dumps(inventory, indent=2) + "\n")
    print(f"Prepared review package: {final}")
    print("No installation, account/service/profile changes, enrollment or hardware operation performed.")


if __name__ == "__main__":
    main()
