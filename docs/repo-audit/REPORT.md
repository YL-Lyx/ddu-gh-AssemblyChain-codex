# AssemblyChain Repository Audit Report

_Last updated: 2025-09-26T20:04:38Z_

## Executive Summary

The AssemblyChain mono-repo houses ten .NET 7 projects spanning core planning logic, Rhino/Grasshopper UI integration, and supporting toolkits, orchestrated through the `AssemblyChain-Core.sln` solution. The codebase is Windows-focused (`net7.0-windows`) and tightly coupled to RhinoCommon/Grasshopper APIs. The current CI configuration and documentation do not reflect these platform constraints, producing a fragile developer experience and unreliable automation. Several GitHub workflows reference missing helper scripts, preventing reproducible quality gates. Tests rely on OR-Tools and Rhino-specific types, yet no deterministic setup or environment guidance exists. Security posture is moderate—third-party DLLs are checked in, but no SBOM, license audit, or secret scanning configuration is present.

A reliability heatmap summarises risk areas:

| Area | Status | Notes |
| --- | --- | --- |
| Build & Tooling | 🔴 High Risk | No .NET SDK installed in baseline environment; Linux CI targets `ubuntu-latest` despite Windows-only target framework, and workflows call non-existent scripts. |
| Testing & Coverage | 🟠 Medium Risk | Single xUnit/FsCheck test project with integration-heavy fixtures; no coverage reporting or gates; OR-Tools optional dependency undocumented. |
| Architecture & Code Health | 🟠 Medium Risk | Clear project boundaries but cyclic dependencies between IO, Geometry, and Planning modules; high-complexity geometry routines without decomposition. |
| Documentation & Onboarding | 🔴 High Risk | No README/quickstart; tooling scripts assume prior knowledge; no changelog or release notes. |
| Security & Compliance | 🟡 Low/Medium Risk | Bundled native DLLs and Rhino references without license documentation; no vulnerability scanning, SBOM, or secret scanning configuration. |

## Repository & Module Topology

* Solution: [`AssemblyChain-Core.sln`](../../AssemblyChain-Core.sln) enumerates nine library projects and one Grasshopper plugin, plus test and benchmark projects.
* Projects inherit Windows-only settings from [`Directory.Build.props`](../../Directory.Build.props), establishing shared package versions for RhinoCommon, Grasshopper, and Newtonsoft.Json.
* Module responsibilities:
  * `AssemblyChain.Core`: domain entities, spatial primitives, and workflow records (e.g., `Spatial/GeometryTypes.cs`).
  * `AssemblyChain.Geometry` & `.Geometry.Abstractions`: mesh/contact utilities layered above the core domain abstractions.
  * `AssemblyChain.Planning`: solver façade orchestrating CSP/MILP/SAT backends with Rhino geometry dependencies.
  * `AssemblyChain.Constraints`, `.Graphs`, `.Analysis`, `.IO`, `.Robotics`: specialised services and adapters consumed by the planning pipeline.
  * `AssemblyChain.Grasshopper`: UI components and native interoperability referencing BulletSharp/WaveEngine libraries.
* Benchmarks (`tests/AssemblyChain.Benchmarks`) depend on BenchmarkDotNet and expect Windows runners.

A detailed module map is provided in [`architecture.md`](architecture.md).

## Build & Continuous Integration

* Baseline environment lacks the .NET SDK—`dotnet --info` fails—and no documented installation steps succeed without manual intervention. The provided `tools/install.sh` script installs .NET 7 via the deprecated `dotnet-install.sh` helper but is not referenced in documentation or CI.
* Projects target `net7.0-windows` and rely on Windows-specific tooling (`UseWindowsForms`, RhinoCommon). GitHub Actions workflows (`.github/workflows/dotnet.yml`, `code-metrics.yml`, `nightly-benchmarks.yml`) run on `ubuntu-latest`, which cannot build Windows-targeted projects with Rhino dependencies.
* Workflows reference missing scripts (`repo_audit.py`, `scripts/generate_metrics_summary.py`, `scripts/check_quality.ps1`, `scripts/enforce_quality_gates.py`). These calls currently fail, preventing code-quality gates from executing.
* `dotnet-desktop.yml` is an unconfigured template (placeholder solution/test paths, signing secrets) and does not execute successfully.
* Local build tooling (`tools/build.sh`, `tools/test.sh`, `tools/pack.sh`) assumes a pre-installed .NET SDK and Windows Rhino environment but does not validate prerequisites.

### Recommended Remediation

1. Consolidate build instructions into a root README and ensure `tools/install.sh` (or a PowerShell equivalent) becomes the canonical bootstrap script.
2. Replace Linux runners with `windows-latest` in workflows that compile `net7.0-windows` projects, or split out cross-platform packages if Linux builds are required.
3. Remove or implement the missing Python/PowerShell scripts referenced by quality workflows; failing steps currently block automation.
4. Align CI/.editorconfig analyzers by enabling `TreatWarningsAsErrors=true` once the codebase is compliant, and publish build artifacts only after quality gates pass.

## Testing & Quality Gates

* Tests reside exclusively in `tests/AssemblyChain.Core.Tests`, using xUnit and FsCheck with project references to all runtime assemblies, including the Grasshopper plugin.
* No test coverage report is generated; while `coverlet.collector` is referenced, CI never collects or uploads coverage artifacts.
* Tests exercise solver pipelines but depend on optional OR-Tools binaries (`Google.OrTools` package reference is commented in `Directory.Build.props`), risking runtime failures when the OR-Tools backend is disabled.
* Benchmarks run nightly on Linux, which is incompatible with the Windows-only assemblies.
* Static analysis packages (NetAnalyzers, StyleCop) are listed in `Directory.Build.props` yet missing `stylecop.json` configuration and enforcement.

### Suggested Improvements

1. Introduce `dotnet test /p:CollectCoverage=true` with Coverlet output and publish thresholds per module (≥80% for `Core`, `Planning`, `Geometry`, ≥70% overall).
2. Split fast-running unit tests from integration tests; add deterministic unit tests for `GeometryTypes`, constraint builders, and serialization logic.
3. Add quality command orchestration (e.g., `dotnet format`, `dotnet build`, `dotnet test`, analyzers) via a PowerShell script invoked locally and in CI.
4. Document OR-Tools backend setup, and provide a mock backend for automated tests to avoid native solver dependencies.

Details and a proposed coverage plan live in [`tests.md`](tests.md).

## Dependencies & Runtime Concerns

* RhinoCommon/Grasshopper packages are pinned to version `8.0.23304.9001` via `Directory.Build.props`, requiring Rhino 8 SDK installation on build hosts.
* The Grasshopper project ships native DLLs (`BulletSharpPInvoke.dll`, `WaveEngine.*`, `libbulletc.dll`) stored in `src/AssemblyChain.Grasshopper/Libs`. Licensing information and update strategy are undocumented.
* No `Directory.Packages.props` or `nuget.config` is used for reproducible dependency management; NuGet package restore relies on the public feed only.
* Optional OR-Tools dependency is not managed—`AssemblyChain.Core` comments mention enabling the backend by adding a `Google.OrTools` reference, but no conditional compilation or instructions exist.
* No runtime configuration templates (appsettings, environment variable list) are included; Grasshopper components implicitly rely on Rhino user directories.

## Documentation & Onboarding

* There is no root README, changelog, or contributing guide. The only high-level documentation is `docs/reports/architecture_modularity_review.md`, which predates current repository state and references artifacts absent from the repo.
* Installation scripts and workflows contain comments in Chinese, but there is no English quickstart or troubleshooting section.
* Packaging (`tools/pack.sh`) copies build outputs into `dist/` for manual distribution, yet no release pipeline or GitHub Release configuration exists.

### Required Actions

1. Draft a comprehensive README covering prerequisites (Rhino 8 SDK, .NET 7 Windows), setup commands, testing, benchmarking, and packaging.
2. Add `CONTRIBUTING.md`, `CHANGELOG.md`, and release process documentation (including versioning policy and plugin distribution steps).
3. Publish API docs or XML documentation summaries for public APIs, aligning with existing analyzers.

## Security, Licensing, and Compliance

* MIT License is present but third-party dependencies (RhinoCommon, Grasshopper, BulletSharp, WaveEngine) require attribution and license summaries.
* No `.github` secret-scanning configuration or CodeQL/Dependabot is enabled; dependency updates and vulnerability alerts are manual.
* Bundled native DLLs may become outdated; no hash or supply-chain validation is documented.
* No SBOM generation (e.g., `syft`, `cyclonedx`) or vulnerability scanning (`trivy`, `dotnet list package --vulnerable`) has been incorporated.

See [`security.md`](security.md) for remediation details.

## Release & Distribution

* `tools/pack.sh` builds and copies plugin binaries but does not sign or version artifacts beyond the hard-coded `0.2` version in `AssemblyChain.Gh.csproj`.
* GitHub Actions do not publish releases or NuGet packages.
* No automated Grasshopper package deployment to the Rhino ecosystem or internal distribution is configured.

### Recommendations

1. Adopt semantic versioning for the plugin and core libraries; move version declarations to a shared props file.
2. Create a Windows packaging workflow that builds, runs tests, signs the `.gha`, bundles native DLLs, and uploads artifacts to a GitHub Release.
3. Document manual release procedures until automation is established.

## Next Steps

The prioritised action backlog is captured in [`ACTIONS.md`](ACTIONS.md), with individual implementation-ready issues under [`issues/`](issues/).

