# Security & License Assessment

## Dependency Overview

| Component | Source | Version | Notes |
| --- | --- | --- | --- |
| RhinoCommon / Grasshopper | NuGet | 8.0.23304.9001 | Windows-only SDK; requires Rhino license on build hosts. |
| Newtonsoft.Json | NuGet | 13.0.3 | Shared across projects; no lock file or central package management. |
| System.Drawing.Common | NuGet | 7.0.0 | Used by Grasshopper plugin; Windows-only in .NET 7. |
| BulletSharpPInvoke.dll | Bundled binary | Unknown | Native physics wrapper shipped in `src/AssemblyChain.Grasshopper/Libs`. |
| WaveEngine.Common.dll / WaveEngine.Mathematics.dll / WaveEngine.Yaml.dll | Bundled binaries | Unknown | No license metadata included; provenance unclear. |
| libbulletc.dll | Bundled native | Unknown | No checksum/signature; must be tracked for CVEs. |

## Identified Risks

1. **Unverified native DLLs** are committed without license texts or integrity checks, creating supply-chain risk.
2. **Platform-specific dependencies** (RhinoCommon, System.Drawing) necessitate Windows hosts; Linux CI jobs will fail and mask real security issues.
3. **Missing secret scanning / dependency scanning** – no Dependabot, CodeQL, or `dotnet list package --vulnerable` automation configured.
4. **No SBOM** – compliance teams cannot audit shipped binaries; release packaging lacks manifest of third-party components.
5. **Potential license obligations** – RhinoCommon/Grasshopper and BulletSharp may impose redistribution requirements not documented in `LICENSE`.

## Recommended Remediations

| Item | Priority | Action |
| --- | --- | --- |
| S1 | P0 | Inventory bundled DLLs, record source URLs, versions, and licenses; add `THIRD_PARTY_NOTICES.md` and ensure redistribution rights. |
| S2 | P1 | Integrate Dependabot updates for NuGet and GitHub Actions; enable GitHub secret scanning and CodeQL. |
| S3 | P1 | Add `dotnet list package --vulnerable` to CI and fail on high/critical vulnerabilities; archive reports in artifacts. |
| S4 | P1 | Generate SBOM (CycloneDX or Syft) during CI and attach to releases. |
| S5 | P2 | Evaluate replacing bundled native libs with package-manager-managed equivalents or documented build scripts. |
| S6 | P2 | Document Rhino license requirements and isolate proprietary SDK usage to optional build paths when possible. |

## Immediate Next Steps

1. Assign ownership for license review of Rhino/BulletSharp/WaveEngine components.
2. Configure Windows CI job to execute vulnerability scans alongside build/test.
3. Publish a security contact process (e.g., `SECURITY.md`) and responsible disclosure policy.

