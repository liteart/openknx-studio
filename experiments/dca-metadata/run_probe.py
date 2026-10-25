#!/usr/bin/env python3
"""Task-140 macOS research harness; not a general sandbox or production parser.

Requires the already provisioned PowerShell 7.6.7 and three privately staged pins.
The parent reads filesystem attributes only; vendor bytes reach only the restricted
child. The exact deny-default policy must pass owned write/network negative probes
before metadata reading. A failed nested sandbox blocks this route; do not relax it.
Raw output is private. --output must be a new basename inside the private stage.
"""

import argparse
import base64
import json
import os
from pathlib import Path
import selectors
import signal
import stat
import subprocess
import sys
import tempfile
import time

PROFILE = "(version 1)(deny default)(allow file-read*)(allow process*)(allow sysctl-read)(allow mach-lookup)"
LIMIT = 2 * 1024 * 1024
DEADLINE = 30.0
PINS = (
    ("MDT-DaliDcaPlus.dll", 452608, "4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d"),
    ("Knx.Ets.Sdk.AddIns.AddInViews.dll", 72704, "5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d"),
    ("Knx.Ets.Sdk.dll", 6979024, "7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989"),
)
OBJECT_MODEL_PIN = ("Knx.Ets.ObjectModel.dll", 14204880, "73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a")
OBJECT_MODEL_IDENTITY = "Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROJECT_ID_PIN = ("Knx.Ets.Ids.dll", 318928, "43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877")
PROJECT_ID_IDENTITY = "Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROJECT_UNDO_MANAGER_PIN = ("Knx.Ets.ObjectModel.dll", 14204880, "73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a")
PROJECT_UNDO_MANAGER_IDENTITY = "Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROJECT_UNDO_STORE_PROVIDER_PIN = ("Knx.Ets.ObjectModel.dll", 14204880, "73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a")
PROJECT_UNDO_STORE_PROVIDER_IDENTITY = "Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROJECT_STREAM_STORE_PIN = ("Knx.Ets.ObjectModel.dll", 14204880, "73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a")
PROJECT_ONLINE_CATALOG_REFERENCES_PIN = OBJECT_MODEL_PIN
PROJECT_ONLINE_CATALOG_REFERENCES_IDENTITY = OBJECT_MODEL_IDENTITY
PROJECT_ONLINE_CATALOG_REFERENCE = "Knx.Ets.OnlineCatalog.Download, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROJECT_STREAM_STORE_IDENTITY = "Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROJECT_TRACE_ENCRYPTOR_PIN = ("Knx.Ets.Common.dll", 503248, "4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd")
PROJECT_TRACE_ENCRYPTOR_IDENTITY = "Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"
PROBE_COMMAND = r"""
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAPROBE_RUNTIME' }
$writeDenied = $false
try { [IO.File]::WriteAllText($env:DCAMETA_WRITE_PROBE, 'owned denial probe') }
catch [UnauthorizedAccessException] { $writeDenied = $true }
$networkDenied = $false
$socket = [Net.Sockets.Socket]::new([Net.Sockets.AddressFamily]::InterNetwork, [Net.Sockets.SocketType]::Stream, [Net.Sockets.ProtocolType]::Tcp)
try { $socket.Connect([Net.IPAddress]::Loopback, 9) }
catch [Net.Sockets.SocketException] { $networkDenied = $_.Exception.SocketErrorCode -eq [Net.Sockets.SocketError]::AccessDenied }
finally { $socket.Dispose() }
if (-not ($writeDenied -and $networkDenied)) { throw 'DCAPROBE_RESTRICTION' }
[ordered]@{powershell_version='7.6.7'; write_denied=$writeDenied; network_denied=$networkDenied} | ConvertTo-Json -Compress
"""
METADATA_COMMAND = r"""
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAPROBE_RUNTIME' }
$pins = @($env:DCAMETA_INPUTS_JSON | ConvertFrom-Json)
if ($pins.Count -ne 3) { throw 'DCAPROBE_INPUT_COUNT' }
foreach ($pin in $pins) { if ([IO.FileInfo]::new($pin.path).Length -ne $pin.bytes) { throw 'DCAPROBE_INPUT_SIZE' } }
& $env:DCAMETA_WORKER -DcaPath $pins[0].path -DcaSha256 $pins[0].sha256 -ReferencePath @($pins[1].path,$pins[2].path) -ReferenceSha256 @($pins[1].sha256,$pins[2].sha256)
"""
PROJECT_COMMAND = METADATA_COMMAND.rstrip() + " -ProjectContext\n"
CONSTRUCTION_COMMAND = r"""
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAPROBE_RUNTIME' }
$pins = @($env:DCAMETA_INPUTS_JSON | ConvertFrom-Json)
if ($pins.Count -ne 4) { throw 'DCAPROBE_INPUT_COUNT' }
foreach ($pin in $pins) { if ([IO.FileInfo]::new($pin.path).Length -ne $pin.bytes) { throw 'DCAPROBE_INPUT_SIZE' } }
& $env:DCAMETA_WORKER -DcaPath $pins[0].path -DcaSha256 $pins[0].sha256 -ReferencePath @($pins[1].path,$pins[2].path,$pins[3].path) -ReferenceSha256 @($pins[1].sha256,$pins[2].sha256,$pins[3].sha256) -ProjectConstruction
"""
SERVICES_COMMAND = CONSTRUCTION_COMMAND.replace(" -ProjectConstruction\n", " -ProjectServices\n")
PROVIDERS_COMMAND = CONSTRUCTION_COMMAND.replace(" -ProjectConstruction\n", " -ProjectProviders\n")
CONSTRUCTORS_COMMAND = CONSTRUCTION_COMMAND.replace(" -ProjectConstruction\n", " -ProjectConstructors\n")
PROJECT_ID_COMMAND = r"""
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAPROBE_RUNTIME' }
$pins = @($env:DCAMETA_INPUTS_JSON | ConvertFrom-Json)
if ($pins.Count -ne 1) { throw 'DCAPROBE_INPUT_COUNT' }
if ([IO.FileInfo]::new($pins[0].path).Length -ne $pins[0].bytes) { throw 'DCAPROBE_INPUT_SIZE' }
& $env:DCAMETA_WORKER -InputPath $pins[0].path -InputSha256 $pins[0].sha256
"""


class ProbeError(Exception):
    """Static, path-free failure code suitable for public logs."""


def kill_group(process):
    """Only the process group created by this invocation is signaled."""
    try:
        os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError:
        pass
    process.wait(timeout=2)


def bounded_child(argv, env, *, timeout=DEADLINE, limit=LIMIT):
    """No shell; total stdout+stderr is bounded, including descendants' pipes."""
    process = subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=subprocess.PIPE,
                               stderr=subprocess.PIPE, env=env, start_new_session=True)
    output = {"stdout": bytearray(), "stderr": bytearray()}
    used = 0
    end = time.monotonic() + timeout
    try:
        with selectors.DefaultSelector() as selector:
            for stream, name in ((process.stdout, "stdout"), (process.stderr, "stderr")):
                os.set_blocking(stream.fileno(), False)
                selector.register(stream, selectors.EVENT_READ, name)
            while selector.get_map():
                remaining = end - time.monotonic()
                if remaining <= 0:
                    raise ProbeError("DCAPROBE_TIMEOUT")
                for key, _ in selector.select(min(remaining, 0.1)):
                    chunk = os.read(key.fd, min(65536, limit - used + 1))
                    if not chunk:
                        selector.unregister(key.fileobj)
                        continue
                    used += len(chunk)
                    if used > limit:
                        raise ProbeError("DCAPROBE_OUTPUT_LIMIT")
                    output[key.data].extend(chunk)
            remaining = end - time.monotonic()
            if remaining <= 0:
                raise ProbeError("DCAPROBE_TIMEOUT")
            try:
                code = process.wait(timeout=remaining)
            except subprocess.TimeoutExpired as error:
                raise ProbeError("DCAPROBE_TIMEOUT") from error
        # No background descendants may survive a completed research child.
        kill_group(process)
        return code, bytes(output["stdout"]), bytes(output["stderr"])
    except BaseException:
        kill_group(process)
        raise
    finally:
        process.stdout.close()
        process.stderr.close()


def checked_path(value):
    """Reject links in explicit absolute paths, without reading file contents."""
    path = Path(value)
    if not path.is_absolute() or ".." in path.parts:
        raise ProbeError("DCAPROBE_ABSOLUTE_PATH")
    for component in (path, *path.parents):
        if stat.S_ISLNK(component.lstat().st_mode):
            raise ProbeError("DCAPROBE_LINK_PATH")
    return path


def child_environment():
    env = {key: os.environ[key] for key in ("HOME", "TMPDIR", "LANG", "LC_ALL") if key in os.environ}
    env.update(PATH="/usr/bin:/bin", POWERSHELL_TELEMETRY_OPTOUT="1",
               POWERSHELL_UPDATECHECK="Off", DOTNET_EnableDiagnostics="0",
               COMPlus_EnableDiagnostics="0", DOTNET_CLI_TELEMETRY_OPTOUT="1",
               DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1")
    return env


def sandbox_command(pwsh, script):
    encoded = base64.b64encode(script.encode("utf-16le")).decode("ascii")
    return ["/usr/bin/sandbox-exec", "-p", PROFILE, str(pwsh), "-NoLogo", "-NoProfile",
            "-NonInteractive", "-EncodedCommand", encoded]


def parse_child(result, code):
    return_code, stdout, stderr = result
    if return_code != 0 or stderr:
        raise ProbeError(code)
    def unique_keys(pairs):
        obj = {}
        for key, value in pairs:
            if key in obj:
                raise ValueError("Duplicate JSON key")
            obj[key] = value
        return obj
    try:
        return json.loads(stdout.decode("utf-8-sig"), object_pairs_hook=unique_keys)
    except (ValueError, UnicodeError) as error:
        raise ProbeError("DCAPROBE_INVALID_JSON") from error


def negative_probes(pwsh, env):
    # This parent-created directory is private and proven writable by the parent.
    with tempfile.TemporaryDirectory(prefix="task140-denial-", dir="/private/tmp") as directory:
        baseline = Path(directory) / "parent-writable"
        baseline.write_bytes(b"owned baseline")
        if baseline.read_bytes() != b"owned baseline":
            raise ProbeError("DCAPROBE_PARENT_WRITE")
        baseline.unlink()
        probe_path = Path(directory) / "child-write-denied"
        probe_env = dict(env, DCAMETA_WRITE_PROBE=str(probe_path))
        result = parse_child(bounded_child(sandbox_command(pwsh, PROBE_COMMAND), probe_env), "DCAPROBE_RESTRICTION")
        if (result != {"powershell_version": "7.6.7", "write_denied": True, "network_denied": True}
                or probe_path.exists()):
            raise ProbeError("DCAPROBE_RESTRICTION")
        return result


def private_output(path, data):
    descriptor = os.open(path, os.O_WRONLY | os.O_CREAT | os.O_EXCL | os.O_NOFOLLOW, 0o600)
    try:
        with os.fdopen(descriptor, "wb") as output:
            output.write(data)
            output.flush()
            os.fsync(output.fileno())
    except BaseException:
        path.unlink()
        raise


def run(stage_value, pwsh_value, output_value=None, *, experiment="reference-map"):
    if experiment not in ("reference-map", "member-map", "entrypoint-map", "project-context", "project-construction", "project-services", "project-providers", "project-constructors", "project-id", "project-undo-manager", "project-undo-store-provider", "project-stream-store", "project-online-catalog-references", "project-trace-encryptor", "project-trace-encryptor-construction", "project-trace-encryptor-parameter"):
        raise ProbeError("DCAPROBE_EXPERIMENT")
    if sys.platform != "darwin":
        raise ProbeError("DCAPROBE_MACOS_REQUIRED")
    stage = checked_path(stage_value)
    attributes = stage.stat()
    if (not stat.S_ISDIR(attributes.st_mode) or attributes.st_uid != os.getuid()
            or stat.S_IMODE(attributes.st_mode) != 0o700):
        raise ProbeError("DCAPROBE_PRIVATE_STAGE")
    if any((parent / ".git").exists() for parent in (stage, *stage.parents)):
        raise ProbeError("DCAPROBE_STAGE_IN_GIT")
    pwsh = checked_path(pwsh_value)
    if not pwsh.is_file() or not os.access(pwsh, os.X_OK):
        raise ProbeError("DCAPROBE_RUNTIME_PATH")
    worker_base = Path(__file__).absolute().parent
    worker_path = {
        "reference-map": worker_base / "Read-ReferenceMetadata.ps1",
        "member-map": worker_base.parent / "dca-member-map" / "Read-MemberMetadata.ps1",
        "entrypoint-map": worker_base.parent / "dca-integration" / "Read-EntrypointMetadata.ps1",
        "project-context": worker_base.parent / "dca-integration" / "Read-EntrypointMetadata.ps1",
        "project-construction": worker_base.parent / "dca-integration" / "Read-EntrypointMetadata.ps1",
        "project-services": worker_base.parent / "dca-integration" / "Read-EntrypointMetadata.ps1",
        "project-providers": worker_base.parent / "dca-integration" / "Read-EntrypointMetadata.ps1",
        "project-constructors": worker_base.parent / "dca-integration" / "Read-EntrypointMetadata.ps1",
        "project-id": worker_base.parent / "dca-integration" / "Read-ProjectIdMetadata.ps1",
        "project-undo-manager": worker_base.parent / "dca-integration" / "Read-ProjectUndoManagerMetadata.ps1",
        "project-undo-store-provider": worker_base.parent / "dca-integration" / "Read-ProjectUndoStoreProviderMetadata.ps1",
        "project-stream-store": worker_base.parent / "dca-integration" / "Read-ProjectStreamStoreMetadata.ps1",
        "project-online-catalog-references": worker_base.parent / "dca-integration" / "Read-ProjectOnlineCatalogReferences.ps1",
        "project-trace-encryptor": worker_base.parent / "dca-integration" / "Read-ProjectTraceEncryptorMetadata.ps1",
        "project-trace-encryptor-construction": worker_base.parent / "dca-integration" / "Read-ProjectTraceEncryptorConstructionMetadata.ps1",
        "project-trace-encryptor-parameter": worker_base.parent / "dca-integration" / "Read-ProjectTraceEncryptorParameterMetadata.ps1",
    }[experiment]
    # Only fixed repository-owned experiments are selectable; never an arbitrary worker.
    worker = checked_path(worker_path)
    selected_pins = PINS + (OBJECT_MODEL_PIN,) if experiment in ("project-construction", "project-services", "project-providers", "project-constructors") else PINS
    if experiment == "project-id":
        selected_pins = (PROJECT_ID_PIN,)
    if experiment == "project-undo-manager":
        selected_pins = (PROJECT_UNDO_MANAGER_PIN,)
    if experiment == "project-undo-store-provider":
        selected_pins = (PROJECT_UNDO_STORE_PROVIDER_PIN,)
    if experiment == "project-stream-store":
        selected_pins = (PROJECT_STREAM_STORE_PIN,)
    if experiment == "project-online-catalog-references":
        selected_pins = (PROJECT_ONLINE_CATALOG_REFERENCES_PIN,)
    if experiment == "project-trace-encryptor":
        selected_pins = (PROJECT_TRACE_ENCRYPTOR_PIN,)
    if experiment == "project-trace-encryptor-construction":
        selected_pins = (PROJECT_TRACE_ENCRYPTOR_PIN,)
    if experiment == "project-trace-encryptor-parameter":
        selected_pins = (PROJECT_TRACE_ENCRYPTOR_PIN,)
    pins = []
    for filename, size, digest in selected_pins:
        path = checked_path(stage / filename)
        info = path.stat()
        if (not stat.S_ISREG(info.st_mode) or info.st_size != size or info.st_uid != os.getuid()
                or stat.S_IMODE(info.st_mode) != 0o600):
            raise ProbeError("DCAPROBE_INPUT_ATTRIBUTES")
        pins.append({"filename": filename, "path": str(path), "bytes": size, "sha256": digest})
    output_path = None
    if output_value is not None:
        output_path = Path(output_value)
        if not output_path.is_absolute() or output_path.parent != stage or output_path.name in {pin[0] for pin in selected_pins}:
            raise ProbeError("DCAPROBE_OUTPUT_PATH")
        if output_path.exists() or output_path.is_symlink():
            raise ProbeError("DCAPROBE_OUTPUT_EXISTS")
    env = child_environment()
    probes = negative_probes(pwsh, env)
    env.update(DCAMETA_INPUTS_JSON=json.dumps(pins), DCAMETA_WORKER=str(worker))
    command = CONSTRUCTORS_COMMAND if experiment == "project-constructors" else PROVIDERS_COMMAND if experiment == "project-providers" else SERVICES_COMMAND if experiment == "project-services" else CONSTRUCTION_COMMAND if experiment == "project-construction" else PROJECT_COMMAND if experiment == "project-context" else METADATA_COMMAND
    if experiment == "project-id":
        command = PROJECT_ID_COMMAND
    if experiment == "project-undo-manager":
        command = PROJECT_ID_COMMAND
    if experiment == "project-undo-store-provider":
        command = PROJECT_ID_COMMAND
    if experiment == "project-stream-store":
        command = PROJECT_ID_COMMAND
    if experiment == "project-online-catalog-references":
        command = PROJECT_ID_COMMAND
    if experiment == "project-trace-encryptor":
        command = PROJECT_ID_COMMAND
    if experiment == "project-trace-encryptor-construction":
        command = PROJECT_ID_COMMAND
    if experiment == "project-trace-encryptor-parameter":
        command = PROJECT_ID_COMMAND
    raw = bounded_child(sandbox_command(pwsh, command), env)
    report = parse_child(raw, "DCAPROBE_METADATA_FAILED")
    if (not isinstance(report, dict) or type(report.get("schema_version")) is not int
            or report["schema_version"] != 1 or report.get("operation") != {
                "reference-map": "static_candidate_contract_map",
                "member-map": "static_candidate_member_map",
                "entrypoint-map": "static_dca_entrypoint_map",
                "project-context": "static_dca_project_context",
                "project-construction": "static_dca_project_construction",
                "project-services": "static_dca_project_services",
                "project-providers": "static_dca_project_providers",
                "project-constructors": "static_dca_project_constructors",
                "project-id": "static_dca_project_id",
                "project-undo-manager": "static_dca_project_undo_manager",
                "project-undo-store-provider": "static_dca_project_undo_store_provider",
                "project-stream-store": "static_dca_project_stream_store",
                "project-online-catalog-references": "static_dca_project_online_catalog_references",
                "project-trace-encryptor": "static_dca_project_trace_encryptor",
                "project-trace-encryptor-construction": "static_dca_project_trace_encryptor_construction",
                "project-trace-encryptor-parameter": "static_dca_project_trace_encryptor_parameter",
            }[experiment]
            or report.get("runtime_execution") != "NOT_RUN" or report.get("rights") != "UNRESOLVED"):
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    observed = report.get("inputs", [])
    expected = [(name, size, digest) for name, size, digest in selected_pins]
    if (not isinstance(observed, list) or not all(isinstance(item, dict) for item in observed)
            or [(item.get("filename"), item.get("bytes"), item.get("sha256")) for item in observed] != expected):
        raise ProbeError("DCAPROBE_REPORT_PINS")
    if experiment in ("project-construction", "project-services", "project-providers", "project-constructors") and observed[3].get("identity") != OBJECT_MODEL_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-id" and observed[0].get("identity") != PROJECT_ID_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-id" and report.get("selected_root") != "Knx.Ets.Ids.Project.ProjectId":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-undo-manager" and observed[0].get("identity") != PROJECT_UNDO_MANAGER_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-undo-manager" and report.get("selected_root") != "Knx.Ets.ObjectModel.UndoRedo.UndoManager":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-undo-store-provider" and observed[0].get("identity") != PROJECT_UNDO_STORE_PROVIDER_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-undo-store-provider" and report.get("selected_root") != "Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-online-catalog-references" and observed[0].get("identity") != PROJECT_ONLINE_CATALOG_REFERENCES_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-online-catalog-references" and report.get("selected_assembly_reference") != PROJECT_ONLINE_CATALOG_REFERENCE:
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-stream-store" and observed[0].get("identity") != PROJECT_STREAM_STORE_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-stream-store" and report.get("selected_root") != "Knx.Ets.ObjectModel.IStreamStore":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-trace-encryptor" and observed[0].get("identity") != PROJECT_TRACE_ENCRYPTOR_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-trace-encryptor" and report.get("selected_root") != "Knx.Ets.Common.Security.IProjectTraceEncryptor":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-trace-encryptor-construction" and observed[0].get("identity") != PROJECT_TRACE_ENCRYPTOR_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-trace-encryptor-construction" and report.get("selected_root") != "Knx.Ets.Common.Security.ProjectTraceEncryptor":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    if experiment == "project-trace-encryptor-parameter" and observed[0].get("identity") != PROJECT_TRACE_ENCRYPTOR_IDENTITY:
        raise ProbeError("DCAPROBE_REPORT_IDENTITY")
    if experiment == "project-trace-encryptor-parameter" and report.get("selected_root") != "Knx.Ets.Common.Security.ProjectTraceEncryptor":
        raise ProbeError("DCAPROBE_REPORT_SHAPE")
    result = {"restriction_probes": probes, "metadata": report}
    payload = (json.dumps(result, ensure_ascii=True, separators=(",", ":")) + "\n").encode("utf-8")
    if len(payload) > LIMIT:
        raise ProbeError("DCAPROBE_OUTPUT_LIMIT")
    if output_path is not None:
        private_output(output_path, payload)
    else:
        sys.stdout.buffer.write(payload)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--stage", required=True)
    parser.add_argument("--pwsh", required=True)
    parser.add_argument("--output", help="New private report path immediately inside stage")
    parser.add_argument("--experiment", choices=("reference-map", "member-map", "entrypoint-map", "project-context", "project-construction", "project-services", "project-providers", "project-constructors", "project-id", "project-undo-manager", "project-undo-store-provider", "project-stream-store", "project-online-catalog-references", "project-trace-encryptor", "project-trace-encryptor-construction", "project-trace-encryptor-parameter"), default="reference-map",
                        help="Fixed owned experiment; Task-140 default is unchanged")
    args = parser.parse_args()
    try:
        run(args.stage, args.pwsh, args.output, experiment=args.experiment)
    except KeyboardInterrupt:
        print("DCAPROBE_CANCELLED", file=sys.stderr)
        return 130
    except ProbeError as error:
        print(str(error), file=sys.stderr)
        return 1
    except (OSError, ValueError, TypeError):
        print("DCAPROBE_IO_OR_REPORT", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
