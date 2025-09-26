# AC-02: Publish developer onboarding & prerequisites

- **Priority:** P0
- **Labels:** `documentation`, `developer-experience`
- **Owner:** DX Team
- **Depends on:** AC-01

## Context

* Repository lacks a root README, contributing guide, or troubleshooting instructions.
* Scripts under `tools/` assume pre-installed dependencies and contain only brief inline comments.
* Rhino/Grasshopper prerequisites and OR-Tools optional backend configuration are undocumented.

## Tasks

1. Create `README.md` covering prerequisites (Windows 10/11, Rhino 8 SDK, .NET 7), setup steps (`tools/install.sh`), and verification commands.
2. Document how to run builds, tests, benchmarks, and packaging scripts.
3. Add troubleshooting section for common issues (missing Rhino SDK, OR-Tools backend disabled, native DLL load failures).
4. Provide `CONTRIBUTING.md` outlining branching, coding standards (StyleCop analyzers), and commit/PR workflow.

## Acceptance Criteria

- New contributors can clone the repo, install prerequisites, and run tests using documented steps.
- README includes sections for setup, build, test, quality checks, release, and troubleshooting.
- Documentation references scripts and workflows that actually exist and are kept up to date.

