# AC-05: Document third-party licenses & produce SBOM

- **Priority:** P1
- **Labels:** `security`, `compliance`
- **Owner:** Security
- **Depends on:** AC-01

## Context

* Bundled native DLLs (BulletSharp, WaveEngine, libbulletc) lack provenance or license notices.
* MIT license in root does not cover third-party obligations.
* No vulnerability scanning or SBOM generation exists in CI.

## Tasks

1. Identify the origin, version, and license for each bundled binary; add `THIRD_PARTY_NOTICES.md`.
2. Integrate SBOM generation (CycloneDX or Syft) into the build pipeline and publish as artifact.
3. Run `dotnet list package --vulnerable` and/or `trivy fs` in CI; fail builds on high/critical issues.
4. Document Rhino/Grasshopper licensing requirements and distribution constraints.

## Acceptance Criteria

- Repository contains an up-to-date third-party notice file covering all dependencies.
- CI publishes SBOM and vulnerability scan outputs for every build.
- Compliance review confirms redistribution obligations are satisfied.

