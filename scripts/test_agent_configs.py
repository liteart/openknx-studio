"""Regression checks for routing and configuration validation; offline only."""

from pathlib import Path
import shutil
import tempfile
import unittest

from check_agent_configs import validate


class AgentConfigChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="knx-agent-check-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        source = Path(__file__).resolve().parents[1] / "config/codex"
        shutil.copytree(source, self.root / "config/codex")
        self.agent = self.root / "config/codex/agents/explorer.toml"
        self.original = self.agent.read_text()

    def test_valid_templates(self):
        self.assertEqual(validate(self.root), [])

    def test_disallowed_model(self):
        self.agent.write_text(self.original.replace("gpt-6-luna", "gpt-6-astra"))
        self.assertTrue(any("unexpected model" in e for e in validate(self.root)))

    def test_wrong_identity(self):
        self.agent.write_text(self.original.replace('name = "explorer"', 'name = "reviewer"'))
        self.assertTrue(any("role name" in e for e in validate(self.root)))

    def test_permission_override(self):
        self.agent.write_text(self.original + '\nsandbox_mode = "danger-full-access"\n')
        self.assertTrue(any("unsupported" in e for e in validate(self.root)))

    def test_malformed_toml(self):
        self.agent.write_text("invalid TOML = [")
        self.assertTrue(validate(self.root))

    def test_installed_drift(self):
        shutil.copytree(self.root / "config/codex", self.root / ".codex")
        self.assertEqual(validate(self.root), [])
        (self.root / ".codex/agents/explorer.toml").write_text(self.original + "\n# drift\n")
        self.assertTrue(any("differs from template" in e for e in validate(self.root)))

    def test_unexpected_installed_role(self):
        shutil.copytree(self.root / "config/codex", self.root / ".codex")
        (self.root / ".codex/agents/extra.toml").write_text(
            self.original.replace('name = "explorer"', 'name = "extra"')
        )
        self.assertTrue(any("unexpected installed" in e for e in validate(self.root)))

    def test_missing_role(self):
        self.agent.unlink()
        self.assertTrue(any("Missing" in e for e in validate(self.root)))

    def test_changed_defaults(self):
        config = self.root / "config/codex/config.toml"
        config.write_text(config.read_text().replace("session = 3", "session = 30"))
        self.assertTrue(any("defaults differ" in e for e in validate(self.root)))


if __name__ == "__main__":
    unittest.main()
