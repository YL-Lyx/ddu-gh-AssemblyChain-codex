# Task: Develop Core & Grasshopper Frontend for AssemblyChain Codex

## 🎯 Goal

Implement a modular Core backend and a Grasshopper frontend (GH plug-in) to realize the digital workflow for generative design → geometric analysis → assembly/disassembly planning → robotic execution of reconfigurable interlocking structures.

## 📌 Scope

### Backend (Core in C#)

- **Domain Model (`AssemblyChain.Core`)**
  - Define `Part`, `Joint`, and `Assembly` as immutable records.
  - Add serialization (`assembly.json`, `plan.json` v1 schemas).
- **Geometry & Constraints (`AssemblyChain.Geometry` / `AssemblyChain.Constraints`)**
  - Contact detection for point/line/face.
  - Direction cones + half-space intersection per part.
- **Graphs (`AssemblyChain.Graphs`)**
  - Build adjacency graph.
  - Build NDBG / DBG from constraints.
- **Planning (`AssemblyChain.Planning`)**
  - Implement `TreeSearchSolver` as first solver.
  - Add path feasibility and stability checks.
- **Analysis (`AssemblyChain.Analysis`)**
  - Compute stability margin from CoM vs support polygon.
- **Robotics (`AssemblyChain.Robotics`)**
  - UR10 RTDE wrapper + URScript export.
  - Schunk EGH Modbus TCP wrapper.
  - Dummy robot executor for tests.

### Frontend (Grasshopper plug-in in C#)

- Implement custom GH data (`GH_Assembly`, `GH_Plan`, etc.).
- Add nodes grouped under tab **AssemblyChain**:
  1. **Data & IO**: Import/Export assembly, Make Part/Joint.
  2. **Constraints**: Contact Detector, Direction Cones, Constraint Set.
  3. **Graphs**: Build Adjacency, Build NDBG, NDBG→DBG.
  4. **Planning**: Plan (Tree Search), Plan (SAT/Sampling as stubs).
  5. **Validation**: Stability Check, Path Feasibility.
  6. **Simulation**: Animate Plan, Sequence Diagram.
  7. **Robotics**: URScript Export, UR10 Live (dry-run), Schunk EGH control.

## ✅ Deliverables

- New projects under `src/`: `AssemblyChain.Core`, `Geometry`, `Constraints`, `Graphs`, `Planning`, `Analysis`, `IO`, `Robotics`, `GH`.
- Sample JSON fixtures in `samples/`.
- Unit tests under `tests/` (`Core`, `Planning`, `Robotics`).
- Integration tests for GH pipeline.
- Updated solution file `AssemblyChain-Core.sln`.
- Documentation in `docs/ARCHITECTURE.md` + node usage.
- CI workflows for build, test, docs, and nightly benchmarks.

## 📅 Acceptance Criteria

- Able to import sample assembly → compute contacts → build NDBG → generate valid assembly/disassembly plan.
- Plans validated as stable (margin > 0) and collision-free.
- Grasshopper user can run full pipeline from geometry → plan → animation → URScript export without coding.
- Nightly benchmarks run automatically on `samples/*` and update `artifacts/benchmarks/`.
