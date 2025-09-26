# AssemblyChain Architecture

## Overview
AssemblyChain is organized as a collection of focused libraries that together cover the entire workflow from raw geometry to executable robot programs. The solution is designed so that computational logic lives in reusable .NET class libraries while user experiences (Grasshopper, CLI tooling) delegate to those services. The diagram below summarises the flow of data between the core modules:

```
Geometry → Constraints → Graphs → Planning → Analysis
      ↘                                ↙
        IO ↔ Robotics ↔ Tools ↔ Grasshopper
```

Each box represents a .NET project rooted under `src/AssemblyChain.*`. Cross-module interactions happen through narrow interfaces to keep the system testable and swappable.

## Modules

### AssemblyChain.Core
Defines immutable domain types (`PartId`, `Part`, `Joint`, `Assembly`) and contracts such as `IAssemblyValidator` and `ISerialization`. The Core project is intentionally free of geometry or platform dependencies.

### AssemblyChain.Geometry
Offers shape access via `IGeometryRef` abstractions and `IGeometryOps` services. Default adapters include RhinoCommon for Breps/Meshes, NetTopologySuite for 2D reasoning, and triangle mesh utilities for physics-style approximations. Core routines provide contact detection and swept-volume collision queries.

### AssemblyChain.Constraints
Consumes assemblies and geometry operations to derive contact manifolds, friction cones, and aggregated half-space representations. The `IConstraintGenerator` service converts low-level contacts into reusable `ConstraintSet` objects that other modules consume.

### AssemblyChain.Graphs
Implements graph builders for adjacency graphs and blocking graphs (both non-directional and directional). Graphs are derived from constraint sets and expose traversal helpers that downstream planners use.

### AssemblyChain.Planning
Provides `Plan` and `PlanStep` primitives together with `ISequenceSolver` implementations. The initial solver is a tree-search variant; future extensions include SAT- and sampling-based solvers plus RL hooks. The module delegates kinematic feasibility checks to `IPathFeasibility` so that motion reasoning stays isolated.

### AssemblyChain.Analysis
Evaluates mechanical stability throughout the plan. The `IStabilityAnalyzer` reports boolean stability as well as numeric margins so planners can prune unsafe branches.

### AssemblyChain.IO
Owns versioned JSON schemas (`assemblychain.schema.json`, `plan.schema.json`) and serialization bridges, including Rhino/GH adapters. It is the single entry point for persistence and interchange.

### AssemblyChain.Robotics
Defines `IRobot`, `IGripper`, and `IExecutor` abstractions. Adapters for UR10 via RTDE, Schunk EGH via Modbus, and URScript exporters live here. A `FakeRobot` implementation powers tests and dry runs.

### AssemblyChain.GH
Contains the Grasshopper plug-in (`.gha`) with thin components that forward to the core services. Custom goo types (`GH_Assembly`, `GH_Part`, etc.) wrap core domain objects to make them first-class in the Grasshopper canvas.

### AssemblyChain.Tools
Hosts command-line utilities such as benchmark runners and data validators. Tools reference the same service interfaces as the Grasshopper plug-in so behaviour stays consistent across entry points.

## Samples and Fixtures
Sample assemblies reside under `samples/`, including the migrated `samples/case-03` project. Test fixtures live under `tests/Fixtures/` alongside targeted unit and integration suites. Nightly benchmarks write deterministic metrics to `artifacts/benchmarks/` for regression tracking.

## Continuous Integration
GitHub Actions workflows perform formatting, build/test, DocFX publication, and nightly benchmarking. Coverage artefacts and benchmark reports are uploaded to aid review. Future additions will link the Grasshopper plug-in build to release pipelines.

## Documentation Topology
This document complements `docs/index.md` and `6A_Workflow_Standard.md`. API reference content is generated via DocFX under `docs/api/`, while process exports and dataset tooling are documented in their respective guides.

