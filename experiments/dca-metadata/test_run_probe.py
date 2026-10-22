#!/usr/bin/env python3
"""Owned process/file fixtures only; no real inputs or sandbox escalation."""

import base64
import json
import os
from pathlib import Path
import signal
import stat
import subprocess
import sys
import tempfile
import time
import unittest
from unittest import mock

import run_probe


class CaptureTests(unittest.TestCase):
    def child(self, code, *arguments, **limits):
        return run_probe.bounded_child([sys.executable, "-c", code, *arguments], {}, **limits)

    def test_stdout_stderr_and_exit_status(self):
        code, stdout, stderr = self.child("import sys; print('owned'); print('diagnostic', file=sys.stderr); sys.exit(4)")
        self.assertEqual((code, stdout, stderr), (4, b"owned\n", b"diagnostic\n"))

    def test_combined_output_limit(self):
        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_OUTPUT_LIMIT$"):
            self.child("import os; os.write(1,b'a'*600); os.write(2,b'b'*600)", limit=1024)

    def test_timeout_with_closed_pipes(self):
        start = time.monotonic()
        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_TIMEOUT$"):
            self.child("import os,time; os.close(1); os.close(2); time.sleep(20)", timeout=0.15)
        self.assertLess(time.monotonic() - start, 3)

    def test_timeout_kills_own_descendant_group(self):
        with tempfile.TemporaryDirectory() as directory:
            launched = Path(directory) / "launched"
            survivor = Path(directory) / "survivor"
            descendant = "import pathlib,sys,time; time.sleep(0.8); pathlib.Path(sys.argv[1]).write_text('must not survive'); time.sleep(20)"
            parent = ("import pathlib,subprocess,sys,time; "
                      "subprocess.Popen([sys.executable,'-c',sys.argv[1],sys.argv[3]]); "
                      "pathlib.Path(sys.argv[2]).write_text('launched'); time.sleep(20)")
            with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_TIMEOUT$"):
                self.child(parent, descendant, str(launched), str(survivor), timeout=0.3)
            self.assertTrue(launched.exists())
            time.sleep(0.9)
            self.assertFalse(survivor.exists())

    def test_cancellation_kills_own_child(self):
        processes = []
        original = subprocess.Popen

        def spawn(*args, **kwargs):
            child = original(*args, **kwargs)
            processes.append(child)
            return child

        with mock.patch.object(run_probe.subprocess, "Popen", side_effect=spawn):
            with mock.patch.object(run_probe.selectors.DefaultSelector, "select", side_effect=KeyboardInterrupt):
                with self.assertRaises(KeyboardInterrupt):
                    self.child("import time; time.sleep(20)")
        self.assertEqual(len(processes), 1)
        self.assertEqual(processes[0].returncode, -signal.SIGKILL)


class PolicyTests(unittest.TestCase):
    def test_fixed_script_and_environment_data_never_become_shell_code(self):
        malicious = 'a"; $(touch SHOULD_NOT_EXIST); `whoami`'
        command = run_probe.sandbox_command(Path("/owned/pwsh"), run_probe.METADATA_COMMAND)
        self.assertEqual(command[:3], ["/usr/bin/sandbox-exec", "-p", run_probe.PROFILE])
        decoded = base64.b64decode(command[-1]).decode("utf-16le")
        self.assertEqual(decoded, run_probe.METADATA_COMMAND)
        self.assertNotIn(malicious, decoded)
        self.assertNotIn("shell", command)
        with mock.patch.dict(os.environ, {"DOTNET_STARTUP_HOOKS": malicious, "PSModulePath": malicious}):
            environment = run_probe.child_environment()
        self.assertNotIn("DOTNET_STARTUP_HOOKS", environment)
        self.assertNotIn("PSModulePath", environment)
        self.assertEqual(environment["DOTNET_EnableDiagnostics"], "0")
        self.assertEqual(run_probe.PROFILE, "(version 1)(deny default)(allow file-read*)(allow process*)(allow sysctl-read)(allow mach-lookup)")

    def test_only_fixed_member_experiment_is_available(self):
        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_EXPERIMENT$"):
            run_probe.run("/unused", "/unused", experiment="arbitrary-worker.ps1")
        with tempfile.TemporaryDirectory() as directory:
            stage = Path(directory).resolve()
            stage.chmod(0o700)
            pwsh = stage / "owned-pwsh"
            pwsh.write_bytes(b"never executed")
            pwsh.chmod(0o700)
            pins = (("Owned.Dca.dll", 1, "0" * 64), ("Owned.Sdk.dll", 1, "1" * 64))
            for name, _, _ in pins:
                path = stage / name
                path.write_bytes(b"x")
                path.chmod(0o600)
            report = {"schema_version": 1, "operation": "static_candidate_member_map",
                      "runtime_execution": "NOT_RUN", "rights": "UNRESOLVED", "inputs": [
                          {"filename": name, "bytes": size, "sha256": digest} for name, size, digest in pins]}
            with mock.patch.object(run_probe.sys, "platform", "darwin"), mock.patch.object(run_probe, "PINS", pins):
                with mock.patch.object(run_probe, "negative_probes", side_effect=run_probe.ProbeError("DCAPROBE_RESTRICTION")):
                    with mock.patch.object(run_probe, "bounded_child") as child:
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
                            run_probe.run(str(stage), str(pwsh), experiment="member-map")
                        child.assert_not_called()
                with mock.patch.object(run_probe, "negative_probes", return_value={"owned": True}):
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(report).encode(), b"")) as child:
                        run_probe.run(str(stage), str(pwsh), str(stage / "report.json"), experiment="member-map")
                        self.assertEqual(Path(child.call_args.args[1]["DCAMETA_WORKER"]).name, "Read-MemberMetadata.ps1")
                        self.assertEqual(child.call_args.args[0], run_probe.sandbox_command(pwsh, run_probe.METADATA_COMMAND))
                    wrong = dict(report, operation="static_candidate_contract_map")
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(wrong).encode(), b"")):
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_REPORT_SHAPE$"):
                            run_probe.run(str(stage), str(pwsh), experiment="member-map")

    def test_entrypoint_experiment_is_fixed_and_restriction_gated(self):
        with tempfile.TemporaryDirectory() as directory:
            stage = Path(directory).resolve()
            stage.chmod(0o700)
            pwsh = stage / "owned-pwsh"
            pwsh.write_bytes(b"never executed")
            pwsh.chmod(0o700)
            pins = (("Owned.Dca.dll", 1, "0" * 64), ("Owned.Sdk.dll", 1, "1" * 64))
            for name, _, _ in pins:
                path = stage / name
                path.write_bytes(b"x")
                path.chmod(0o600)
            report = {"schema_version": 1, "operation": "static_dca_entrypoint_map",
                      "runtime_execution": "NOT_RUN", "rights": "UNRESOLVED", "inputs": [
                          {"filename": name, "bytes": size, "sha256": digest} for name, size, digest in pins]}
            with mock.patch.object(run_probe.sys, "platform", "darwin"), mock.patch.object(run_probe, "PINS", pins):
                with mock.patch.object(run_probe, "negative_probes", side_effect=run_probe.ProbeError("DCAPROBE_RESTRICTION")):
                    with mock.patch.object(run_probe, "bounded_child") as child:
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
                            run_probe.run(str(stage), str(pwsh), experiment="entrypoint-map")
                        child.assert_not_called()
                with mock.patch.object(run_probe, "negative_probes", return_value={"owned": True}):
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(report).encode(), b"")) as child:
                        run_probe.run(str(stage), str(pwsh), str(stage / "report.json"), experiment="entrypoint-map")
                        self.assertEqual(Path(child.call_args.args[1]["DCAMETA_WORKER"]).name, "Read-EntrypointMetadata.ps1")
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(dict(report, operation="static_candidate_member_map")).encode(), b"")):
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_REPORT_SHAPE$"):
                            run_probe.run(str(stage), str(pwsh), experiment="entrypoint-map")

    def test_project_experiment_is_fixed_and_restriction_gated(self):
        with tempfile.TemporaryDirectory() as directory:
            stage = Path(directory).resolve()
            stage.chmod(0o700)
            pwsh = stage / "owned-pwsh"
            pwsh.write_bytes(b"never executed")
            pwsh.chmod(0o700)
            pins = (("Owned.Dca.dll", 1, "0" * 64), ("Owned.Sdk.dll", 1, "1" * 64))
            for name, _, _ in pins:
                path = stage / name
                path.write_bytes(b"x")
                path.chmod(0o600)
            report = {"schema_version": 1, "operation": "static_dca_project_context",
                      "runtime_execution": "NOT_RUN", "rights": "UNRESOLVED", "inputs": [
                          {"filename": name, "bytes": size, "sha256": digest} for name, size, digest in pins]}
            with mock.patch.object(run_probe.sys, "platform", "darwin"), mock.patch.object(run_probe, "PINS", pins):
                with mock.patch.object(run_probe, "negative_probes", side_effect=run_probe.ProbeError("DCAPROBE_RESTRICTION")):
                    with mock.patch.object(run_probe, "bounded_child") as child:
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
                            run_probe.run(str(stage), str(pwsh), experiment="project-context")
                        child.assert_not_called()
                with mock.patch.object(run_probe, "negative_probes", return_value={"owned": True}):
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(report).encode(), b"")) as child:
                        run_probe.run(str(stage), str(pwsh), str(stage / "report.json"), experiment="project-context")
                        self.assertEqual(Path(child.call_args.args[1]["DCAMETA_WORKER"]).name, "Read-EntrypointMetadata.ps1")
                        command = base64.b64decode(child.call_args.args[0][-1]).decode("utf-16le")
                        self.assertEqual(command, run_probe.PROJECT_COMMAND)
                        self.assertTrue(command.endswith(" -ProjectContext\n"))
                        self.assertEqual(child.call_args.args[0][2], run_probe.PROFILE)
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(dict(report, operation="static_candidate_member_map")).encode(), b"")):
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_REPORT_SHAPE$"):
                            run_probe.run(str(stage), str(pwsh), experiment="project-context")

    def test_construction_mode_has_four_fixed_pins_and_restriction_first(self):
        self.assert_four_input_mode("project-construction", "static_dca_project_construction",
                                    run_probe.CONSTRUCTION_COMMAND, " -ProjectConstruction")

    def test_services_mode_has_four_fixed_pins_and_restriction_first(self):
        self.assert_four_input_mode("project-services", "static_dca_project_services",
                                    run_probe.SERVICES_COMMAND, " -ProjectServices")
        self.assertNotEqual(run_probe.SERVICES_COMMAND, run_probe.CONSTRUCTION_COMMAND)
        self.assertNotIn(" -ProjectConstruction", run_probe.SERVICES_COMMAND)

    def test_providers_mode_has_four_fixed_pins_and_restriction_first(self):
        self.assert_four_input_mode("project-providers", "static_dca_project_providers",
                                    run_probe.PROVIDERS_COMMAND, " -ProjectProviders")
        self.assertNotEqual(run_probe.PROVIDERS_COMMAND, run_probe.CONSTRUCTION_COMMAND)
        self.assertNotIn(" -ProjectServices", run_probe.PROVIDERS_COMMAND)

    def test_constructors_mode_has_four_fixed_pins_and_restriction_first(self):
        self.assert_four_input_mode("project-constructors", "static_dca_project_constructors",
                                    run_probe.CONSTRUCTORS_COMMAND, " -ProjectConstructors")
        self.assertNotEqual(run_probe.CONSTRUCTORS_COMMAND, run_probe.CONSTRUCTION_COMMAND)
        self.assertNotIn(" -ProjectProviders", run_probe.CONSTRUCTORS_COMMAND)

    def assert_four_input_mode(self, experiment, operation, expected_command, switch):
        self.assertEqual(len(run_probe.PINS), 3)
        self.assertNotIn(run_probe.OBJECT_MODEL_PIN, run_probe.PINS)
        with tempfile.TemporaryDirectory() as directory:
            stage = Path(directory).resolve()
            stage.chmod(0o700)
            pwsh = stage / "owned-pwsh"
            pwsh.write_bytes(b"never executed")
            pwsh.chmod(0o700)
            pins = tuple(("Owned" + str(i) + ".dll", 1, str(i) * 64) for i in range(3))
            object_model = ("OwnedObjectModel.dll", 1, "3" * 64)
            for name, _, _ in (*pins, object_model):
                path = stage / name
                path.write_bytes(b"x")
                path.chmod(0o600)
            report = {"schema_version": 1, "operation": operation,
                      "runtime_execution": "NOT_RUN", "rights": "UNRESOLVED", "inputs": [
                          {"filename": name, "bytes": size, "sha256": digest}
                          for name, size, digest in (*pins, object_model)]}
            report["inputs"][3]["identity"] = run_probe.OBJECT_MODEL_IDENTITY
            with mock.patch.object(run_probe.sys, "platform", "darwin"), \
                    mock.patch.object(run_probe, "PINS", pins), \
                    mock.patch.object(run_probe, "OBJECT_MODEL_PIN", object_model):
                with mock.patch.object(run_probe, "negative_probes", side_effect=run_probe.ProbeError("DCAPROBE_RESTRICTION")):
                    with mock.patch.object(run_probe, "bounded_child") as child:
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
                            run_probe.run(str(stage), str(pwsh), experiment=experiment)
                        child.assert_not_called()
                with mock.patch.object(run_probe, "negative_probes", return_value={"owned": True}):
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(report).encode(), b"")) as child:
                        run_probe.run(str(stage), str(pwsh), str(stage / "report.json"), experiment=experiment)
                        command = base64.b64decode(child.call_args.args[0][-1]).decode("utf-16le")
                        self.assertEqual(command, expected_command)
                        self.assertIn("$pins.Count -ne 4", command)
                        self.assertIn(switch, command)
                        self.assertEqual(child.call_args.args[0][2], run_probe.PROFILE)
                        environment = child.call_args.args[1]
                        observed = json.loads(environment["DCAMETA_INPUTS_JSON"])
                        self.assertEqual([(p["filename"], p["bytes"], p["sha256"]) for p in observed], list((*pins, object_model)))
                        self.assertEqual(Path(environment["DCAMETA_WORKER"]).name, "Read-EntrypointMetadata.ps1")
                    for field, value, code in (("identity", "Owned.Wrong", "REPORT_IDENTITY"), ("sha256", "f" * 64, "REPORT_PINS")):
                        changed = json.loads(json.dumps(report))
                        changed["inputs"][3][field] = value
                        with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(changed).encode(), b"")):
                            with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_" + code + "$"):
                                run_probe.run(str(stage), str(pwsh), experiment=experiment)
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(dict(report, operation="static_dca_project_context")).encode(), b"")):
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_REPORT_SHAPE$"):
                            run_probe.run(str(stage), str(pwsh), experiment=experiment)
                with mock.patch.object(run_probe, "bounded_child") as child:
                    with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_OUTPUT_PATH$"):
                        run_probe.run(str(stage), str(pwsh), str(stage / object_model[0]), experiment=experiment)
                    child.assert_not_called()

    def test_probe_failure_is_closed(self):
        result = (0, b'{"powershell_version":"7.6.7","write_denied":true,"network_denied":false}', b"")
        with mock.patch.object(run_probe, "bounded_child", return_value=result):
            with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
                run_probe.negative_probes(Path("/owned/pwsh"), {})

    def test_stderr_and_invalid_json_are_failures(self):
        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
            run_probe.parse_child((0, b"{}", b"warning"), "DCAPROBE_RESTRICTION")
        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_INVALID_JSON$"):
            run_probe.parse_child((0, b"not-json", b""), "DCAPROBE_RESTRICTION")
        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_INVALID_JSON$"):
            run_probe.parse_child((0, b'{"nested":{"rights":"UNRESOLVED","rights":"APPROVED"}}', b""), "DCAPROBE_RESTRICTION")

    def test_private_output_is_exclusive_and_owner_only(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "owned.json"
            run_probe.private_output(output, b"owned")
            self.assertEqual(stat.S_IMODE(output.stat().st_mode), 0o600)
            with self.assertRaises(FileExistsError):
                run_probe.private_output(output, b"replacement")
            self.assertEqual(output.read_bytes(), b"owned")

    def test_link_path_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            parent = Path(directory).resolve()
            target = parent / "target"
            target.write_bytes(b"owned")
            link = parent / "link"
            link.symlink_to(target)
            with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_LINK_PATH$"):
                run_probe.checked_path(link)

    def test_run_gates_metadata_and_pins_using_only_owned_files(self):
        with tempfile.TemporaryDirectory() as directory:
            stage = Path(directory).resolve()
            stage.chmod(0o700)
            pwsh = stage / "owned-pwsh"
            pwsh.write_bytes(b"owned, never executed")
            pwsh.chmod(0o700)
            pins = (("Owned.Dca.dll", 1, "0" * 64), ("Owned.Reference.dll", 1, "1" * 64))
            for name, _, _ in pins:
                file = stage / name
                file.write_bytes(b"x")
                file.chmod(0o600)
            output = stage / "private.json"
            report = {"schema_version": 1, "operation": "static_candidate_contract_map",
                      "runtime_execution": "NOT_RUN", "rights": "UNRESOLVED", "inputs": [
                {"filename": name, "bytes": size, "sha256": digest} for name, size, digest in pins]}
            with mock.patch.object(run_probe.sys, "platform", "darwin"), mock.patch.object(run_probe, "PINS", pins):
                with mock.patch.object(run_probe, "negative_probes", side_effect=run_probe.ProbeError("DCAPROBE_RESTRICTION")):
                    with mock.patch.object(run_probe, "bounded_child") as child:
                        with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_RESTRICTION$"):
                            run_probe.run(str(stage), str(pwsh), str(output))
                        child.assert_not_called()
                        self.assertFalse(output.exists())
                with mock.patch.object(run_probe, "negative_probes", return_value={"owned": True}):
                    for key, value in (("schema_version", 2), ("runtime_execution", "PASS"), ("rights", "APPROVED")):
                        malformed = dict(report, **{key: value})
                        with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(malformed).encode(), b"")):
                            with self.assertRaisesRegex(run_probe.ProbeError, "^DCAPROBE_REPORT_SHAPE$"):
                                run_probe.run(str(stage), str(pwsh), str(output))
                            self.assertFalse(output.exists())
                    with mock.patch.object(run_probe, "bounded_child", return_value=(0, json.dumps(report).encode(), b"")) as child:
                        run_probe.run(str(stage), str(pwsh), str(output))
                        environment = child.call_args.args[1]
                        self.assertEqual(json.loads(environment["DCAMETA_INPUTS_JSON"])[0]["path"], str(stage / pins[0][0]))
                        self.assertEqual(child.call_args.args[0], run_probe.sandbox_command(pwsh, run_probe.METADATA_COMMAND))
            self.assertEqual(stat.S_IMODE(output.stat().st_mode), 0o600)
            self.assertEqual(json.loads(output.read_bytes())["metadata"], report)


if __name__ == "__main__":
    unittest.main()
