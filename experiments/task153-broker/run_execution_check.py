#!/usr/bin/env python3
"""Exact-source development integration only; no installed or device executor."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import sys
sys.dont_write_bytecode = True

SOURCE = Path(__file__).resolve().parent
GUARD = SOURCE.parent / "task020-guard"
spec = importlib.util.spec_from_file_location("task020_export", GUARD / "run_adapter_check.py")
helper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helper)
EXACL_REV = "18275aa1a82aae9a786b108b0db0992a4d06e6e2"

def copy_sources(source, destination, hashes):
    destination.mkdir(parents=True)
    paths = [source / "Cargo.toml", source / "Cargo.lock"]
    paths += list((source / "src").rglob("*"))
    if len(paths) > 256:
        raise ValueError("source file count bound")
    for path in paths:
        if path.is_symlink():
            raise ValueError("symlink source input")
        if path.is_dir():
            continue
        relative = path.relative_to(source)
        if path.is_symlink() or path.suffix not in (".rs", ".js", ".html", ".toml", ".lock") or path.stat().st_size > 262144:
            raise ValueError("unexpected source input")
        target = destination / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
        hashes[f"{source.name}/{relative}"] = helper.digest(target)

def merged_lock(left, right):
    # Preserve exact cached/yanked resolutions from both approved source locks.
    # Cargo may update only this temporary union; no release lock is changed.
    import tomllib
    seen, blocks = set(), []
    preferred = {package["name"] for package in tomllib.loads(left)["package"]}
    for side, text in enumerate((left, right)):
        for block in text.split("\n[[package]]\n")[1:]:
            package = tomllib.loads("[[package]]\n" + block)["package"][0]
            key = (package["name"], package["version"], package.get("source"))
            if side == 1 and package["name"] in preferred:
                continue
            if key not in seen:
                seen.add(key); blocks.append(block)
    return "# Temporary exact-source integration lock.\nversion = 4\n\n[[package]]\n" + "\n[[package]]\n".join(blocks)

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fork", required=True, type=Path)
    parser.add_argument("--exacl", required=True, type=Path)
    args = parser.parse_args()
    fork, exacl = args.fork.resolve(strict=True), args.exacl.resolve(strict=True)
    helper.inspect_fork(fork)
    if helper.git_bytes(exacl,"rev-parse","HEAD").decode().strip() != EXACL_REV or helper.git_bytes(exacl,"diff","HEAD","--"):
        raise ValueError("exacl revision/worktree mismatch")
    work = Path(tempfile.mkdtemp(prefix="task153-execution-", dir="/private/tmp"))
    print(f"Development artifacts: {work}", flush=True)
    report = {"status":"IN PROGRESS", "fork_revision":helper.FORK_REVISION,"exacl_revision":EXACL_REV,"hardware":"NOT RUN","installed_boundary":"NOT RUN","source_sha256":{},"commands":[]}
    def record(): (work / "result.json").write_text(json.dumps(report,indent=2)+"\n")
    env = helper.isolated_build_env(work)
    def execute(command, cwd):
        log = work / f"command-{len(report['commands']):02d}.log"
        with log.open("wb") as output:
            process = subprocess.run(command,cwd=cwd,env=env,stdout=output,stderr=subprocess.STDOUT,timeout=360)
        report["commands"].append({"args":command,"exit":process.returncode,"log":log.name});record()
        if process.returncode: raise RuntimeError(f"check failed; see {log}")
        return log.read_text()
    try:
        exported = work / "pinned-fork"
        report["fork_files"] = helper.export_pinned_tree(fork,exported)
        acl_export = work / "pinned-exacl"
        report["exacl_files"] = helper.export_pinned_tree(exacl,acl_export,EXACL_REV)
        editor = work / "editor/experiments"
        broker = editor / "task153-broker"
        copy_sources(SOURCE,broker,report["source_sha256"])
        copy_sources(SOURCE.parent / "task152-passkey",editor / "task152-passkey",report["source_sha256"])
        guard = editor / "task020-guard";guard.mkdir()
        for name in helper.INPUTS:
            shutil.copyfile(GUARD / name,guard / name)
            report["source_sha256"][f"task020-guard/{name}"] = helper.digest(guard / name)
        # Visible source-only registrations. No shipping source/manifest/path
        # override or production constructor is added to the editor package.
        with (broker / "src/lib.rs").open("a") as output:
            output.write('\n#[allow(dead_code)]\n#[path="../../task020-guard/integrated_guard.rs"]\nmod guard;\n#[allow(dead_code)]\n#[path="../../task020-guard/knust_adapter.rs"]\nmod knust_adapter;\n')
        shutil.copyfile(guard / "guard.rs",guard / "integrated_guard.rs")
        with (guard / "integrated_guard.rs").open("a") as output:
            output.write('\n#[cfg(test)]\n#[path="actor_tests.rs"]\nmod actor_tests;\n#[allow(dead_code)]\n#[path="../task153-broker/src/guard_execution.rs"]\nmod protected_operator;\n')
        manifest = (broker / "Cargo.toml").read_text()
        manifest = manifest.replace('exacl = { git = "https://github.com/byllyfish/exacl.git", rev = "'+EXACL_REV+'", default-features = false }','exacl = { path = '+json.dumps(str(acl_export))+', default-features = false }')
        manifest += '\n[dependencies.tokio]\nversion="=1.53.1"\ndefault-features=false\nfeatures=["rt","sync","time","macros"]\n[dependencies.knust]\npath='+json.dumps(str(exported))+'\ndefault-features=false\nfeatures=["dpt"]\n'
        (broker / "Cargo.toml").write_text(manifest)
        (broker / "Cargo.lock").write_text(merged_lock((SOURCE / "Cargo.lock").read_text(),(exported / "Cargo.lock").read_text()))
        def compiled_hashes():
            paths = sorted(path for path in editor.rglob("*") if path.is_file())
            if len(paths) > 512 or any(path.is_symlink() for path in paths):
                raise ValueError("compiled source inventory rejected")
            return {str(path.relative_to(editor)): helper.digest(path) for path in paths}
        before_check = compiled_hashes()
        version = execute(["rustc","-vV"],broker)
        if not version.startswith("rustc 1.99.1 "): raise ValueError("pinned Rust required")
        execute(["cargo","check","--offline","--lib"],broker)
        compiled = compiled_hashes()
        lock_name = "task153-broker/Cargo.lock"
        if {name:value for name,value in before_check.items() if name != lock_name} != {name:value for name,value in compiled.items() if name != lock_name}:
            raise ValueError("compiled source changed during check")
        report["compiled_source_sha256"] = compiled
        report["temporary_lock_sha256"] = helper.digest(broker / "Cargo.lock")
        record()
        # Build the actual service entrypoint from this same registered crate,
        # not only a library/test harness. This does not enable the uninhabited
        # host qualification, run the service, or install/package anything.
        execute(["cargo","build","--locked","--offline","--bin","task153-operator-service"],broker)
        service = work / "target/debug/task153-operator-service"
        if service.is_symlink() or not service.is_file() or not 0 < service.stat().st_size <= 134217728:
            raise ValueError("operator service artifact rejected")
        artifacts = work / "artifacts"
        artifacts.mkdir()
        copied_service = artifacts / service.name
        shutil.copyfile(service,copied_service)
        copied_service.chmod(0o755)
        if helper.digest(copied_service) != helper.digest(service):
            raise ValueError("copied operator service changed")
        report["operator_service_artifact"] = {
            "path":str(copied_service.relative_to(work)),
            "sha256":helper.digest(copied_service),
            "size_bytes":copied_service.stat().st_size,
            "host_qualification":"UNCONSTRUCTIBLE",
            "service_started":"NOT RUN",
            "installation":"NOT RUN",
            "hardware":"NOT RUN",
        }
        record()
        execute(["cargo","build","--locked","--offline","--no-default-features","--features","dpt","--example","connected_management_simulator"],exported)
        metadata=json.loads(execute(["cargo","metadata","--locked","--offline","--format-version","1","--filter-platform","aarch64-apple-darwin"],broker))
        ids={package["id"] for package in metadata["packages"] if package["name"]=="knust"}
        features=[node["features"] for node in metadata["resolve"]["nodes"] if node["id"] in ids]
        if features != [["dpt"]]: raise ValueError("unexpected fork/Secure features")
        report["fork_features"]=features[0]
        execute(["cargo","test","--locked","--offline","--lib","--","--test-threads=1"],broker)
        execute(["cargo","clippy","--locked","--offline","--lib","--tests","--","-D","warnings"],broker)
        execute(["python3",str(GUARD / "validate_actor_logs.py"),str(work)],broker)
        if compiled_hashes() != compiled:
            raise ValueError("compiled source changed during verification")
        if (helper.digest(copied_service) != report["operator_service_artifact"]["sha256"]
                or helper.digest(service) != report["operator_service_artifact"]["sha256"]):
            raise ValueError("operator service artifact changed during verification")
        for directory, key in [(exported,"fork_files"),(acl_export,"exacl_files")]:
            if any(helper.digest(directory / name) != value["sha256"] for name,value in report[key].items()):
                raise ValueError("exported dependency changed during verification")
        helper.inspect_fork(fork)
        report["status"]="PASS; exact-source development integration, synthetic loopback only"
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        report["status"]="FAIL";report["error"]=str(error);record();raise
    record();print(report["status"],flush=True)
if __name__ == "__main__": main()
