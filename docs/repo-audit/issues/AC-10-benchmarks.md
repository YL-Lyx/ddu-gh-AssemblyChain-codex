# AC-10: Establish benchmarking strategy

- **Priority:** P3
- **Labels:** `performance`, `automation`
- **Owner:** Performance
- **Depends on:** AC-01

## Context

* `nightly-benchmarks.yml` runs on `ubuntu-latest` and fails due to Windows-only targets.
* Benchmark artifacts are stored under `artifacts/benchmarks` but not retained or analysed.

## Tasks

1. Update nightly workflow to run on `windows-latest` and ensure prerequisites (Rhino SDK, OR-Tools) are available.
2. Configure BenchmarkDotNet to export reports (Markdown/JSON) and publish as artifacts.
3. Integrate trend reporting (e.g., push summary to GitHub Pages or dashboard) for performance regressions.

## Acceptance Criteria

- Nightly benchmarks execute successfully and produce retained artifacts.
- Performance regressions trigger alerts or dashboards for review.
- Benchmark documentation explains how to reproduce results locally.

