# Prioritised Action List

Priority legend: **P0** – critical blocking work, **P1** – high priority, **P2** – medium priority, **P3** – improvements. Effort: **S** ≤ 1 day, **M** ≤ 3 days, **L** > 3 days.

| ID | Priority | Title | Effort | Owner | Summary |
| --- | --- | --- | --- | --- | --- |
| AC-01 | P0 | Stabilise Windows build & CI pipeline | M | Platform Team | Switch GitHub Actions jobs to `windows-latest`, install Rhino 8 SDK + .NET 7, and replace placeholder workflows/missing scripts so `dotnet build/test` succeed. |
| AC-02 | P0 | Publish developer onboarding & prerequisites | S | DX Team | Author root README covering environment setup (`tools/install.sh`), Rhino dependencies, test/benchmark commands, and troubleshooting. |
| AC-03 | P1 | Restore quality automation scripts | M | Platform Team | Implement or remove the `repo_audit.py`, `generate_metrics_summary.py`, and `check_quality.ps1` helpers referenced in workflows and `Directory.Build.props`. |
| AC-04 | P1 | Introduce coverage & test segregation | M | Quality Guild | Configure Coverlet in CI, define coverage thresholds, and separate fast unit tests from integration/solver tests. |
| AC-05 | P1 | Document third-party licenses & produce SBOM | M | Security | Inventory Rhino/BulletSharp/WaveEngine dependencies, publish license notices, and add SBOM + vulnerability scanning to CI. |
| AC-06 | P1 | Define release packaging workflow | M | Release Eng. | Automate `.gha` packaging on Windows, sign artifacts, and upload to GitHub Releases with semantic versioning. |
| AC-07 | P2 | Break IO ⇄ Geometry ⇄ Planning dependency cycle | L | Architecture | Introduce DTO/adapters to decouple Rhino types from IO/Planning layers, enabling isolated testing and cross-platform builds. |
| AC-08 | P2 | Consolidate documentation & code style policies | S | DX Team | Provide `CONTRIBUTING.md`, `CHANGELOG.md`, StyleCop configuration, and enforce analyzers with `TreatWarningsAsErrors=true`. |
| AC-09 | P2 | Replace bundled native DLLs with managed supply chain | M | Security | Document provenance of `Libs/` DLLs, add integrity checks, or fetch via package manager to reduce drift. |
| AC-10 | P3 | Establish benchmarking strategy | S | Performance | Rework nightly benchmarks to run on Windows, capture artifacts, and tie results to release cadence. |

Action breakdowns and acceptance criteria are provided per-issue in [`issues/`](issues/).

