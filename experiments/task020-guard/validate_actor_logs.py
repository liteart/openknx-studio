"""Read-only bounded artifact validation; Python optimization cannot skip checks."""
import argparse
import json
from pathlib import Path
import re

EXPECTED = {
    "normal": (["00", "00", "04", "00"], ["04", "00"], "00", None),
    "queued": (["00"], [], "00", None),
    "hold-before": (["00", "00"], [], "00", "WRITE_PENDING"),
    "hold-after": (["00", "00"], ["04"], "04", "WRITE_INFLIGHT"),
    "deadline": (["00", "00"], ["04"], "04", "WRITE_INFLIGHT"),
}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def bounded_read(path, limit=65_536):
    require(not path.is_symlink(), f"symlink artifact: {path.name}")
    with path.open("rb") as source:
        data = source.read(limit + 1)
    require(len(data) <= limit, f"oversized artifact: {path.name}")
    return data.decode("utf-8")


def entries(path):
    require(path.is_dir() and not path.is_symlink(), "invalid artifact directory")
    found = []
    for child in path.iterdir():
        require(len(found) < 256, "artifact directory entry bound exceeded")
        found.append(child)
    return found


def validate(root):
    root_entries = entries(root)
    results = {}
    for case, (reads, writes, final, held) in EXPECTED.items():
        def check(condition, reason):
            require(condition, f"{case}: {reason}")
        check(not bounded_read(root / f"e2e-{case}.stderr.log", 1024), "nonempty stderr")
        lines = bounded_read(root / f"e2e-{case}.sim.log").splitlines()
        check(3 <= len(lines) <= 132 and all(len(line) <= 512 for line in lines), "invalid line count/length")
        check(re.fullmatch(r"SIMULATOR_READY endpoint=127\.0\.0\.1:[1-9][0-9]* synthetic=true", lines[0]), "invalid loopback readiness")
        events = lines[1:-1]
        ids = [re.match(r"SIM_EVENT id=(\d+) kind=", line) for line in events]
        check(all(ids) and [int(match[1]) for match in ids] == list(range(1, len(events) + 1)), "event sequence mismatch")
        text = "\n".join(events)
        check(re.findall(r"kind=PROPERTY_READ value=(\w+)", text) == reads, "property read values")
        check(re.findall(r"kind=WRITE_APPLIED ordinal=\d+ value=(\w+)", text) == writes, "applied write values")
        check(sum("kind=DESCRIPTOR_READ mask=0705" in line for line in events) == len(reads), "descriptor count")
        operations = []
        for line in events:
            read = re.search(r"kind=PROPERTY_READ value=(\w+)", line)
            write = re.search(r"kind=WRITE_APPLIED ordinal=\d+ value=(\w+)", line)
            if read:
                operations.append(("read", read[1]))
            if write:
                operations.append(("write", write[1]))
        wanted = [("read", value) for value in reads[:2]]
        if case == "normal":
            wanted += [("write", "04"), ("read", "04"), ("write", "00"), ("read", "00")]
        elif writes:
            wanted += [("write", "04")]
        check(operations == wanted, "independent read/write order")
        check(len(events) >= 2 and events[-2].endswith("kind=DEVICE_DISCONNECTED"), "missing distinct device cleanup")
        check(events[-1].endswith("kind=GATEWAY_DISCONNECTED device_open=false"), "missing distinct gateway cleanup")
        check(lines[-1] == f"SIMULATOR_DONE reason=gateway_closed value={final} writes={len(writes)} device_open=false synthetic=true", "final state mismatch")
        check(sum("kind=WRITE_PENDING" in line or "kind=WRITE_INFLIGHT" in line for line in events) == (1 if held else 0), "unexpected pending/inflight count")
        if held:
            check(sum(f"kind={held} " in line for line in events) == 1, "missing held event")
        journals = [path for path in root_entries if path.name.startswith(f"e2e-{case}-journal-")]
        check(bool(journals), "missing journal")
        journal = max(journals, key=lambda path: path.stat().st_mtime_ns)
        files = entries(journal)
        for path in files:
            json.loads(bounded_read(path, 16_384))
        def count(suffix):
            return sum(bool(re.fullmatch(r"[0-9a-f]+-" + re.escape(suffix) + r"\.json", path.name)) for path in files)
        check(count("intent") == 1, "intent count")
        check(count("consumed-grant") == 1, "consumed grant count")
        check(count("unresolved-route") == (0 if case == "normal" else 1), "quarantine count")
        check(count("cleanup") == (1 if case == "normal" else 0), "cleanup receipt count")
        check(count("complete") == (1 if case == "normal" else 0), "verified completion count")
        results[case] = {"status": "PASS", "property_reads": reads, "applied_writes": writes, "final_synthetic_value": final, "route_quarantined": case != "normal", "journal": str(journal)}
    return results


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("artifact_dir", type=Path)
    args = parser.parse_args()
    try:
        print(json.dumps(validate(args.artifact_dir.resolve(strict=True)), indent=2))
    except (OSError, ValueError) as error:
        parser.exit(1, f"Artifact validation failed: {error}\n")


if __name__ == "__main__":
    main()
