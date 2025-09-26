# AC-01: Stabilise Windows build & CI pipeline

- **Priority:** P0
- **Labels:** `ci`, `platform`, `blocking`
- **Owner:** Platform Team
- **Depends on:** None

## Context

* Solution targets `net7.0-windows` (`Directory.Build.props`).
* Linux workflows (`.github/workflows/dotnet.yml`, `code-metrics.yml`, `nightly-benchmarks.yml`) cannot compile Rhino-dependent projects.
* Missing helper scripts (`repo_audit.py`, `scripts/generate_metrics_summary.py`, `scripts/check_quality.ps1`) break CI steps.

## Tasks

1. Switch all build/test workflows to `windows-latest` runners and ensure .NET 7 SDK is installed (use `actions/setup-dotnet@v4`).
2. Provision Rhino 8 SDK or stub assemblies required for compilation.
3. Replace missing script references with working equivalents or remove the steps.
4. Validate `dotnet restore`, `dotnet build`, and `dotnet test` succeed on clean checkout.

## Acceptance Criteria

- CI pipeline completes successfully on pull requests and main branch.
- Build/test steps run on Windows and pass without manual intervention.
- All workflow steps referenced in YAML exist and execute without `file not found` errors.
- Build artifacts (plugin DLL/gha) are generated and uploaded for inspection.

