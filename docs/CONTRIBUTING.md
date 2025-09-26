# Contributing Guidelines

Welcome to AssemblyChain! This document captures the expectations for collaborators and the checks that gate our CI pipeline.

## Coding Standards
- **C# style**: prefer explicit access modifiers, expression-bodied members for small accessors, and `var` only when the type is evident from the right-hand side.
- **Documentation**: public APIs require XML doc comments; keep summary lines concise and include parameter remarks when semantics are non-trivial.
- **Testing**: add unit tests for new solver/contact logic and update benchmarks when performance-sensitive code changes.
- **Naming**: use PascalCase for classes, interfaces, and methods; camelCase for locals and parameters; avoid abbreviations unless industry-standard (e.g., `CSP`, `SAT`).

## Quality Gates
Our hardened `scripts/check_quality.ps1` enforces the following thresholds:
- Dependency cycles: **0** allowed.
- Duplicate fragments: **≤ 20**.
- Max cyclomatic complexity per method: **≤ 15**.
- Max method length: **≤ 120** lines.
- Public API documentation coverage: **≥ 25%**.

The script restores tools, builds, runs unit tests, executes the repository audit, and launches a short benchmark smoke test. Execute it locally before opening a PR:

```powershell
pwsh ./scripts/check_quality.ps1
```

## Pull Request Checklist
- [ ] Run `scripts/check_quality.ps1` and address any reported violations.
- [ ] Update or add unit tests/benchmarks relevant to your change.
- [ ] Refresh documentation or samples impacted by your change.
- [ ] Append to `reports/development_summary.md` with a section for each major task completed (use the existing template).
- [ ] Provide context in the PR description, including benchmark deltas when performance is affected.

Thanks for helping keep AssemblyChain healthy and predictable!
