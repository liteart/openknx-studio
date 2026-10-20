#!/usr/bin/env python3
"""Validate project routing templates without loading secrets or starting models."""

import argparse
from pathlib import Path
import sys
import tomllib

ROLES = {
    "explorer": ("gpt-6-luna", "high"),
    "implementer": ("gpt-6.1-sol", "high"),
    "protocol": ("gpt-6.1-sol", "high"),
    "dca": ("gpt-6.1-sol", "high"),
    "verifier": ("gpt-6-luna", "high"),
    "reviewer": ("gpt-6.1-sol", "high"),
    "evidence": ("gpt-6-luna", "medium"),
}


def validate(root: Path) -> list[str]:
    errors = []
    source = root / "config/codex"

    def load(path):
        try:
            return tomllib.loads(path.read_text(encoding="utf-8"))
        except (OSError, ValueError) as exc:
            errors.append(f"{path.relative_to(root)}: {exc}")
            return {}

    config = load(source / "config.toml")
    expected = {
        "model": "gpt-6.1-sol",
        "model_reasoning_effort": "medium",
        "review_model": "gpt-6.1-sol",
        "agents": {
            "enabled": True,
            "max_concurrent_threads_per_session": 3,
            "default_subagent_model": "gpt-6-luna",
            "default_subagent_reasoning_effort": "high",
        },
    }
    if config != expected:
        errors.append("Project defaults differ from the reviewed routing policy")
    files = sorted((source / "agents").glob("*.toml"))
    if {p.stem for p in files} != set(ROLES):
        errors.append("Missing or unexpected agent files")
    allowed = {"name", "description", "model", "model_reasoning_effort", "developer_instructions"}
    for path in files:
        role = load(path)
        if set(role) != allowed:
            errors.append(f"{path.name}: unsupported or missing fields")
        if role.get("name") != path.stem:
            errors.append(f"{path.name}: role name does not match filename")
        selected = (role.get("model"), role.get("model_reasoning_effort"))
        if selected != ROLES.get(path.stem):
            errors.append(f"{path.name}: unexpected model/effort {selected}")
        for field in ("description", "developer_instructions"):
            if not isinstance(role.get(field), str) or not role[field].strip():
                errors.append(f"{path.name}: empty {field}")
    runtime = root / ".codex"
    if runtime.exists():
        installed_roles = {p.stem for p in (runtime / "agents").glob("*.toml")}
        if installed_roles != set(ROLES):
            errors.append("Missing or unexpected installed agent files")
        for path in [source / "config.toml", *files]:
            installed = runtime / path.relative_to(source)
            try:
                if installed.read_bytes() != path.read_bytes():
                    errors.append(f"{installed.relative_to(root)}: differs from template")
            except OSError as exc:
                errors.append(f"{installed.relative_to(root)}: {exc}")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    root = parser.parse_args().root.resolve()
    errors = validate(root)
    if errors:
        for error in errors:
            print(f"FAIL: {error}", file=sys.stderr)
        return 1
    print("PASS: project defaults and all seven agent templates")
    print("PASS: installed copies match" if (root / ".codex").exists()
          else "NOT RUN: runtime installation absent; templates only")
    return 0


if __name__ == "__main__":
    sys.exit(main())
