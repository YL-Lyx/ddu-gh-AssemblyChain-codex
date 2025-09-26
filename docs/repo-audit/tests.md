# Test & Quality Report

## Current Test Inventory

| Suite | Framework | Scope | Notes |
| --- | --- | --- | --- |
| `tests/AssemblyChain.Core.Tests` | xUnit + FsCheck | Integration-heavy solver, workflow, and Grasshopper interop tests. | References all runtime projects including Grasshopper plugin; slow to load due to Rhino types. |
| `tests/AssemblyChain.Benchmarks` | BenchmarkDotNet | Performance micro-benchmarks for core domain operations. | Requires Windows runner and Rhino dependencies; not wired into CI. |

Additional manual testing is implied via Grasshopper UI but undocumented.

## Baseline Execution

Attempting to enumerate the installed .NET SDK fails in the clean environment:

```
$ dotnet --info
bash: command not found: dotnet
```

Because the SDK is absent, `dotnet build`/`dotnet test` cannot be executed without first running `tools/install.sh` (which itself depends on internet access and Windows prerequisites). No coverage artefacts or test reports exist in the repository.

## Coverage & Quality Gates

* `coverlet.collector` is referenced in the test project, but neither CI nor local scripts collect or enforce coverage thresholds.
* No mutation, property-based coverage metrics, or flaky test detection is configured.
* Static analysis analyzers are included via `Directory.Build.props` yet no workflow invokes `dotnet format` or `dotnet build -warnaserror`.

## Gaps & Risks

1. Tests rely on Rhino types and OR-Tools backend availability, making them brittle on headless agents.
2. Lack of fast unit tests for mathematical primitives (e.g., `Vector3d`, `BoundingBox`) and serializers leaves regressions undetected.
3. Benchmarks run nightly on Linux (`ubuntu-latest`) despite Windows-only target, so the workflow will fail before producing data.
4. No seed data or fixtures documented for integration workflows; test data creation is hidden within helper methods.

## Hardening Plan

| Milestone | Description | Owner | Target |
| --- | --- | --- | --- |
| T1 | Add Windows-based CI job that runs `dotnet test` with Coverlet, publishing Cobertura/LCOV artefacts and enforcing module thresholds (≥80% `Core`, `Geometry`, `Planning`; ≥70% overall). | Quality Guild | Sprint 1 |
| T2 | Introduce unit tests for geometry primitives, constraint builders, serialization round-trips, and Graph utilities (aim for ≥30 new unit tests). | Core Team | Sprint 1–2 |
| T3 | Provide mock solver backend or interface to decouple OR-Tools from fast unit tests; document enabling native backend separately. | Planning Team | Sprint 2 |
| T4 | Split integration tests (Grasshopper, OR-Tools) into dedicated category executed on a nightly Windows pipeline. | Platform Team | Sprint 2 |
| T5 | Enable StyleCop/NetAnalyzers enforcement and integrate `dotnet format` into quality workflow. | DX Team | Sprint 1 |

## Additional Recommendations

* Capture flaky test data by enabling test retries with logging (e.g., `vstest.console` retry) and storing failure artefacts.
* Adopt deterministic test data builders instead of inline anonymous types to improve readability and reuse.
* Integrate BenchmarkDotNet outputs into performance dashboards only after the Windows runner is stable.

