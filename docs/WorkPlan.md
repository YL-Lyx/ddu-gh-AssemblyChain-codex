# AssemblyChain Work Plan

## Architecture and Dependencies
- Adopt the `AssemblyChainFacade` as the single integration surface for solver, contact detection, dataset export and robotics.
- Consolidate Grasshopper components on top of the facade to avoid direct cross-namespace dependencies.

## Solver Backends
- Maintain the OR-Tools adapter stub as default and gate vendor-specific implementations behind the backend interface.
- Extend solver options to enable hybrid selection and telemetry capture through facade metadata.

## Robotic Execution
- Leverage `ProcessSchema` and the companion JSON schema to export robot-ready process files with consistent metadata.
- Bridge UR10 stack integration through structured samples that demonstrate file contracts.

## Engineering & CI
- Run Windows-based quality checks (`scripts/check_quality.ps1`) covering build, unit tests and smoke benchmarks.
- Capture benchmark and test logs as workflow artifacts to aid regression analysis.

## Data & Learning
- Provide dataset export utilities that produce canonical JSON snapshots for downstream learning workflows.
- Ship ONNX inference stubs to document expected runtime shape while avoiding heavyweight dependencies.

## Documentation & Samples
- Keep documentation coverage above 25% by adding XML docs and markdown guides for new modules.
- Update samples when export schemas or dataset payloads change.
