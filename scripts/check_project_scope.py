#!/usr/bin/env python3
"""Check the reviewed planning scope, local Markdown paths and task identities offline."""

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import sys
import tomllib
from urllib.parse import unquote, urlsplit


# Pin parsed values, allowing comments/formatting changes. See docs/SCOPE-BASELINE.md
# before updating this value; a passing check cannot grant human approval.
BASELINE_SHA256 = "371458c1330c4e6f5d1275cca3f4321f184f84ef5c915c355fb53b85c82d4c95"
INLINE_LINK = re.compile(r"!?\[[^\]\n]*\]\(\s*(<[^>\n]+>|[^\s)]+)(?:\s+\"[^\"\n]*\")?\s*\)")
REFERENCE_LINK = re.compile(r"^\s{0,3}\[[^\]\n]+\]:\s*(<[^>\n]+>|\S+)", re.MULTILINE)


def validate_scope(root: Path) -> list[str]:
    path = root / "config/project-scope.toml"
    try:
        scope = tomllib.loads(path.read_text(encoding="utf-8"))
        canonical = json.dumps(scope, sort_keys=True, separators=(",", ":"))
    except (OSError, ValueError, TypeError) as exc:
        return [f"config/project-scope.toml: {exc}"]
    if hashlib.sha256(canonical.encode("utf-8")).hexdigest() != BASELINE_SHA256:
        return ["Scope baseline changed: approved ADR and corresponding task/test review "
                "required for scope changes; explicit human decisions required to resolve "
                "repository owner/remotes. Do not update the pin to bypass review."]
    return []


def validate_documents(root: Path) -> list[str]:
    errors = []
    texts = {}
    markdown_paths = []
    for directory, names, files in os.walk(root):
        parent = Path(directory)
        names[:] = sorted(name for name in names
                          if not name.startswith(".")
                          and name not in {"target", "node_modules"}
                          and not (parent / name / ".git").exists()
                          and not (parent / name).is_symlink())
        markdown_paths.extend(parent / name for name in sorted(files)
                              if name.endswith(".md"))
    for path in markdown_paths:
        relative = path.relative_to(root)
        if any(part.startswith(".") or part in {"target", "node_modules"}
               for part in relative.parts):
            continue
        try:
            contents = path.read_text(encoding="utf-8")
        except (OSError, UnicodeError) as exc:
            errors.append(f"{relative}: {exc}")
            continue
        texts[relative.as_posix()] = contents
        # Current repository links use inline destinations or reference definitions.
        # Fenced examples are excluded; URL fragments are not heading validation.
        prose = re.sub(r"(?ms)^(`{3,}|~{3,})[^\n]*\n.*?^\1[^\n]*$", "", contents)
        destinations = [m.group(1) for m in INLINE_LINK.finditer(prose)]
        destinations += [m.group(1) for m in REFERENCE_LINK.finditer(prose)]
        for destination in destinations:
            destination = destination.strip("<>")
            try:
                parsed = urlsplit(destination)
            except ValueError:
                errors.append(f"{relative}: malformed link {destination}")
                continue
            if parsed.scheme or parsed.netloc or not parsed.path:
                continue
            target = (path.parent / unquote(parsed.path)).resolve()
            if not target.is_relative_to(root) or not target.exists():
                errors.append(f"{relative}: unresolved local link {destination}")

    index = texts.get("TASKS.md", "")
    if not index:
        errors.append("TASKS.md: missing task index")
    seen = set()
    tasks = sorted((root / "tasks").glob("Task-*.md"))
    if not tasks:
        errors.append("tasks/: no task files")
    for path in tasks:
        contents = texts.get(path.relative_to(root).as_posix(), "")
        task_id = path.stem
        identities = re.findall(r"^\| Task ID \| (Task-\d{3}) \|$", contents, re.MULTILINE)
        heading = re.match(r"# (Task-\d{3}): ", contents)
        if identities != [task_id] or heading is None or heading.group(1) != task_id:
            errors.append(f"{path.name}: task ID must match filename, heading and metadata")
        for identity in identities:
            if identity in seen:
                errors.append(f"{path.name}: duplicate task ID {identity}")
            seen.add(identity)
        entry = rf"^\| \[{re.escape(task_id)}\]\(tasks/{re.escape(path.name)}\) \|"
        if len(re.findall(entry, index, re.MULTILINE)) != 1:
            errors.append(f"TASKS.md: {task_id} must occur exactly once as an index link")
    return errors


def validate(root: Path) -> list[str]:
    root = root.resolve()
    return validate_scope(root) + validate_documents(root)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    errors = validate(parser.parse_args().root)
    for error in errors:
        print(f"FAIL: {error}", file=sys.stderr)
    if errors:
        return 1
    print("PASS: pinned planning scope, local Markdown paths and unique task identities")
    print("NOT RUN: product, Windows DCA, hardware, rights and human gate approval")
    return 0


if __name__ == "__main__":
    sys.exit(main())
