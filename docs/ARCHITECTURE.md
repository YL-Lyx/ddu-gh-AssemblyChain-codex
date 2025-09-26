# AssemblyChain Codex Architecture

This document summarises the modular architecture implemented to support the digital workflow from generative design through robotic execution. The solution is composed of small, focused .NET projects that can be consumed individually or orchestrated via the Grasshopper front-end.

## Project Overview

| Project | Responsibility |
| --- | --- |
| `AssemblyChain.Core` | Domain records (`Part`, `Joint`, `Assembly`, `AssemblyPlan`) and spatial primitives shared across the stack. |
| `AssemblyChain.IO` | JSON serializers for `assembly.json` and `plan.json` fixtures. |
| `AssemblyChain.Geometry` | Contact detection for point/line/face primitives. |
| `AssemblyChain.Constraints` | Direction cone and half-space builders derived from contacts. |
| `AssemblyChain.Graphs` | Construction of adjacency, NDBG and DBG graph representations. |
| `AssemblyChain.Analysis` | Stability margin computation using support polygons. |
| `AssemblyChain.Planning` | Tree search solver with path feasibility checks. |
| `AssemblyChain.Robotics` | URScript export, UR10 RTDE stub and Schunk EGH control helpers. |
| `AssemblyChain.GH` | Grasshopper front-end with custom data types and components. |

Unit tests live in `tests/AssemblyChain.Core.Tests` and cover serialization, planning, robotics shims and a Grasshopper integration flow. Sample fixtures used by tests and demos are stored under `samples/`.

## Workflow

1. **Data & IO** – Assemblies and plans are exchanged using the JSON serializers. Custom Grasshopper components (`ImportAssembly`, `ExportPlan`, `MakePart`, `MakeJoint`) expose the functionality to designers.
2. **Constraints** – The contact detector in `AssemblyChain.Geometry` discovers candidate contacts which are converted into direction cones and half-space constraints.
3. **Graphs** – The graph builders in `AssemblyChain.Graphs` materialise adjacency, NDBG and DBG structures. Grasshopper components provide one-click access to each representation.
4. **Planning** – `TreeSearchSolver` consumes the graph data to produce validated plans. Additional planning components act as placeholders for SAT and sampling strategies.
5. **Validation & Simulation** – Stability and path feasibility checks expose immediate feedback. Simulation components produce plan animations and sequence diagrams.
6. **Robotics** – The robotics module exports URScript, simulates UR10 connectivity, and records gripper commands. Grasshopper components wrap these capabilities for non-programmers.

Nightly benchmarks can execute via the reusable scripts and `tests/AssemblyChain.Benchmarks` project, producing reports under `artifacts/benchmarks/`.

## Grasshopper Components

All Grasshopper nodes live under the `AssemblyChain` tab and are organised into the following panels:

1. **Data & IO** – Import/Export assembly, Make Part, Make Joint.
2. **Constraints** – Contact Detector, Direction Cones, Constraint Set.
3. **Graphs** – Build Adjacency, Build NDBG, NDBG → DBG.
4. **Planning** – Plan (Tree Search), Plan (SAT), Plan (Sampling).
5. **Validation** – Stability Check, Path Feasibility.
6. **Simulation** – Animate Plan, Sequence Diagram.
7. **Robotics** – URScript Export, UR10 Live, Schunk EGH.

Each component is available in the stub build so automated tests can exercise the intended Grasshopper workflow without requiring Rhino.
