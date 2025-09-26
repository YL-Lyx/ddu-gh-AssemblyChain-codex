# AC-09: Replace bundled native DLLs with managed supply chain

- **Priority:** P2
- **Labels:** `security`, `build`
- **Owner:** Security
- **Depends on:** AC-05

## Context

* Native binaries in `src/AssemblyChain.Grasshopper/Libs` are committed without update strategy or checksums.
* Manual copying occurs during build (`AssemblyChain.Gh.csproj` AfterBuild target), increasing drift risk.

## Tasks

1. Evaluate NuGet or package feeds for BulletSharp/WaveEngine equivalents; if unavailable, script deterministic downloads with hashing.
2. Add integrity verification (hash comparison) before packaging.
3. Document upgrade procedure and align with release workflow.

## Acceptance Criteria

- Build pipeline obtains native dependencies from reproducible sources with checksum validation.
- Repository no longer relies on ad-hoc committed binaries or, if retention is required, includes documented update policy.
- Security review sign-off obtained for supply chain approach.

