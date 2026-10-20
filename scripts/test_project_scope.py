"""Focused synthetic regressions for the offline Task-001 planning checks."""

from pathlib import Path
import shutil
import tempfile
import unittest

from check_project_scope import validate


class ProjectScopeChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="knx-scope-check-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        source = Path(__file__).resolve().parents[1]
        for directory in ("docs", "tasks", "templates", "config"):
            shutil.copytree(source / directory, self.root / directory)
        for path in [*source.glob("*.md"), source / "LICENSE", source / "NOTICE"]:
            if path.is_file():
                shutil.copy2(path, self.root / path.name)
        # Script paths may be linked from the documentation, but are never executed.
        shutil.copytree(source / "scripts", self.root / "scripts",
                        ignore=shutil.ignore_patterns("__pycache__"))
        self.scope = self.root / "config/project-scope.toml"

    def test_current_baseline_and_navigation(self):
        self.assertEqual(validate(self.root), [])

    def test_secure_scope_drift_without_adr_is_rejected(self):
        self.scope.write_text(self.scope.read_text().replace(
            "knx_ip_secure = true", "knx_ip_secure = false"))
        self.assertTrue(any("approved ADR" in error for error in validate(self.root)))

    def test_claimed_adr_does_not_grant_approval(self):
        self.scope.write_text(self.scope.read_text().replace("Rust", "Java"))
        (self.root / "docs/ADR-999.md").write_text("# ADR-999\n\nAccepted by an agent.\n")
        self.assertTrue(any("Scope baseline changed" in error for error in validate(self.root)))

    def test_comments_do_not_change_scope(self):
        self.scope.write_text(self.scope.read_text() + "\n# Formatting-only edit.\n")
        self.assertEqual(validate(self.root), [])

    def test_missing_or_malformed_scope_is_rejected(self):
        self.scope.write_text("not valid = [")
        self.assertTrue(any("project-scope.toml" in error for error in validate(self.root)))
        self.scope.unlink()
        self.assertTrue(any("project-scope.toml" in error for error in validate(self.root)))

    def test_broken_local_link_is_rejected(self):
        with (self.root / "README.md").open("a") as output:
            output.write("\n[Missing](docs/does-not-exist.md)\n")
        self.assertTrue(any("unresolved local link" in error for error in validate(self.root)))

    def test_reference_link_is_checked_without_network(self):
        with (self.root / "README.md").open("a") as output:
            output.write("\n[External](https://example.invalid/)\n"
                         "[Missing][missing]\n[missing]: docs/does-not-exist.md\n")
        errors = validate(self.root)
        self.assertEqual(len(errors), 1)
        self.assertIn("docs/does-not-exist.md", errors[0])

    def test_duplicate_task_identity_is_rejected(self):
        task = self.root / "tasks/Task-002.md"
        task.write_text(task.read_text().replace("| Task ID | Task-002 |",
                                                "| Task ID | Task-001 |"))
        errors = validate(self.root)
        self.assertTrue(any("duplicate task ID" in error for error in errors))

    def test_nested_git_repository_is_not_editor_documentation(self):
        nested = self.root / "knust-fork"
        nested.mkdir()
        (nested / ".git").mkdir()
        (nested / "README.md").write_text("[Upstream](missing-upstream-document.md)\n")
        self.assertEqual(validate(self.root), [])

    def test_duplicate_task_index_link_is_rejected(self):
        with (self.root / "TASKS.md").open("a") as output:
            output.write("\n| [Task-001](tasks/Task-001.md) | Duplicate entry |\n")
        self.assertTrue(any("exactly once" in error for error in validate(self.root)))


if __name__ == "__main__":
    unittest.main()
