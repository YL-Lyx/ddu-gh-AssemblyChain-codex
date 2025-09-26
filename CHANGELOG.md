# Changelog

## [Unreleased]
### Added
- AssemblyChain facade consolidating solver, dataset, inference and robotics pipelines.
- Robotic process schema (`ProcessSchema`) and JSON schema export for UR10 integration.
- Dataset export utilities, ONNX inference stub, and benchmark suite.
- Windows quality workflow with build, test and smoke benchmark stages.

### Changed
- Grasshopper Goo and Param classes refactored to share base implementations.
- Contact detection, planar operations and mesh preprocessing split into partial classes for maintainability.

### Documentation
- Added work plan outlining future architecture, CI and learning goals.
- Included samples for process export and dataset payloads.
