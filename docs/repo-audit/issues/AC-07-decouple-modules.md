# AC-07: Break IO ⇄ Geometry ⇄ Planning dependency cycle

- **Priority:** P2
- **Labels:** `architecture`, `refactor`
- **Owner:** Architecture
- **Depends on:** AC-01

## Context

* Cyclic dependencies exist between IO contracts, Geometry contact detection, and Planning models, complicating isolated testing and future multi-platform support.
* Rhino types leak across module boundaries, forcing everything to build against RhinoCommon.

## Tasks

1. Map current dependency graph and identify specific classes causing cycles (e.g., shared static helpers).
2. Introduce DTOs/interfaces in IO layer that abstract Rhino types.
3. Refactor Geometry/Planning code to consume abstractions rather than concrete Rhino types.
4. Update tests to validate decoupled interfaces, adding mocks/fakes as needed.

## Acceptance Criteria

- Solution builds with IO, Geometry, Planning having acyclic project references.
- Unit tests can run without Rhino assemblies loaded.
- Architectural documentation updated to reflect new boundaries.

