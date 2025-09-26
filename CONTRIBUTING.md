# Contributing to AssemblyChain Codex

Thank you for your interest in improving AssemblyChain! This document explains how to propose changes and keep the project healthy.

## Code of Conduct

All contributors are expected to uphold the standards in our community Code of Conduct (TBD). Be respectful and inclusive.

## Development Workflow

1. Fork the repository and create a feature branch.
2. Run `dotnet restore` to fetch dependencies.
3. Implement your changes following the module boundaries described in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
4. Add or update automated tests under `tests/`.
5. Format the code with `dotnet format` and ensure `dotnet test` passes locally.
6. Commit using conventional messages (e.g., `feat:`, `fix:`) and open a pull request referencing the relevant issue.

## Pull Request Checklist

- [ ] Tests added or updated.
- [ ] Documentation updated (README, docs pages, or XML comments as applicable).
- [ ] CI passes (build, tests, lint, docs if touched).
- [ ] Benchmarks updated when planner performance changes.

## Grasshopper Plug-in

Grasshopper components must call into the corresponding services from the Core libraries. Avoid duplicating logic inside `.gha` projects—share functionality through public interfaces in `src/AssemblyChain.*` projects.

## Samples and Fixtures

Keep runnable samples in `samples/`. Use descriptive folder names (kebab-case) and document execution steps in a local `README.md`. Regression fixtures should live under `tests/Fixtures/` and remain deterministic.

## Releasing

Release notes are managed via `CHANGELOG.md`. When preparing a release, ensure that DocFX output is up-to-date and that the nightly benchmarks have been reviewed for regressions.

