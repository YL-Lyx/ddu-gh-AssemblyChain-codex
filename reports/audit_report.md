# AssemblyChain Source Audit Report

## Repository Overview

* Analysed directory: `src`
* Total files analysed: 108
* Total LOC / SLOC: 15853 / 13794
* Average LOC per file: 146.8
* Average method complexity: 2.77
* Maximum method complexity: 22
* Average documentation density: 18.06%
* Async methods detected: 6
* Duplicate fragments detected: 50

### Directory structure

```
src
├── AssemblyChain.Core
│   ├── Contact
│   │   ├── Detection
│   │   │   ├── BroadPhase
│   │   │   │   ├── RTreeBroadPhase.cs
│   │   │   │   └── SweepAndPrune.cs
│   │   │   ├── NarrowPhase
│   │   │   │   ├── BrepContactDetector.cs
│   │   │   │   ├── MeshContactDetector.cs
│   │   │   │   ├── MeshContactDetector.Testing.cs
│   │   │   │   ├── MixedGeoContactDetector.cs
│   │   │   │   └── NarrowPhaseDetection.cs
│   │   │   ├── ContactDetection.cs
│   │   │   └── DetectionOptions.cs
│   │   ├── ContactGraphBuilder.cs
│   │   ├── ContactModel.cs
│   │   └── ContactUtils.cs
│   ├── Contracts
│   │   ├── AssemblyPlanRequest.cs
│   │   ├── AssemblyPlanResult.cs
│   │   ├── ContactZone.cs
│   │   ├── DatasetExportOptions.cs
│   │   ├── DatasetExportResult.cs
│   │   ├── IContactUtils.cs
│   │   ├── NarrowPhaseResult.cs
│   │   ├── OnnxInferenceRequest.cs
│   │   ├── OnnxInferenceResult.cs
│   │   └── ProcessExportOptions.cs
│   ├── Data
│   │   └── DatasetExporter.cs
│   ├── Domain
│   │   ├── Common
│   │   │   ├── Entity.cs
│   │   │   └── ValueObject.cs
│   │   ├── Entities
│   │   │   ├── Assembly.cs
│   │   │   ├── Joint.cs
│   │   │   └── Part.cs
│   │   ├── Interfaces
│   │   │   ├── IAssemblyService.cs
│   │   │   └── IPartRepository.cs
│   │   ├── Services
│   │   │   └── DomainServices.cs
│   │   └── ValueObjects
│   │       ├── MaterialProperties.cs
│   │       ├── PartGeometry.cs
│   │       └── PhysicsProperties.cs
│   ├── Facade
│   │   └── AssemblyChainFacade.cs
│   ├── Graph
│   │   ├── ConstraintGraphBuilder.cs
│   │   ├── GNNAnalyzer.cs
│   │   ├── GraphOptions.cs
│   │   └── GraphViews.cs
│   ├── Learning
│   │   └── OnnxInferenceService.cs
│   ├── Model
│   │   ├── AssemblyModel.cs
│   │   ├── AssemblyModelFactory.cs
│   │   ├── ConstraintModel.cs
│   │   ├── ConstraintModelFactory.cs
│   │   ├── GraphModel.cs
│   │   ├── MotionModel.cs
│   │   └── SolverModel.cs
│   ├── Motion
│   │   ├── ConeIntersection.cs
│   │   ├── MotionEvaluator.cs
│   │   ├── MotionOptions.cs
│   │   ├── PoseEstimator.cs
│   │   └── PoseTypes.cs
│   ├── Robotics
│   │   ├── process.schema.json
│   │   └── ProcessSchema.cs
│   ├── Solver
│   │   ├── Backends
│   │   │   ├── ISolverBackend.cs
│   │   │   └── OrToolsBackend.cs
│   │   ├── BaseSolver.cs
│   │   ├── CSPSolver.cs
│   │   ├── MILPSolver.cs
│   │   ├── SATSolver.cs
│   │   └── SolverOptions.cs
│   ├── Toolkit
│   │   ├── BBox
│   │   │   └── BoundingHelpers.cs
│   │   ├── Brep
│   │   │   ├── BrepUtilities.cs
│   │   │   ├── PlanarOps.cs
│   │   │   └── PlanarOps.Types.cs
│   │   ├── Geometry
│   │   │   ├── MeshGeometry.cs
│   │   │   └── PlaneOperations.cs
│   │   ├── Intersection
│   │   │   ├── BrepBrepIntersect.cs
│   │   │   └── MeshMeshIntersect.cs
│   │   ├── Math
│   │   │   ├── Clustering.cs
│   │   │   ├── ConvexCone.cs
│   │   │   └── LinearAlgebra.cs
│   │   ├── Mesh
│   │   │   ├── Preprocessing
│   │   │   │   ├── MeshOptimizer.cs
│   │   │   │   ├── MeshRepair.cs
│   │   │   │   └── MeshValidator.cs
│   │   │   ├── MeshPreprocessor.cs
│   │   │   ├── MeshPreprocessor.Types.cs
│   │   │   ├── MeshSpatialIndex.cs
│   │   │   └── MeshToBrep.cs
│   │   └── Utils
│   │       ├── CacheManager.cs
│   │       ├── ContactDetectionHelpers.cs
│   │       ├── ExtremeRayExtractor.cs
│   │       ├── GroupCandidates.cs
│   │       ├── HalfspaceCone.cs
│   │       ├── Hashing.cs
│   │       ├── JsonSerializer.cs
│   │       ├── ParallelProcessor.cs
│   │       ├── PerformanceMonitor.cs
│   │       └── Tolerance.cs
│   └── AssemblyChain.Core.csproj
└── AssemblyChain.Grasshopper
    ├── Components
    │   ├── 1_Property
    │   │   └── AcGhDefinePhysicalProperty.cs
    │   ├── 2_Part
    │   │   ├── AcGhCreateAssembly.cs
    │   │   └── AcGhCreatePart.cs
    │   ├── 3_Solver
    │   │   ├── AcGhBuildContactModel.cs
    │   │   └── AcGhContactZones.cs
    │   └── 4_Simulation
    │       └── Physics
    │           ├── BulletPhysics.cs.back
    │           └── CreateGround.cs.back
    ├── Kernel
    │   ├── Goo
    │   │   ├── AcGhAssemblyWrapGoo.cs
    │   │   ├── AcGhContactModelGoo.cs
    │   │   ├── AcGhGooBase.cs
    │   │   ├── AcGhPartWrapGoo.cs
    │   │   └── AcGhPhysicalPropertyGoo.cs
    │   └── Params
    │       ├── AcGhAssemblyWrapParam.cs
    │       ├── AcGhContactModelParam.cs
    │       ├── AcGhParamBase.cs
    │       ├── AcGhPartWrapParam.cs
    │       └── AcGhPhysicalPropertyParam.cs
    ├── Libs
    │   ├── BulletSharpPInvoke.dll
    │   ├── libbulletc.dll
    │   ├── WaveEngine.Common.dll
    │   ├── WaveEngine.Mathematics.dll
    │   └── WaveEngine.Yaml.dll
    ├── Properties
    │   ├── AssemblyInfo.cs
    │   └── launchSettings.json
    ├── UI
    │   ├── Attributes
    │   │   └── ComponentButton.cs
    │   ├── Icons
    │   │   └── DDU LOGO_WhiteTEXT.png
    │   ├── ACDBGConduit.cs
    │   ├── ACPreviewConduit.cs
    │   └── ComponentForm.cs
    ├── AssemblyChain.Gh.csproj
    └── AssemblyChain.Gh.csproj.user
```

## Architecture & Dependencies

| Namespace | Fan-out | Fan-in | Dependencies |
| --- | --- | --- | --- |
| `AssemblyChain.Core.Contact` | 7 | 10 | `AssemblyChain.Core.Contact.Detection.BroadPhase`, `AssemblyChain.Core.Contact.Detection.NarrowPhase`, `AssemblyChain.Core.Contracts`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Graph`, `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Contact.Detection.BroadPhase` | 3 | 1 | `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Contact.Detection.NarrowPhase` | 8 | 1 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `AssemblyChain.Core.Toolkit`, `AssemblyChain.Core.Toolkit.Geometry`, `AssemblyChain.Core.Toolkit.Mesh`, `AssemblyChain.Core.Toolkit.Utils`, `Rhino.Geometry` |
| `AssemblyChain.Core.Contracts` | 4 | 5 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Model`, `AssemblyChain.Core.Solver`, `Rhino.Geometry` |
| `AssemblyChain.Core.Data` | 4 | 1 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Contracts`, `AssemblyChain.Core.Model`, `Newtonsoft.Json` |
| `AssemblyChain.Core.Domain.Common` | 0 | 2 | ∅ |
| `AssemblyChain.Core.Domain.Entities` | 3 | 11 | `AssemblyChain.Core.Domain.Common`, `AssemblyChain.Core.Domain.ValueObjects`, `Rhino.Geometry` |
| `AssemblyChain.Core.Domain.Interfaces` | 1 | 1 | `AssemblyChain.Core.Domain.Entities` |
| `AssemblyChain.Core.Domain.Services` | 4 | 0 | `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Domain.Interfaces`, `AssemblyChain.Core.Domain.ValueObjects`, `Rhino.Geometry` |
| `AssemblyChain.Core.Domain.ValueObjects` | 2 | 3 | `AssemblyChain.Core.Domain.Common`, `Rhino.Geometry` |
| `AssemblyChain.Core.Facade` | 8 | 1 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Contracts`, `AssemblyChain.Core.Data`, `AssemblyChain.Core.Learning`, `AssemblyChain.Core.Model`, `AssemblyChain.Core.Robotics`, `AssemblyChain.Core.Solver`, `AssemblyChain.Core.Solver.Backends` |
| `AssemblyChain.Core.Graph` | 3 | 1 | `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Learning` | 2 | 1 | `AssemblyChain.Core.Contracts`, `AssemblyChain.Core.Model` |
| `AssemblyChain.Core.Model` | 3 | 16 | `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Toolkit.Utils`, `Rhino.Geometry` |
| `AssemblyChain.Core.Motion` | 4 | 0 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Robotics` | 4 | 1 | `AssemblyChain.Core.Contracts`, `AssemblyChain.Core.Model`, `Newtonsoft.Json`, `Rhino.Geometry` |
| `AssemblyChain.Core.Solver` | 3 | 2 | `AssemblyChain.Core.Model`, `AssemblyChain.Core.Solver.Backends`, `Rhino.Geometry` |
| `AssemblyChain.Core.Solver.Backends` | 2 | 2 | `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.BBox` | 1 | 0 | `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Brep` | 4 | 0 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Geometry` | 2 | 1 | `AssemblyChain.Core.Contact`, `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Intersection` | 3 | 0 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Model`, `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Math` | 1 | 0 | `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Mesh` | 1 | 1 | `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Mesh.Preprocessing` | 1 | 0 | `Rhino.Geometry` |
| `AssemblyChain.Core.Toolkit.Utils` | 6 | 2 | `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Newtonsoft.Json`, `Newtonsoft.Json.Converters`, `Rhino.Geometry` |
| `AssemblyChain.Core.Utils` | 0 | 0 | ∅ |
| `AssemblyChain.Gh.Attributes` | 4 | 0 | `Grasshopper.GUI`, `Grasshopper.GUI.Canvas`, `Grasshopper.Kernel`, `Grasshopper.Kernel.Attributes` |
| `AssemblyChain.Gh.Kernel` | 10 | 0 | `AssemblyChain.Core`, `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Domain.ValueObjects`, `AssemblyChain.Core.Facade`, `AssemblyChain.Core.Model`, `Grasshopper.Kernel`, `Grasshopper.Kernel.Data`, `Grasshopper.Kernel.Types`, `Rhino.Geometry` |
| `AssemblyChain.Gh.UI` | 2 | 0 | `Eto.Drawing`, `Eto.Forms` |
| `AssemblyChain.Gh.Visualization` | 2 | 0 | `Rhino.Display`, `Rhino.Geometry` |

**Cycles detected:**
* `AssemblyChain.Core.Contact` → `AssemblyChain.Core.Contact.Detection.BroadPhase` → `AssemblyChain.Core.Model` → `AssemblyChain.Core.Toolkit.Utils` → `AssemblyChain.Core.Contact`
* `AssemblyChain.Core.Model` → `AssemblyChain.Core.Toolkit.Utils` → `AssemblyChain.Core.Model`
* `AssemblyChain.Core.Contact` → `AssemblyChain.Core.Contact.Detection.NarrowPhase` → `AssemblyChain.Core.Contact`
* `AssemblyChain.Core.Contact` → `AssemblyChain.Core.Contact.Detection.NarrowPhase` → `AssemblyChain.Core.Toolkit.Geometry` → `AssemblyChain.Core.Contact`
* `AssemblyChain.Core.Contact` → `AssemblyChain.Core.Contracts` → `AssemblyChain.Core.Contact`

## Hotspots

### Top complexity methods

| Method | Complexity | Length | File |
| --- | --- | --- | --- |
| `SolveInstance` | 22 | 163 | `AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs` |
| `DrawForeground` | 17 | 75 | `AssemblyChain.Grasshopper/UI/ACDBGConduit.cs` |
| `DetectCoplanarContacts` | 16 | 120 | `AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs` |
| `PreprocessMesh` | 16 | 117 | `AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs` |
| `DetectContactsWithIntersectionLines` | 15 | 125 | `AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs` |
| `DrawForeground` | 15 | 83 | `AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs` |
| `ValidateSettings` | 12 | 20 | `AssemblyChain.Core/Toolkit/Utils/Tolerance.cs` |
| `SolveInstance` | 12 | 92 | `AssemblyChain.Grasshopper/Components/2_Part/AcGhCreateAssembly.cs` |
| `ExtractClauses` | 11 | 50 | `AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs` |
| `DBSCAN` | 11 | 43 | `AssemblyChain.Core/Toolkit/Math/Clustering.cs` |

### Longest methods

| Method | Length | Complexity | File |
| --- | --- | --- | --- |
| `SolveInstance` | 163 | 22 | `AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs` |
| `DetectContactsWithIntersectionLines` | 125 | 15 | `AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs` |
| `DetectCoplanarContacts` | 120 | 16 | `AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs` |
| `PreprocessMesh` | 117 | 16 | `AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs` |
| `SolveInstance` | 92 | 12 | `AssemblyChain.Grasshopper/Components/2_Part/AcGhCreateAssembly.cs` |
| `SolveInstance` | 91 | 11 | `AssemblyChain.Grasshopper/Components/3_Solver/AcGhContactZones.cs` |
| `DetectMeshContactsEnhanced` | 86 | 7 | `AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs` |
| `DrawForeground` | 83 | 15 | `AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs` |
| `ComputeIntersection` | 81 | 9 | `AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs` |
| `ApplySnapshot` | 80 | 8 | `AssemblyChain.Grasshopper/UI/ACDBGConduit.cs` |

### Duplicate fragments

```csharp
Value = goo.Value;
return true;
default:
return base.CastFrom(source);
}
}
public override bool CastTo<T>(ref T target)
{
```
* `AssemblyChain.Grasshopper/Kernel/Goo/AcGhAssemblyWrapGoo.cs` @ line 47
* `AssemblyChain.Grasshopper/Kernel/Goo/AcGhContactModelGoo.cs` @ line 51
* `AssemblyChain.Grasshopper/Kernel/Goo/AcGhPartWrapGoo.cs` @ line 106
* `AssemblyChain.Grasshopper/Kernel/Goo/AcGhPhysicalPropertyGoo.cs` @ line 48

```csharp
{
try
{
var vertices = new Point3d[] {
mesh.Vertices[face.A],
mesh.Vertices[face.B],
mesh.Vertices[face.C]
};
```
* `AssemblyChain.Core/Toolkit/Geometry/MeshGeometry.cs` @ line 49
* `AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs` @ line 433

```csharp
{
bbox = partBbox;
initialized = true;
}
else
{
bbox.Union(partBbox);
}
```
* `AssemblyChain.Core/Domain/Entities/Assembly.cs` @ line 71
* `AssemblyChain.Core/Model/AssemblyModel.cs` @ line 63

```csharp
}
}
private void DrawArrow(DrawEventArgs e, Point3d position, Vector3d direction, double size, Color color)
{
var right = Vector3d.CrossProduct(direction, Vector3d.ZAxis);
if (right.IsTiny()) right = Vector3d.CrossProduct(direction, Vector3d.XAxis);
right.Unitize();
var up = Vector3d.CrossProduct(right, direction);
```
* `AssemblyChain.Grasshopper/UI/ACDBGConduit.cs` @ line 268
* `AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs` @ line 148

```csharp
{
if (constraintNormals == null || constraintNormals.Count == 0) return true;
foreach (var normal in constraintNormals)
{
var dot = Vector3d.Multiply(direction, normal);
if (dot < -tolerance) return false;
}
return true;
```
* `AssemblyChain.Core/Motion/ConeIntersection.cs` @ line 26
* `AssemblyChain.Core/Toolkit/Utils/HalfspaceCone.cs` @ line 26

## File Metrics

| File | LOC | SLOC | Methods | Avg Complexity | Max Complexity | Doc % | Await Count |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `AssemblyChain.Core/Contact/ContactGraphBuilder.cs` | 96 | 83 | 4 | 3.00 | 4 | 6.2% | 0 |
| `AssemblyChain.Core/Contact/ContactModel.cs` | 235 | 203 | 13 | 1.38 | 5 | 29.4% | 0 |
| `AssemblyChain.Core/Contact/ContactUtils.cs` | 69 | 62 | 2 | 3.50 | 4 | 5.8% | 0 |
| `AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs` | 202 | 170 | 7 | 2.43 | 7 | 15.3% | 0 |
| `AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs` | 274 | 241 | 7 | 2.71 | 7 | 19.0% | 0 |
| `AssemblyChain.Core/Contact/Detection/ContactDetection.cs` | 116 | 95 | 2 | 4.50 | 5 | 7.8% | 0 |
| `AssemblyChain.Core/Contact/Detection/DetectionOptions.cs` | 50 | 44 | 0 | 0.00 | 0 | 12.0% | 0 |
| `AssemblyChain.Core/Contact/Detection/NarrowPhase/BrepContactDetector.cs` | 58 | 49 | 1 | 4.00 | 4 | 10.3% | 0 |
| `AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.Testing.cs` | 88 | 76 | 3 | 1.33 | 2 | 17.0% | 0 |
| `AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs` | 679 | 568 | 13 | 4.85 | 15 | 5.9% | 0 |
| `AssemblyChain.Core/Contact/Detection/NarrowPhase/MixedGeoContactDetector.cs` | 117 | 102 | 2 | 5.00 | 6 | 7.7% | 0 |
| `AssemblyChain.Core/Contact/Detection/NarrowPhase/NarrowPhaseDetection.cs` | 78 | 66 | 3 | 2.00 | 4 | 7.7% | 0 |
| `AssemblyChain.Core/Contracts/AssemblyPlanRequest.cs` | 60 | 54 | 1 | 1.00 | 1 | 43.3% | 0 |
| `AssemblyChain.Core/Contracts/AssemblyPlanResult.cs` | 33 | 30 | 1 | 1.00 | 1 | 42.4% | 0 |
| `AssemblyChain.Core/Contracts/ContactZone.cs` | 53 | 47 | 2 | 1.00 | 1 | 43.4% | 0 |
| `AssemblyChain.Core/Contracts/DatasetExportOptions.cs` | 25 | 22 | 0 | 0.00 | 0 | 48.0% | 0 |
| `AssemblyChain.Core/Contracts/DatasetExportResult.cs` | 29 | 27 | 1 | 1.00 | 1 | 48.3% | 0 |
| `AssemblyChain.Core/Contracts/IContactUtils.cs` | 27 | 26 | 0 | 0.00 | 0 | 40.7% | 0 |
| `AssemblyChain.Core/Contracts/NarrowPhaseResult.cs` | 116 | 105 | 5 | 1.40 | 3 | 26.7% | 0 |
| `AssemblyChain.Core/Contracts/OnnxInferenceRequest.cs` | 32 | 29 | 1 | 1.00 | 1 | 43.8% | 0 |
| `AssemblyChain.Core/Contracts/OnnxInferenceResult.cs` | 24 | 22 | 1 | 1.00 | 1 | 41.7% | 0 |
| `AssemblyChain.Core/Contracts/ProcessExportOptions.cs` | 30 | 26 | 0 | 0.00 | 0 | 50.0% | 0 |
| `AssemblyChain.Core/Data/DatasetExporter.cs` | 109 | 89 | 1 | 6.00 | 6 | 10.1% | 0 |
| `AssemblyChain.Core/Domain/Common/Entity.cs` | 49 | 38 | 3 | 2.00 | 4 | 12.2% | 0 |
| `AssemblyChain.Core/Domain/Common/ValueObject.cs` | 56 | 42 | 2 | 2.00 | 3 | 5.4% | 0 |
| `AssemblyChain.Core/Domain/Entities/Assembly.cs` | 238 | 202 | 12 | 2.25 | 4 | 26.5% | 0 |
| `AssemblyChain.Core/Domain/Entities/Joint.cs` | 145 | 121 | 5 | 1.60 | 3 | 35.2% | 0 |
| `AssemblyChain.Core/Domain/Entities/Part.cs` | 174 | 148 | 6 | 1.17 | 2 | 36.2% | 0 |
| `AssemblyChain.Core/Domain/Interfaces/IAssemblyService.cs` | 81 | 71 | 0 | 0.00 | 0 | 40.7% | 0 |
| `AssemblyChain.Core/Domain/Interfaces/IPartRepository.cs` | 57 | 48 | 0 | 0.00 | 0 | 52.6% | 0 |
| `AssemblyChain.Core/Domain/Services/DomainServices.cs` | 303 | 256 | 16 | 2.00 | 5 | 10.9% | 3 |
| `AssemblyChain.Core/Domain/ValueObjects/MaterialProperties.cs` | 93 | 79 | 2 | 1.00 | 1 | 32.3% | 0 |
| `AssemblyChain.Core/Domain/ValueObjects/PartGeometry.cs` | 89 | 76 | 3 | 1.00 | 1 | 37.1% | 0 |
| `AssemblyChain.Core/Domain/ValueObjects/PhysicsProperties.cs` | 61 | 51 | 2 | 1.00 | 1 | 29.5% | 0 |
| `AssemblyChain.Core/Facade/AssemblyChainFacade.cs` | 172 | 153 | 10 | 1.60 | 3 | 22.1% | 0 |
| `AssemblyChain.Core/Graph/ConstraintGraphBuilder.cs` | 139 | 113 | 4 | 3.00 | 5 | 4.3% | 0 |
| `AssemblyChain.Core/Graph/GNNAnalyzer.cs` | 583 | 494 | 15 | 3.53 | 10 | 9.4% | 0 |
| `AssemblyChain.Core/Graph/GraphOptions.cs` | 14 | 12 | 0 | 0.00 | 0 | 21.4% | 0 |
| `AssemblyChain.Core/Graph/GraphViews.cs` | 76 | 62 | 10 | 1.00 | 1 | 0.0% | 0 |
| `AssemblyChain.Core/Learning/OnnxInferenceService.cs` | 44 | 39 | 1 | 3.00 | 3 | 20.5% | 0 |
| `AssemblyChain.Core/Model/AssemblyModel.cs` | 86 | 73 | 1 | 4.00 | 4 | 29.1% | 0 |
| `AssemblyChain.Core/Model/AssemblyModelFactory.cs` | 42 | 39 | 2 | 1.50 | 2 | 21.4% | 0 |
| `AssemblyChain.Core/Model/ConstraintModel.cs` | 72 | 62 | 6 | 1.50 | 3 | 5.6% | 0 |
| `AssemblyChain.Core/Model/ConstraintModelFactory.cs` | 50 | 45 | 1 | 2.00 | 2 | 16.0% | 0 |
| `AssemblyChain.Core/Model/GraphModel.cs` | 82 | 73 | 7 | 1.14 | 2 | 4.9% | 0 |
| `AssemblyChain.Core/Model/MotionModel.cs` | 86 | 72 | 8 | 1.75 | 4 | 4.7% | 0 |
| `AssemblyChain.Core/Model/SolverModel.cs` | 131 | 117 | 6 | 2.00 | 4 | 3.1% | 0 |
| `AssemblyChain.Core/Motion/ConeIntersection.cs` | 58 | 51 | 4 | 1.75 | 4 | 25.9% | 0 |
| `AssemblyChain.Core/Motion/MotionEvaluator.cs` | 123 | 101 | 6 | 3.33 | 7 | 4.9% | 0 |
| `AssemblyChain.Core/Motion/MotionOptions.cs` | 15 | 13 | 0 | 0.00 | 0 | 20.0% | 0 |
| `AssemblyChain.Core/Motion/PoseEstimator.cs` | 26 | 22 | 2 | 1.00 | 1 | 0.0% | 0 |
| `AssemblyChain.Core/Motion/PoseTypes.cs` | 16 | 14 | 1 | 1.00 | 1 | 0.0% | 0 |
| `AssemblyChain.Core/Robotics/ProcessSchema.cs` | 225 | 196 | 5 | 2.40 | 5 | 31.1% | 0 |
| `AssemblyChain.Core/Solver/Backends/ISolverBackend.cs` | 100 | 90 | 1 | 1.00 | 1 | 42.0% | 0 |
| `AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs` | 760 | 663 | 29 | 3.52 | 11 | 1.2% | 0 |
| `AssemblyChain.Core/Solver/BaseSolver.cs` | 140 | 126 | 5 | 2.00 | 5 | 12.9% | 0 |
| `AssemblyChain.Core/Solver/CSPSolver.cs` | 33 | 32 | 0 | 0.00 | 0 | 39.4% | 0 |
| `AssemblyChain.Core/Solver/MILPSolver.cs` | 34 | 32 | 0 | 0.00 | 0 | 38.2% | 0 |
| `AssemblyChain.Core/Solver/SATSolver.cs` | 34 | 32 | 0 | 0.00 | 0 | 38.2% | 0 |
| `AssemblyChain.Core/Solver/SolverOptions.cs` | 29 | 26 | 0 | 0.00 | 0 | 20.7% | 0 |
| `AssemblyChain.Core/Toolkit/BBox/BoundingHelpers.cs` | 287 | 254 | 13 | 3.00 | 7 | 16.7% | 0 |
| `AssemblyChain.Core/Toolkit/Brep/BrepUtilities.cs` | 302 | 259 | 9 | 4.56 | 9 | 12.9% | 0 |
| `AssemblyChain.Core/Toolkit/Brep/PlanarOps.Types.cs` | 73 | 63 | 0 | 0.00 | 0 | 53.4% | 0 |
| `AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs` | 472 | 393 | 13 | 5.08 | 16 | 6.4% | 0 |
| `AssemblyChain.Core/Toolkit/Geometry/MeshGeometry.cs` | 235 | 209 | 10 | 2.90 | 6 | 20.0% | 0 |
| `AssemblyChain.Core/Toolkit/Geometry/PlaneOperations.cs` | 341 | 293 | 10 | 2.80 | 7 | 22.0% | 0 |
| `AssemblyChain.Core/Toolkit/Intersection/BrepBrepIntersect.cs` | 308 | 268 | 8 | 4.12 | 9 | 10.7% | 0 |
| `AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs` | 366 | 317 | 7 | 4.71 | 9 | 9.0% | 0 |
| `AssemblyChain.Core/Toolkit/Math/Clustering.cs` | 317 | 291 | 12 | 4.83 | 11 | 0.9% | 0 |
| `AssemblyChain.Core/Toolkit/Math/ConvexCone.cs` | 194 | 170 | 22 | 1.82 | 4 | 4.6% | 0 |
| `AssemblyChain.Core/Toolkit/Math/LinearAlgebra.cs` | 173 | 158 | 10 | 2.10 | 5 | 21.4% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.Types.cs` | 89 | 65 | 2 | 1.00 | 1 | 16.9% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs` | 288 | 255 | 11 | 3.91 | 16 | 4.5% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/MeshSpatialIndex.cs` | 211 | 185 | 12 | 2.33 | 7 | 19.4% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/MeshToBrep.cs` | 223 | 194 | 6 | 3.33 | 5 | 12.6% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs` | 495 | 436 | 17 | 4.35 | 10 | 4.6% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs` | 369 | 329 | 12 | 4.17 | 11 | 3.3% | 0 |
| `AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs` | 341 | 304 | 10 | 4.30 | 9 | 5.3% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/CacheManager.cs` | 230 | 209 | 12 | 2.33 | 4 | 20.0% | 1 |
| `AssemblyChain.Core/Toolkit/Utils/ContactDetectionHelpers.cs` | 26 | 25 | 1 | 2.00 | 2 | 23.1% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/ExtremeRayExtractor.cs` | 81 | 72 | 2 | 7.50 | 8 | 7.4% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/GroupCandidates.cs` | 118 | 103 | 4 | 5.00 | 7 | 15.3% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/HalfspaceCone.cs` | 47 | 41 | 3 | 2.00 | 4 | 25.5% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/Hashing.cs` | 103 | 92 | 10 | 1.10 | 2 | 11.7% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/JsonSerializer.cs` | 278 | 248 | 18 | 2.56 | 8 | 1.1% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/ParallelProcessor.cs` | 196 | 176 | 2 | 3.00 | 5 | 12.2% | 3 |
| `AssemblyChain.Core/Toolkit/Utils/PerformanceMonitor.cs` | 224 | 199 | 15 | 1.47 | 5 | 29.5% | 0 |
| `AssemblyChain.Core/Toolkit/Utils/Tolerance.cs` | 171 | 150 | 16 | 1.94 | 12 | 3.5% | 0 |
| `AssemblyChain.Grasshopper/Components/1_Property/AcGhDefinePhysicalProperty.cs` | 88 | 76 | 3 | 3.00 | 7 | 3.4% | 0 |
| `AssemblyChain.Grasshopper/Components/2_Part/AcGhCreateAssembly.cs` | 126 | 112 | 4 | 3.75 | 12 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs` | 458 | 404 | 16 | 3.50 | 22 | 2.0% | 0 |
| `AssemblyChain.Grasshopper/Components/3_Solver/AcGhBuildContactModel.cs` | 105 | 87 | 3 | 2.00 | 4 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/Components/3_Solver/AcGhContactZones.cs` | 162 | 139 | 6 | 3.33 | 11 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Goo/AcGhAssemblyWrapGoo.cs` | 96 | 81 | 5 | 2.00 | 4 | 3.1% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Goo/AcGhContactModelGoo.cs` | 69 | 58 | 5 | 1.80 | 4 | 4.3% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Goo/AcGhGooBase.cs` | 55 | 48 | 4 | 1.00 | 1 | 41.8% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Goo/AcGhPartWrapGoo.cs` | 142 | 125 | 5 | 2.00 | 5 | 10.6% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Goo/AcGhPhysicalPropertyGoo.cs` | 66 | 56 | 5 | 1.80 | 4 | 4.5% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Params/AcGhAssemblyWrapParam.cs` | 13 | 11 | 0 | 0.00 | 0 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Params/AcGhContactModelParam.cs` | 19 | 18 | 0 | 0.00 | 0 | 36.8% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Params/AcGhParamBase.cs` | 49 | 45 | 3 | 1.00 | 1 | 38.8% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Params/AcGhPartWrapParam.cs` | 19 | 18 | 0 | 0.00 | 0 | 36.8% | 0 |
| `AssemblyChain.Grasshopper/Kernel/Params/AcGhPhysicalPropertyParam.cs` | 19 | 18 | 0 | 0.00 | 0 | 36.8% | 0 |
| `AssemblyChain.Grasshopper/Properties/AssemblyInfo.cs` | 38 | 33 | 0 | 0.00 | 0 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/UI/ACDBGConduit.cs` | 361 | 325 | 10 | 4.10 | 17 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs` | 167 | 149 | 3 | 6.00 | 15 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/UI/Attributes/ComponentButton.cs` | 81 | 72 | 6 | 1.83 | 5 | 0.0% | 0 |
| `AssemblyChain.Grasshopper/UI/ComponentForm.cs` | 46 | 41 | 3 | 1.00 | 1 | 0.0% | 0 |

## Issues & Risks

* **P0** – CyclicDependency: Cyclic namespace dependency detected: AssemblyChain.Core.Contact → AssemblyChain.Core.Contact.Detection.BroadPhase → AssemblyChain.Core.Model → AssemblyChain.Core.Toolkit.Utils → AssemblyChain.Core.Contact
* **P0** – CyclicDependency: Cyclic namespace dependency detected: AssemblyChain.Core.Model → AssemblyChain.Core.Toolkit.Utils → AssemblyChain.Core.Model
* **P0** – CyclicDependency: Cyclic namespace dependency detected: AssemblyChain.Core.Contact → AssemblyChain.Core.Contact.Detection.NarrowPhase → AssemblyChain.Core.Contact
* **P0** – CyclicDependency: Cyclic namespace dependency detected: AssemblyChain.Core.Contact → AssemblyChain.Core.Contact.Detection.NarrowPhase → AssemblyChain.Core.Toolkit.Geometry → AssemblyChain.Core.Contact
* **P0** – CyclicDependency: Cyclic namespace dependency detected: AssemblyChain.Core.Contact → AssemblyChain.Core.Contracts → AssemblyChain.Core.Contact
* **P0** – ExtremeComplexity: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs:SolveInstance complexity 22.
* **P1** – SharedHotspot: `AssemblyChain.Core.Contact` is a dependency hotspot with fan-in 10.
* **P1** – SharedHotspot: `AssemblyChain.Core.Domain.Entities` is a dependency hotspot with fan-in 11.
* **P1** – SharedHotspot: `AssemblyChain.Core.Model` is a dependency hotspot with fan-in 16.
* **P1** – HighCoupling: `AssemblyChain.Gh.Kernel` has fan-out 10; consider splitting responsibilities.
* **P1** – LargeFile: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs exceeds 600 LOC (679 LOC).
* **P1** – HighComplexity: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectContactsWithIntersectionLines complexity 15.
* **P1** – LongMethod: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectContactsWithIntersectionLines spans 125 lines.
* **P1** – LargeFile: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs exceeds 600 LOC (760 LOC).
* **P1** – HighComplexity: AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs:DetectCoplanarContacts complexity 16.
* **P1** – LongMethod: AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs:DetectCoplanarContacts spans 120 lines.
* **P1** – HighComplexity: AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs:PreprocessMesh complexity 16.
* **P1** – HighComplexity: AssemblyChain.Core/Toolkit/Utils/Tolerance.cs:ValidateSettings complexity 12.
* **P1** – HighComplexity: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreateAssembly.cs:SolveInstance complexity 12.
* **P1** – LongMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs:SolveInstance spans 163 lines.
* **P1** – HighComplexity: AssemblyChain.Grasshopper/UI/ACDBGConduit.cs:DrawForeground complexity 17.
* **P1** – HighComplexity: AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs:DrawForeground complexity 15.
* **P1** – Duplication: Detected 50 duplicated fragments across modules.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Contact/ContactModel.cs:ContactModel lacks XML documentation.
* **P2** – LongParameterList: AssemblyChain.Core/Contact/ContactModel.cs:ContactData defines 8 parameters.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:CreatePreset lacks XML documentation.
* **P2** – LongMethod: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectMeshContactsEnhanced spans 86 lines.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:ValidateAndFilterContacts lacks XML documentation.
* **P2** – LongParameterList: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectContactsWithTightInclusion defines 6 parameters.
* **P2** – LongParameterList: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectContactsWithIntersectionLines defines 6 parameters.
* **P2** – LongParameterList: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectContactsWithSimpleOverlap defines 6 parameters.
* **P2** – LongParameterList: AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:ComputeEdgeContacts defines 6 parameters.
* **P2** – LongParameterList: AssemblyChain.Core/Contact/Detection/NarrowPhase/NarrowPhaseDetection.cs:DetectContactsForPair defines 6 parameters.
* **P2** – LongParameterList: AssemblyChain.Core/Domain/ValueObjects/MaterialProperties.cs:MaterialProperties defines 9 parameters.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Graph/ConstraintGraphBuilder.cs:BuildPartConstraints lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Graph/ConstraintGraphBuilder.cs:BuildGroupConstraints lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Model/AssemblyModel.cs:AssemblyModel lacks XML documentation.
* **P2** – LongParameterList: AssemblyChain.Core/Model/GraphModel.cs:GraphModel defines 6 parameters.
* **P2** – LongParameterList: AssemblyChain.Core/Model/SolverModel.cs:DgSolverModel defines 10 parameters.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Model/SolverModel.cs:ToAssemblySequence lacks XML documentation.
* **P2** – LongParameterList: AssemblyChain.Core/Solver/Backends/ISolverBackend.cs:SolverBackendResult defines 6 parameters.
* **P2** – LowDocumentation: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs has sparse XML documentation (1.2%).
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:SolveSat lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:TrySelectMotionVector lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:ApplyDeclarativeConstraints lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:ExtractClauses lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:SolveBooleanAssignment lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:SatisfiesClauses lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/Backends/OrToolsBackend.cs:ResolveOrder lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Solver/BaseSolver.cs:MapToModel lacks XML documentation.
* **P2** – LongParameterList: AssemblyChain.Core/Toolkit/Geometry/PlaneOperations.cs:ComputeFaceIntersectionGeometry defines 6 parameters.
* **P2** – LongMethod: AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs:ComputeIntersection spans 81 lines.
* **P2** – LowDocumentation: AssemblyChain.Core/Toolkit/Math/Clustering.cs has sparse XML documentation (0.9%).
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Math/Clustering.cs:KMeans lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Math/Clustering.cs:InitializeKMeansPlusPlus lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Math/Clustering.cs:DBSCAN lacks XML documentation.
* **P2** – LowDocumentation: AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs has sparse XML documentation (4.5%).
* **P2** – LongMethod: AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs:PreprocessMesh spans 117 lines.
* **P2** – LowDocumentation: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs has sparse XML documentation (4.6%).
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs:ReduceVertices lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs:SmoothMesh lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs:CalculateFaceArea lacks XML documentation.
* **P2** – LowDocumentation: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs has sparse XML documentation (3.3%).
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs:FillMeshHoles lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs:RemoveDuplicateFaces lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs:GroupNakedEdgesIntoLoops lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs:TryFillHole lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs:CheckDegenerateFaces lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs:CheckNormals lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs:CheckBoundingBox lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs:IsDegenerateFace lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Utils/ExtremeRayExtractor.cs:SampleRaysEvenly lacks XML documentation.
* **P2** – LowDocumentation: AssemblyChain.Core/Toolkit/Utils/JsonSerializer.cs has sparse XML documentation (1.1%).
* **P2** – UndocumentedMethod: AssemblyChain.Core/Toolkit/Utils/JsonSerializer.cs:Serialize lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/1_Property/AcGhDefinePhysicalProperty.cs:SolveInstance lacks XML documentation.
* **P2** – LongMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreateAssembly.cs:SolveInstance spans 92 lines.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreateAssembly.cs:SolveInstance lacks XML documentation.
* **P2** – LowDocumentation: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs has sparse XML documentation (2.0%).
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs:SolveInstance lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs:VariableParameterMaintenance lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs:UpdateInputParameters lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/2_Part/AcGhCreatePart.cs:AppendAdditionalComponentMenuItems lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/3_Solver/AcGhBuildContactModel.cs:SolveInstance lacks XML documentation.
* **P2** – LongMethod: AssemblyChain.Grasshopper/Components/3_Solver/AcGhContactZones.cs:SolveInstance spans 91 lines.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/Components/3_Solver/AcGhContactZones.cs:SolveInstance lacks XML documentation.
* **P2** – LowDocumentation: AssemblyChain.Grasshopper/UI/ACDBGConduit.cs has sparse XML documentation (0.0%).
* **P2** – LongMethod: AssemblyChain.Grasshopper/UI/ACDBGConduit.cs:ApplySnapshot spans 80 lines.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/UI/ACDBGConduit.cs:ApplySnapshot lacks XML documentation.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/UI/ACDBGConduit.cs:DrawForeground lacks XML documentation.
* **P2** – LongMethod: AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs:DrawForeground spans 83 lines.
* **P2** – UndocumentedMethod: AssemblyChain.Grasshopper/UI/ACPreviewConduit.cs:DrawForeground lacks XML documentation.

## Recommendations

### Architecture

* Break cyclic namespace dependencies via inversion (interfaces) or mediator services and enforce one-way references.
* Introduce façade services around shared hotspots to reduce direct dependencies and protect domain boundaries.
* Review high fan-out namespaces and split responsibilities into cohesive modules with explicit APIs.

### Module

* Decompose oversized files into focused classes following single-responsibility principles.
* Factor repeated fragments into shared utilities or generics to eliminate duplication across modules.

### Function

* Introduce parameter objects or configuration records to shrink long parameter lists.
* Refactor high-complexity methods using guard clauses, extraction, and descriptive helpers to flatten nesting.
* Split long methods around distinct responsibilities and favour pipelines or smaller private helpers.

### Engineering

* Add regression tests around identified hotspots before refactoring to protect behaviour.
* Automate this audit via CI to monitor metric drift and enforce agreed quality gates.
* Raise documentation coverage by requiring XML summaries for public APIs and critical workflows.

## Roadmap

1. **Phase 1 – Baseline**: Run automated audit in CI, validate metrics with the architecture team, and agree on target gates.
2. **Phase 2 – Stabilise**: Address P0/P1 issues (cycles, extreme complexity, duplication) while adding guard tests around hotspots.
3. **Phase 3 – Optimise**: Refine module boundaries, pursue performance improvements (profiling-driven) and enforce documentation standards.
