# AC-06: Define release packaging workflow

- **Priority:** P1
- **Labels:** `release`, `automation`
- **Owner:** Release Eng.
- **Depends on:** AC-01, AC-02

## Context

* `tools/pack.sh` manually copies `.gha` and DLLs to `dist/` without signing or versioning.
* No GitHub Release automation or artifact retention is defined.
* Plugin version (`0.2`) is hard-coded in `AssemblyChain.Gh.csproj`.

## Tasks

1. Move version metadata into shared props file and adopt semantic versioning.
2. Create Windows GitHub Action that builds, tests, signs, and packages the plugin, uploading `.gha` + dependencies to a release draft.
3. Include SBOM, license notices, and changelog excerpts in release artifacts.
4. Document manual fallback procedure for releases.

## Acceptance Criteria

- Release workflow can be triggered on tags and produces signed `.gha` with all dependencies.
- Version numbers are consistent across projects and release notes.
- Release documentation updated with step-by-step instructions.

