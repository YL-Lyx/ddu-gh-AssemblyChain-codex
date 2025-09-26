# AC-04: Introduce coverage & test segregation

- **Priority:** P1
- **Labels:** `testing`, `coverage`
- **Owner:** Quality Guild
- **Depends on:** AC-01

## Context

* Tests currently run only via `dotnet test` without coverage reporting.
* `coverlet.collector` is referenced but unused; CI does not publish coverage artifacts.
* Integration-heavy tests slow down runs and require optional OR-Tools backend.

## Tasks

1. Configure `dotnet test` to collect Coverlet coverage (Cobertura + LCOV) and upload artifacts.
2. Define coverage thresholds (module: ≥80% for `Core`, `Geometry`, `Planning`; overall ≥70%) and fail builds below thresholds.
3. Split fast unit tests from integration/solver tests using traits or separate projects.
4. Provide documentation for enabling/disabling OR-Tools-dependent suites.

## Acceptance Criteria

- CI publishes coverage reports and enforces thresholds.
- Developers can run fast unit tests locally without Rhino/OR-Tools dependencies.
- Regression in coverage causes CI failure with clear messaging.

