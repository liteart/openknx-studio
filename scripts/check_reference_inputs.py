#!/usr/bin/env python3
"""Check the development-only reference inventory and private file hashes offline."""

import argparse
import hashlib
from pathlib import Path, PurePosixPath
import re
import stat
import sys
import tomllib


MAX_FILE_BYTES = 256 * 1024 * 1024  # EP-03 compressed input bound; no extraction.
MAX_MANIFEST_BYTES = 128 * 1024
INPUT_KINDS = {
    "dali-dca": "file",
    "dali-knxprod": "file",
    "simple-knxprod": "file",
    "simple-device": "qualification",
    "dali-device": "qualification",
    "dali-application-match": "qualification",
    "simple-application-match": "qualification",
    "knxnetip-gateway": "qualification",
    "isolated-bench": "qualification",
    "dali-gear": "qualification",
    "power-recovery": "qualification",
    "reversible-property": "qualification",
    "human-write-grant": "qualification",
    "clean-windows": "qualification",
    "sdk-references-rights": "qualification",
    "firmware-dca": "file",
    "firmware-device": "qualification",
    "firmware-image": "file",
    "knxproj": "file",
}
COMMON_FIELDS = {"id", "kind", "status", "identity", "owner", "source", "evidence"}
FILE_FIELDS = {"path", "bytes", "sha256"}
OWNERS = {"Project maintainer", "Project operator"}


def check_file(root: Path, item: dict) -> list[str]:
    identity = item["id"]
    raw_path = item.get("path")
    if (not isinstance(raw_path, str) or not raw_path or "\\" in raw_path
            or ":" in raw_path or any(ord(char) < 32 for char in raw_path)):
        return [f"{identity}: file path must be a relative POSIX path"]
    relative = PurePosixPath(raw_path)
    if (relative.is_absolute() or ".." in relative.parts
            or relative.parts[:2] != ("private", "fixtures") or len(relative.parts) < 3):
        return [f"{identity}: file path must stay inside private/fixtures"]
    path = root.joinpath(*relative.parts)
    component = root
    for part in relative.parts:
        component = component / part
        if component.is_symlink():
            return [f"{identity}: symlink file paths are not accepted"]
    if not path.resolve().is_relative_to(root / "private/fixtures"):
        return [f"{identity}: resolved file path escapes private/fixtures"]
    size = item.get("bytes")
    digest = item.get("sha256")
    if type(size) is not int or not 0 < size <= MAX_FILE_BYTES:
        return [f"{identity}: bytes must be between 1 and {MAX_FILE_BYTES}"]
    if not isinstance(digest, str) or re.fullmatch(r"[0-9a-f]{64}", digest) is None:
        return [f"{identity}: sha256 must contain 64 lowercase hexadecimal digits"]
    if item["identity"] != relative.name:
        return [f"{identity}: file identity must equal the filename"]
    try:
        metadata = path.stat()
        if not stat.S_ISREG(metadata.st_mode):
            return [f"{identity}: available input must be a regular file"]
        if metadata.st_size != size:
            return [f"{identity}: file size mismatch"]
        actual = hashlib.sha256()
        count = 0
        with path.open("rb") as stream:
            while chunk := stream.read(min(1024 * 1024, MAX_FILE_BYTES + 1 - count)):
                count += len(chunk)
                if count > MAX_FILE_BYTES:
                    return [f"{identity}: file exceeds the 256 MiB bound"]
                actual.update(chunk)
        if count != size:
            return [f"{identity}: file size changed while hashing"]
        if actual.hexdigest() != digest:
            return [f"{identity}: SHA-256 mismatch"]
    except OSError as exc:
        return [f"{identity}: cannot read available input ({exc.strerror})"]
    return []


def validate(root: Path, required: tuple[str, ...] = ()) -> list[str]:
    root = root.resolve()
    try:
        with (root / "config/reference-inputs.toml").open("rb") as stream:
            raw = stream.read(MAX_MANIFEST_BYTES + 1)
        if len(raw) > MAX_MANIFEST_BYTES:
            return ["reference-inputs.toml: manifest exceeds the 128 KiB bound"]
        manifest = tomllib.loads(raw.decode("utf-8"))
    except (OSError, ValueError, UnicodeError) as exc:
        return [f"reference-inputs.toml: cannot load manifest ({type(exc).__name__})"]
    if (set(manifest) != {"schema_version", "inputs"}
            or type(manifest.get("schema_version")) is not int
            or manifest["schema_version"] != 1
            or not isinstance(manifest.get("inputs"), list)):
        return ["reference-inputs.toml: expected schema_version = 1 and inputs array"]
    inputs = manifest["inputs"]
    if len(inputs) > len(INPUT_KINDS):
        return ["reference-inputs.toml: too many input entries"]
    errors = []
    seen = {}
    for index, item in enumerate(inputs):
        if not isinstance(item, dict):
            errors.append(f"input {index}: expected a table")
            continue
        identity = item.get("id")
        if not isinstance(identity, str) or identity not in INPUT_KINDS:
            errors.append(f"input {index}: unknown input ID")
            continue
        if identity in seen:
            errors.append(f"{identity}: duplicate input ID")
            continue
        seen[identity] = item
        if not isinstance(item.get("status"), str) or item["status"] not in {"Available", "Missing"}:
            errors.append(f"{identity}: status must be Available or Missing")
            continue
        expected = COMMON_FIELDS.copy()
        if item["status"] == "Missing":
            expected.add("missing_reason")
        elif INPUT_KINDS[identity] == "file":
            expected |= FILE_FIELDS
        if set(item) != expected:
            errors.append(f"{identity}: unexpected or missing fields")
            continue
        text_fields = expected - {"bytes"}
        if any(not isinstance(item[field], str) or not item[field].strip()
               or len(item[field]) > 2048 for field in text_fields):
            errors.append(f"{identity}: text fields must be nonempty and bounded")
            continue
        if item["kind"] != INPUT_KINDS[identity] or item["owner"] not in OWNERS:
            errors.append(f"{identity}: invalid kind or acquisition owner role")
            continue
        if item["status"] == "Available" and item["kind"] == "file":
            errors.extend(check_file(root, item))
    for identity in sorted(INPUT_KINDS.keys() - seen.keys()):
        errors.append(f"{identity}: missing inventory entry")
    for identity in required:
        if identity not in INPUT_KINDS:
            errors.append(f"{identity}: unknown required input")
        elif identity not in seen or seen[identity].get("status") != "Available":
            errors.append(f"{identity}: required input is Missing")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--require", nargs="+", default=[], metavar="INPUT_ID")
    args = parser.parse_args()
    errors = validate(args.root, tuple(args.require))
    print("INPUT ONLY: no gate approval, bench qualification, or permission for bus operations")
    for error in errors:
        print(f"FAIL: {error}", file=sys.stderr)
    if errors:
        return 1
    print("PASS: complete input inventory and available-file sizes/SHA-256 hashes")
    return 0


if __name__ == "__main__":
    sys.exit(main())
