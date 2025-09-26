# AC-03: Restore quality automation scripts

- **Priority:** P1
- **Labels:** `quality`, `automation`
- **Owner:** Platform Team
- **Depends on:** AC-01

## Context

* `Directory.Build.props` invokes `repo_audit.py` and `scripts/enforce_quality_gates.py`, which are absent from the repository.
* `code-metrics` workflow references `scripts/generate_metrics_summary.py` and fails today.
* Quality workflow expects `scripts/check_quality.ps1`, also missing.

## Tasks

1. Implement the missing Python/PowerShell scripts or remove the hooks if superseded.
2. Ensure scripts write outputs to `build/logs`/`reports` directories expected by workflows.
3. Add unit tests or smoke tests for the scripts themselves to validate exit codes.
4. Document usage in README/CONTRIBUTING and align script parameters with CI secrets.

## Acceptance Criteria

- Invocations from MSBuild targets and GitHub workflows complete successfully.
- Generated reports (audit, metrics) are uploaded as CI artifacts.
- Failing quality gates produce actionable error messages and non-zero exit codes.

