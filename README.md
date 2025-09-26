# AssemblyChain Codex

AssemblyChain Codex brings together the computational backend for assembly reasoning and the Grasshopper-based authoring experience. This repository houses the solution scaffold, documentation, and sample assets required to build disassembly plans and robot-ready programs.

## Project Layout

- `src/` – .NET solution projects (`AssemblyChain.Core`, Grasshopper plug-in, robotics adapters, tools).
- `tests/` – Unit and integration suites (C# and Python).
- `docs/` – DocFX site, architecture notes, and workflow guides.
- `samples/` – Runnable examples and JSON fixtures.
- `artifacts/benchmarks/` – Nightly benchmark outputs.
- `tools/` – Command-line helpers and CI utilities.

## Getting Started

1. Install the .NET 8 SDK and Rhino 7+ (for Grasshopper integration).
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build and test the solution:
   ```bash
   dotnet build
   dotnet test
   ```
4. Explore the samples:
   ```bash
   dotnet run --project samples/case-03/Case03.csproj
   ```

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Workflow Standard](6A_Workflow_Standard.md)
- [Core & Grasshopper Task Brief](docs/task-core-grasshopper-frontend.md)
- Additional guides under `docs/`

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for coding standards, branching strategy, and CI expectations.

## License

AssemblyChain Codex is released under the terms described in [LICENSE](LICENSE).

