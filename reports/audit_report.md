# AssemblyChain Core Audit Report

## Repository Overview

* Analysed directory: `src/AssemblyChain.Core`
* Total files analysed: 66
* Total LOC: 11623
* Average LOC per file: 176.1
* Average method complexity: 2.19
* Maximum method complexity: 18
* Average documentation density: 14.54%
* Duplicate fragments detected: 138

### Directory structure

```
AssemblyChain.Core
├── Contact
│   ├── Detection
│   │   ├── BroadPhase
│   │   │   ├── RTreeBroadPhase.cs
│   │   │   └── SweepAndPrune.cs
│   │   ├── NarrowPhase
│   │   │   ├── BrepContactDetector.cs
│   │   │   ├── MeshContactDetector.cs
│   │   │   ├── MixedGeoContactDetector.cs
│   │   │   └── NarrowPhaseDetection.cs
│   │   ├── ContactDetection.cs
│   │   └── DetectionOptions.cs
│   ├── ContactGraphBuilder.cs
│   └── ContactModel.cs
├── Domain
│   ├── Common
│   │   ├── Entity.cs
│   │   └── ValueObject.cs
│   ├── Entities
│   │   ├── Assembly.cs
│   │   ├── Joint.cs
│   │   └── Part.cs
│   ├── Interfaces
│   │   ├── IAssemblyService.cs
│   │   └── IPartRepository.cs
│   ├── Services
│   │   └── DomainServices.cs
│   └── ValueObjects
│       ├── MaterialProperties.cs
│       ├── PartGeometry.cs
│       └── PhysicsProperties.cs
├── Graph
│   ├── ConstraintGraphBuilder.cs
│   ├── GNNAnalyzer.cs
│   ├── GraphOptions.cs
│   └── GraphViews.cs
├── Model
│   ├── AssemblyModel.cs
│   ├── AssemblyModelFactory.cs
│   ├── ConstraintModel.cs
│   ├── GraphModel.cs
│   ├── MotionModel.cs
│   └── SolverModel.cs
├── Motion
│   ├── ConeIntersection.cs
│   ├── MotionEvaluator.cs
│   ├── MotionOptions.cs
│   ├── PoseEstimator.cs
│   └── PoseTypes.cs
├── Solver
│   ├── CSPSolver.cs
│   ├── MILPSolver.cs
│   ├── SATSolver.cs
│   └── SolverOptions.cs
├── Toolkit
│   ├── BBox
│   │   └── BoundingHelpers.cs
│   ├── Brep
│   │   ├── BrepUtilities.cs
│   │   └── PlanarOps.cs
│   ├── Geometry
│   │   ├── MeshGeometry.cs
│   │   └── PlaneOperations.cs
│   ├── Intersection
│   │   ├── BrepBrepIntersect.cs
│   │   └── MeshMeshIntersect.cs
│   ├── Math
│   │   ├── Clustering.cs
│   │   ├── ConvexCone.cs
│   │   └── LinearAlgebra.cs
│   ├── Mesh
│   │   ├── Preprocessing
│   │   │   ├── MeshOptimizer.cs
│   │   │   ├── MeshRepair.cs
│   │   │   └── MeshValidator.cs
│   │   ├── MeshPreprocessor.cs
│   │   ├── MeshSpatialIndex.cs
│   │   └── MeshToBrep.cs
│   └── Utils
│       ├── CacheManager.cs
│       ├── ContactDetectionHelpers.cs
│       ├── ExtremeRayExtractor.cs
│       ├── GroupCandidates.cs
│       ├── HalfspaceCone.cs
│       ├── Hashing.cs
│       ├── JsonSerializer.cs
│       ├── ParallelProcessor.cs
│       ├── PerformanceMonitor.cs
│       └── Tolerance.cs
└── AssemblyChain.Core.csproj
```

## Namespace Dependencies

* `AssemblyChain.Core.Contact` → `AssemblyChain.Core.Contact.Detection.BroadPhase`, `AssemblyChain.Core.Contact.Detection.NarrowPhase`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Graph`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Contact.Detection.BroadPhase` → `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Contact.Detection.NarrowPhase` → `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `AssemblyChain.Core.Toolkit`, `AssemblyChain.Core.Toolkit.Geometry`, `AssemblyChain.Core.Toolkit.Mesh`, `AssemblyChain.Core.Toolkit.Utils`, `Rhino.Geometry`
* `AssemblyChain.Core.Domain.Entities` → `AssemblyChain.Core.Domain.Common`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Domain.ValueObjects`, `Rhino.Geometry`
* `AssemblyChain.Core.Domain.Interfaces` → `AssemblyChain.Core.Domain.Entities`
* `AssemblyChain.Core.Domain.Services` → `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Domain.Interfaces`, `AssemblyChain.Core.Domain.ValueObjects`, `Rhino.Geometry`
* `AssemblyChain.Core.Domain.ValueObjects` → `AssemblyChain.Core.Domain.Common`, `Rhino.Geometry`
* `AssemblyChain.Core.Graph` → `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Model` → `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Toolkit.Utils`, `Rhino.Geometry`
* `AssemblyChain.Core.Motion` → `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Solver` → `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.BBox` → `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Brep` → `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Geometry` → `AssemblyChain.Core.Contact`, `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Intersection` → `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Model`, `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Math` → `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Mesh` → `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Mesh.Preprocessing` → `Rhino.Geometry`
* `AssemblyChain.Core.Toolkit.Utils` → `AssemblyChain.Core.Contact`, `AssemblyChain.Core.Domain.Entities`, `AssemblyChain.Core.Model`, `Newtonsoft.Json`, `Newtonsoft.Json.Converters`, `Rhino.Geometry`

## File Metrics

### `src/AssemblyChain.Core/Contact/ContactGraphBuilder.cs`

* LOC / SLOC: 96 / 83
* Classes: ContactGraphBuilder
* Methods: 4
* Total complexity: 12
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| BuildGraph | 2 | 4 | ✅ | 19-48 |
| TryParsePartIndex | 2 | 2 | ⚠️ | 54-61 |
| CalculateInDegrees | 1 | 4 | ⚠️ | 64-75 |
| FindStronglyConnectedComponents | 1 | 2 | ⚠️ | 78-90 |

### `src/AssemblyChain.Core/Contact/ContactModel.cs`

* LOC / SLOC: 250 / 217
* Classes: ContactModel, ContactData, ContactPair, ContactZone, ContactPlane, MotionConstraint, ContactAnalysisResult, ContactRelation
* Methods: 15
* Total complexity: 25
* Await count: 0
* Documentation lines: 75

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ContactModel | 2 | 3 | ⚠️ | 21-60 |
| TryParsePartIndex | 2 | 2 | ⚠️ | 63-68 |
| GetContactsForPart | 1 | 1 | ⚠️ | 71-74 |
| GetContactsBetweenParts | 2 | 1 | ⚠️ | 77-83 |
| ContactData | 8 | 2 | ✅ | 91-119 |
| ToString | 0 | 1 | ✅ | 117-119 |
| ContactPair | 5 | 2 | ✅ | 126-139 |
| ToString | 0 | 1 | ✅ | 136-138 |
| ContactZone | 4 | 3 | ✅ | 156-166 |
| ToString | 0 | 2 | ✅ | 161-165 |
| ContactPlane | 3 | 2 | ✅ | 172-180 |
| ToString | 0 | 1 | ✅ | 177-179 |
| MotionConstraint | 3 | 2 | ✅ | 186-194 |
| ToString | 0 | 1 | ✅ | 191-193 |
| ContactRelation | 5 | 1 | ⚠️ | 238-244 |

### `src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs`

* LOC / SLOC: 202 / 170
* Classes: RTreeBroadPhase, RTreeOptions, RTreeResult
* Methods: 7
* Total complexity: 16
* Await count: 0
* Documentation lines: 31

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Execute | 2 | 6 | ✅ | 39-98 |
| CreateRTree | 2 | 3 | ✅ | 104-117 |
| ExpandBoundingBox | 2 | 2 | ✅ | 123-142 |
| ExecuteOnMeshes | 2 | 1 | ✅ | 148-151 |
| ExecuteOnBreps | 2 | 1 | ✅ | 157-160 |
| ExecuteOnGeometry | 2 | 1 | ✅ | 166-169 |
| ExecuteWithCustomBoxes | 3 | 2 | ✅ | 175-193 |

### `src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs`

* LOC / SLOC: 274 / 241
* Classes: BroadPhaseFactory, SweepAndPruneAlgorithm, RTreeAlgorithm, SweepAndPrune, SapOptions, SapResult, Endpoint
* Methods: 7
* Total complexity: 15
* Await count: 0
* Documentation lines: 52

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Create | 1 | 2 | ✅ | 18-25 |
| Execute | 2 | 2 | ✅ | 102-165 |
| CreateEndpoints | 3 | 7 | ✅ | 171-212 |
| ExecuteOnMeshes | 2 | 1 | ✅ | 228-231 |
| ExecuteOnBreps | 2 | 1 | ✅ | 237-240 |
| ExecuteOnGeometry | 2 | 1 | ✅ | 246-249 |
| ExecuteOnParts | 2 | 1 | ✅ | 255-258 |

### `src/AssemblyChain.Core/Contact/Detection/ContactDetection.cs`

* LOC / SLOC: 116 / 95
* Classes: ContactDetection
* Methods: 2
* Total complexity: 6
* Await count: 0
* Documentation lines: 9

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DetectContacts | 2 | 3 | ✅ | 20-68 |
| DetectContacts | 2 | 3 | ✅ | 74-113 |

### `src/AssemblyChain.Core/Contact/Detection/DetectionOptions.cs`

* LOC / SLOC: 50 / 44
* Classes: DetectionOptions, ContactDetectionConstants
* Methods: 0
* Total complexity: 0
* Await count: 0
* Documentation lines: 6

### `src/AssemblyChain.Core/Contact/Detection/NarrowPhase/BrepContactDetector.cs`

* LOC / SLOC: 58 / 49
* Classes: BrepContactDetector
* Methods: 1
* Total complexity: 3
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DetectBrepContacts | 3 | 3 | ✅ | 20-54 |

### `src/AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs`

* LOC / SLOC: 771 / 643
* Classes: MeshContactDetector, EnhancedDetectionOptions, MeshContactTestUtilities
* Methods: 16
* Total complexity: 46
* Await count: 0
* Documentation lines: 52

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| CreatePreset | 1 | 2 | ⚠️ | 52-87 |
| Sanitize | 0 | 1 | ⚠️ | 90-102 |
| DetectMeshContactsEnhanced | 3 | 2 | ✅ | 115-198 |
| DetectMeshContacts | 3 | 1 | ✅ | 205-214 |
| ValidateInputs | 4 | 2 | ⚠️ | 218-240 |
| ValidateAndFilterContacts | 3 | 6 | ⚠️ | 243-274 |
| DetectContactsWithTightInclusion | 6 | 2 | ✅ | 285-352 |
| DetectContactsWithIntersectionLines | 6 | 12 | ✅ | 360-481 |
| DetectContactsWithSimpleOverlap | 6 | 3 | ✅ | 489-531 |
| GroupCurvesByConnectivity | 2 | 3 | ✅ | 543-569 |
| AreCurvesConnected | 3 | 1 | ✅ | 575-583 |
| ComputeContactRegions | 5 | 3 | ✅ | 592-627 |
| ComputeEdgeContacts | 6 | 4 | ✅ | 634-669 |
| CreateTestCube | 2 | 1 | ✅ | 687-714 |
| RunBasicContactTest | 2 | 2 | ✅ | 720-735 |
| RunPerformanceTest | 2 | 1 | ✅ | 741-765 |

### `src/AssemblyChain.Core/Contact/Detection/NarrowPhase/MixedGeoContactDetector.cs`

* LOC / SLOC: 117 / 102
* Classes: MixedGeoContactDetector
* Methods: 1
* Total complexity: 3
* Await count: 0
* Documentation lines: 9

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DetectMixedGeoContacts | 3 | 3 | ✅ | 20-70 |

### `src/AssemblyChain.Core/Contact/Detection/NarrowPhase/NarrowPhaseDetection.cs`

* LOC / SLOC: 78 / 66
* Classes: NarrowPhaseDetection
* Methods: 2
* Total complexity: 4
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DetectContactsForPair | 6 | 3 | ✅ | 20-69 |
| DetectContact | 2 | 1 | ⚠️ | 73-76 |

### `src/AssemblyChain.Core/Domain/Common/Entity.cs`

* LOC / SLOC: 49 / 38
* Classes: for, Entity
* Methods: 3
* Total complexity: 8
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Entity | 1 | 1 | ⚠️ | 15-17 |
| Equals | 1 | 4 | ⚠️ | 20-31 |
| GetHashCode | 0 | 3 | ⚠️ | 34-48 |

### `src/AssemblyChain.Core/Domain/Common/ValueObject.cs`

* LOC / SLOC: 56 / 42
* Classes: for, ValueObject
* Methods: 2
* Total complexity: 4
* Await count: 0
* Documentation lines: 3

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Equals | 1 | 3 | ⚠️ | 14-24 |
| GetHashCode | 0 | 1 | ⚠️ | 27-36 |

### `src/AssemblyChain.Core/Domain/Entities/Assembly.cs`

* LOC / SLOC: 238 / 202
* Classes: Assembly
* Methods: 12
* Total complexity: 30
* Await count: 0
* Documentation lines: 63

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| UpdateName | 1 | 2 | ✅ | 98-103 |
| UpdateDescription | 1 | 1 | ✅ | 109-111 |
| AddPart | 1 | 4 | ✅ | 117-125 |
| RemovePart | 1 | 1 | ✅ | 131-133 |
| RemovePart | 1 | 1 | ✅ | 139-142 |
| AddSubAssembly | 1 | 3 | ✅ | 148-153 |
| RemoveSubAssembly | 1 | 1 | ✅ | 159-161 |
| GetPart | 1 | 4 | ✅ | 167-180 |
| GetAllParts | 0 | 4 | ✅ | 186-195 |
| GetPhysicsParts | 0 | 1 | ✅ | 201-203 |
| IsValid | 0 | 4 | ✅ | 209-217 |
| HasCircularReference | 1 | 4 | ⚠️ | 220-232 |

### `src/AssemblyChain.Core/Domain/Entities/Joint.cs`

* LOC / SLOC: 145 / 121
* Classes: Joint, JointLimits
* Methods: 5
* Total complexity: 8
* Await count: 0
* Documentation lines: 51

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Activate | 0 | 1 | ✅ | 65-67 |
| Deactivate | 0 | 1 | ✅ | 73-75 |
| InvolvesPart | 1 | 1 | ✅ | 81-83 |
| GetOtherPart | 1 | 3 | ✅ | 89-93 |
| JointLimits | 2 | 2 | ⚠️ | 129-135 |

### `src/AssemblyChain.Core/Domain/Entities/Part.cs`

* LOC / SLOC: 174 / 148
* Classes: Part
* Methods: 6
* Total complexity: 7
* Await count: 0
* Documentation lines: 63

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| UpdateName | 1 | 2 | ✅ | 102-107 |
| UpdateGeometry | 1 | 1 | ✅ | 113-115 |
| UpdatePhysics | 1 | 1 | ✅ | 121-123 |
| UpdateMaterial | 1 | 1 | ✅ | 129-131 |
| WithPhysics | 1 | 1 | ✅ | 137-139 |
| WithMaterial | 1 | 1 | ✅ | 145-147 |

### `src/AssemblyChain.Core/Domain/Interfaces/IAssemblyService.cs`

* LOC / SLOC: 81 / 71
* Classes: AssemblyValidationResult, AssemblyProperties, CollisionInfo
* Methods: 0
* Total complexity: 0
* Await count: 0
* Documentation lines: 33

### `src/AssemblyChain.Core/Domain/Interfaces/IPartRepository.cs`

* LOC / SLOC: 57 / 48
* Classes: —
* Methods: 0
* Total complexity: 0
* Await count: 0
* Documentation lines: 30

### `src/AssemblyChain.Core/Domain/Services/DomainServices.cs`

* LOC / SLOC: 303 / 256
* Classes: DomainServices, DisassemblyAnalysis, AssemblySequence, AssemblyStep, ValidationResult, StabilityAnalysis
* Methods: 16
* Total complexity: 29
* Await count: 3
* Documentation lines: 33

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DomainServices | 2 | 1 | ⚠️ | 20-23 |
| AnalyzeDisassemblySequenceAsync | 2 | 3 | ✅ | 29-56 |
| FindBlockingPartsAsync | 3 | 5 | ✅ | 62-78 |
| CalculateOptimalSequenceAsync | 1 | 4 | ✅ | 84-118 |
| ValidatePart | 1 | 2 | ✅ | 124-156 |
| AnalyzeStabilityAsync | 1 | 2 | ✅ | 162-185 |
| PartsIntersectAsync | 2 | 1 | ⚠️ | 188-196 |
| IsBlocking | 2 | 2 | ⚠️ | 199-205 |
| CalculateCenterOfMass | 1 | 1 | ⚠️ | 208-211 |
| CalculateSupportPolygon | 1 | 2 | ⚠️ | 214-228 |
| AddError | 1 | 1 | ⚠️ | 241-242 |
| AddStep | 3 | 1 | ⚠️ | 253-255 |
| AddError | 1 | 1 | ⚠️ | 258-259 |
| AssemblyStep | 3 | 1 | ⚠️ | 270-274 |
| AddError | 1 | 1 | ⚠️ | 288-290 |
| AddWarning | 1 | 1 | ⚠️ | 289-290 |

### `src/AssemblyChain.Core/Domain/ValueObjects/MaterialProperties.cs`

* LOC / SLOC: 93 / 79
* Classes: MaterialProperties
* Methods: 2
* Total complexity: 2
* Await count: 0
* Documentation lines: 30

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| MaterialProperties | 9 | 1 | ⚠️ | 57-67 |
| GetEqualityComponents | 0 | 1 | ⚠️ | 72-82 |

### `src/AssemblyChain.Core/Domain/ValueObjects/PartGeometry.cs`

* LOC / SLOC: 89 / 76
* Classes: PartGeometry
* Methods: 3
* Total complexity: 4
* Await count: 0
* Documentation lines: 33

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| PartGeometry | 2 | 1 | ✅ | 51-58 |
| PartGeometry | 5 | 1 | ✅ | 64-71 |
| GetEqualityComponents | 0 | 2 | ⚠️ | 79-84 |

### `src/AssemblyChain.Core/Domain/ValueObjects/PhysicsProperties.cs`

* LOC / SLOC: 61 / 51
* Classes: PhysicsProperties
* Methods: 2
* Total complexity: 2
* Await count: 0
* Documentation lines: 18

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| PhysicsProperties | 5 | 1 | ⚠️ | 37-43 |
| GetEqualityComponents | 0 | 1 | ⚠️ | 47-53 |

### `src/AssemblyChain.Core/Graph/ConstraintGraphBuilder.cs`

* LOC / SLOC: 139 / 113
* Classes: ConstraintGraphBuilder
* Methods: 4
* Total complexity: 12
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| BuildConstraints | 2 | 1 | ✅ | 17-34 |
| BuildPartConstraints | 2 | 3 | ⚠️ | 39-71 |
| BuildGroupConstraints | 2 | 5 | ⚠️ | 75-116 |
| CheckExternalBlocking | 2 | 3 | ⚠️ | 120-133 |

### `src/AssemblyChain.Core/Graph/GNNAnalyzer.cs`

* LOC / SLOC: 583 / 494
* Classes: GNNAnalyzer, NodeFeatures, GNNAnalysisResult
* Methods: 14
* Total complexity: 33
* Await count: 0
* Documentation lines: 55

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Analyze | 3 | 3 | ✅ | 25-51 |
| UpdateAnalysis | 4 | 5 | ✅ | 59-114 |
| InitializeNodeFeatures | 3 | 3 | ✅ | 123-161 |
| UpdateNodeFeatures | 4 | 1 | ✅ | 170-172 |
| UpdateNodeFeaturesIncrementally | 5 | 3 | ✅ | 181-239 |
| CalculateMessage | 3 | 1 | ✅ | 249-261 |
| HasConverged | 4 | 3 | ✅ | 267-280 |
| ComputeSingleScores | 3 | 2 | ✅ | 287-297 |
| ComputeSingleScore | 1 | 1 | ✅ | 304-313 |
| CalculatePairAffinity | 5 | 3 | ✅ | 398-434 |
| CalculateSurfaceArea | 1 | 4 | ✅ | 442-469 |
| CalculateExposureScore | 3 | 2 | ✅ | 475-515 |
| Clone | 0 | 1 | ⚠️ | 532-543 |
| GNNAnalysisResult | 1 | 1 | ⚠️ | 556-558 |

### `src/AssemblyChain.Core/Graph/GraphOptions.cs`

* LOC / SLOC: 14 / 12
* Classes: struct
* Methods: 0
* Total complexity: 0
* Await count: 0
* Documentation lines: 3

### `src/AssemblyChain.Core/Graph/GraphViews.cs`

* LOC / SLOC: 76 / 62
* Classes: namespace, GraphViews, Dbg, BlockingEdge, DirectionalBlockingGraph, AssemblyGraph, Node
* Methods: 5
* Total complexity: 5
* Await count: 0
* Documentation lines: 0

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| BuildDbgForDirections | 2 | 1 | ⚠️ | 13-16 |
| BlockingEdge | 2 | 1 | ⚠️ | 30-33 |
| GetBlockingScore | 1 | 1 | ⚠️ | 53-54 |
| Node | 1 | 1 | ⚠️ | 61-61 |
| AssemblyGraph | 3 | 1 | ⚠️ | 67-70 |

### `src/AssemblyChain.Core/Model/AssemblyModel.cs`

* LOC / SLOC: 86 / 73
* Classes: AssemblyModel
* Methods: 1
* Total complexity: 3
* Await count: 0
* Documentation lines: 25

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| AssemblyModel | 3 | 3 | ⚠️ | 50-80 |

### `src/AssemblyChain.Core/Model/AssemblyModelFactory.cs`

* LOC / SLOC: 42 / 39
* Classes: AssemblyModelFactory
* Methods: 2
* Total complexity: 3
* Await count: 0
* Documentation lines: 9

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Create | 1 | 2 | ✅ | 21-31 |
| MaterializeParts | 1 | 1 | ⚠️ | 34-39 |

### `src/AssemblyChain.Core/Model/ConstraintModel.cs`

* LOC / SLOC: 72 / 62
* Classes: ConstraintModel
* Methods: 6
* Total complexity: 9
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ConstraintModel | 7 | 1 | ⚠️ | 20-26 |
| GetPartConstraints | 1 | 1 | ⚠️ | 34-36 |
| GetGroupConstraints | 1 | 1 | ⚠️ | 39-41 |
| GetGroupConstraints | 1 | 1 | ⚠️ | 44-48 |
| CanPartMove | 3 | 2 | ⚠️ | 51-55 |
| CanGroupMove | 3 | 3 | ⚠️ | 58-66 |

### `src/AssemblyChain.Core/Model/GraphModel.cs`

* LOC / SLOC: 82 / 73
* Classes: GraphModel, BlockingGraph, NonDirectionalBlockingGraph, BlockingEdge, StronglyConnectedComponent
* Methods: 7
* Total complexity: 8
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| GraphModel | 7 | 1 | ⚠️ | 25-32 |
| GetInDegree | 1 | 1 | ⚠️ | 41-43 |
| GetFreeParts | 0 | 1 | ⚠️ | 46-48 |
| GetComponentForNode | 1 | 1 | ⚠️ | 51-53 |
| AreInSameComponent | 2 | 1 | ⚠️ | 56-60 |
| StronglyConnectedComponent | 0 | 1 | ⚠️ | 73-73 |
| StronglyConnectedComponent | 3 | 2 | ⚠️ | 74-78 |

### `src/AssemblyChain.Core/Model/MotionModel.cs`

* LOC / SLOC: 86 / 72
* Classes: MotionModel
* Methods: 8
* Total complexity: 14
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| MotionModel | 5 | 1 | ⚠️ | 20-24 |
| GetPartMotionRays | 1 | 1 | ⚠️ | 30-32 |
| GetGroupMotionRays | 1 | 1 | ⚠️ | 35-39 |
| GetGroupMotionRays | 1 | 1 | ⚠️ | 42-44 |
| IsMotionFeasible | 3 | 4 | ⚠️ | 47-57 |
| IsGroupMotionFeasible | 3 | 4 | ⚠️ | 60-70 |
| GetAllGroupKeys | 0 | 1 | ⚠️ | 73-75 |
| ParseGroupKey | 1 | 1 | ⚠️ | 78-80 |

### `src/AssemblyChain.Core/Model/SolverModel.cs`

* LOC / SLOC: 131 / 117
* Classes: DgSolverModel, Step
* Methods: 6
* Total complexity: 11
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DgSolverModel | 11 | 2 | ⚠️ | 27-40 |
| GetStep | 1 | 2 | ⚠️ | 53-57 |
| GetVector | 1 | 2 | ⚠️ | 60-64 |
| ToAssemblySequence | 0 | 3 | ⚠️ | 67-102 |
| GetSummary | 0 | 1 | ⚠️ | 105-109 |
| Step | 3 | 1 | ⚠️ | 116-120 |

### `src/AssemblyChain.Core/Motion/ConeIntersection.cs`

* LOC / SLOC: 58 / 51
* Classes: ConeIntersection
* Methods: 4
* Total complexity: 8
* Await count: 0
* Documentation lines: 15

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| IsPointInHalfspace | 3 | 1 | ✅ | 16-19 |
| IsDirectionFeasible | 3 | 4 | ✅ | 25-33 |
| FindConeBoundary | 1 | 1 | ✅ | 39-41 |
| ComputeExtremeRays | 2 | 2 | ✅ | 47-50 |

### `src/AssemblyChain.Core/Motion/MotionEvaluator.cs`

* LOC / SLOC: 123 / 101
* Classes: MotionEvaluator
* Methods: 6
* Total complexity: 20
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| EvaluateMotion | 2 | 1 | ✅ | 18-23 |
| ComputePartMotionRays | 2 | 2 | ⚠️ | 28-40 |
| ComputeMotionRaysForPart | 3 | 4 | ⚠️ | 44-57 |
| ComputeGroupMotionRays | 2 | 2 | ⚠️ | 61-76 |
| ComputeMotionRaysForGroup | 3 | 4 | ⚠️ | 80-96 |
| GenerateCombinations | 2 | 7 | ⚠️ | 100-117 |

### `src/AssemblyChain.Core/Motion/MotionOptions.cs`

* LOC / SLOC: 15 / 13
* Classes: struct
* Methods: 0
* Total complexity: 0
* Await count: 0
* Documentation lines: 3

### `src/AssemblyChain.Core/Motion/PoseEstimator.cs`

* LOC / SLOC: 26 / 22
* Classes: PoseEstimator
* Methods: 2
* Total complexity: 2
* Await count: 0
* Documentation lines: 0

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| PoseEstimator | 2 | 1 | ⚠️ | 12-15 |
| GenerateCandidates | 1 | 1 | ⚠️ | 18-22 |

### `src/AssemblyChain.Core/Motion/PoseTypes.cs`

* LOC / SLOC: 16 / 14
* Classes: PoseCandidate
* Methods: 1
* Total complexity: 1
* Await count: 0
* Documentation lines: 0

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| PoseCandidate | 2 | 1 | ⚠️ | 9-12 |

### `src/AssemblyChain.Core/Solver/CSPSolver.cs`

* LOC / SLOC: 72 / 65
* Classes: CSPsolver
* Methods: 1
* Total complexity: 1
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Solve | 4 | 1 | ⚠️ | 22-63 |

### `src/AssemblyChain.Core/Solver/MILPSolver.cs`

* LOC / SLOC: 65 / 59
* Classes: MILPsolver
* Methods: 1
* Total complexity: 1
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Solve | 4 | 1 | ⚠️ | 17-56 |

### `src/AssemblyChain.Core/Solver/SATSolver.cs`

* LOC / SLOC: 65 / 59
* Classes: SATsolver
* Methods: 1
* Total complexity: 1
* Await count: 0
* Documentation lines: 4

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Solve | 4 | 1 | ⚠️ | 17-56 |

### `src/AssemblyChain.Core/Solver/SolverOptions.cs`

* LOC / SLOC: 29 / 26
* Classes: struct
* Methods: 0
* Total complexity: 0
* Await count: 0
* Documentation lines: 6

### `src/AssemblyChain.Core/Toolkit/BBox/BoundingHelpers.cs`

* LOC / SLOC: 287 / 254
* Classes: BoundingHelpers, ExpansionOptions, VoxelOptions
* Methods: 13
* Total complexity: 34
* Await count: 0
* Documentation lines: 48

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ExpandBoundingBox | 2 | 3 | ✅ | 40-70 |
| IntersectBoundingBoxes | 1 | 5 | ✅ | 76-96 |
| UnionBoundingBoxes | 1 | 5 | ✅ | 102-122 |
| BoundingBoxesIntersect | 3 | 2 | ✅ | 128-137 |
| BoundingBoxSurfaceArea | 1 | 2 | ✅ | 143-147 |
| BoundingBoxVolume | 1 | 2 | ✅ | 153-157 |
| VoxelizeBoundingBox | 2 | 3 | ✅ | 163-204 |
| IsOnBoundary | 3 | 1 | ✅ | 210-216 |
| CreateBoundingBox | 1 | 3 | ✅ | 222-230 |
| BoundingBoxCenter | 1 | 2 | ✅ | 236-243 |
| BoundingBoxSize | 1 | 2 | ✅ | 249-256 |
| ContainsPoint | 3 | 2 | ✅ | 262-267 |
| BoundingBoxAspectRatio | 1 | 2 | ✅ | 273-281 |

### `src/AssemblyChain.Core/Toolkit/Brep/BrepUtilities.cs`

* LOC / SLOC: 302 / 259
* Classes: BrepUtilities, BrepOptions, ProcessingResult
* Methods: 7
* Total complexity: 28
* Await count: 0
* Documentation lines: 39

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ProcessBrep | 2 | 2 | ✅ | 41-100 |
| SplitIntersectingFaces | 2 | 2 | ✅ | 106-119 |
| MergeCoplanarFaces | 2 | 9 | ✅ | 125-162 |
| AreCoplanar | 3 | 4 | ✅ | 168-178 |
| AreAdjacentFaces | 3 | 4 | ✅ | 184-200 |
| DetectClosureIssues | 2 | 6 | ✅ | 206-222 |
| HasSelfIntersections | 1 | 1 | ✅ | 228-231 |

### `src/AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs`

* LOC / SLOC: 496 / 415
* Classes: PlanarOps, PlanarOptions, PlanarResult, PlaneComparer, CoplanarContactResult
* Methods: 11
* Total complexity: 43
* Await count: 0
* Documentation lines: 36

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ExtractPlanarFaces | 2 | 2 | ✅ | 43-91 |
| ExtractFacePlane | 1 | 3 | ✅ | 97-115 |
| FitPlaneToFace | 1 | 6 | ✅ | 121-160 |
| ProjectFaceTo2D | 2 | 6 | ✅ | 246-290 |
| Union2DPolygons | 1 | 1 | ⚠️ | 293-295 |
| AreCoplanar | 3 | 2 | ⚠️ | 298-309 |
| PlaneComparer | 1 | 1 | ⚠️ | 316-318 |
| Equals | 2 | 1 | ⚠️ | 321-323 |
| GetHashCode | 1 | 1 | ⚠️ | 326-328 |
| DetectCoplanarContacts | 5 | 18 | ✅ | 348-461 |
| MakeContact | 2 | 2 | ✅ | 472-492 |

### `src/AssemblyChain.Core/Toolkit/Geometry/MeshGeometry.cs`

* LOC / SLOC: 235 / 209
* Classes: MeshGeometry
* Methods: 8
* Total complexity: 18
* Await count: 0
* Documentation lines: 47

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| CalculateFaceCenter | 2 | 1 | ✅ | 22-39 |
| CalculateFaceArea | 2 | 1 | ✅ | 48-74 |
| CalculateTriangleArea | 3 | 1 | ✅ | 80-85 |
| GetFaceNormal | 2 | 3 | ✅ | 94-103 |
| CalculateMinDistance | 3 | 5 | ✅ | 113-152 |
| ApproximateArea | 1 | 3 | ✅ | 160-172 |
| ComputeGeometryArea | 1 | 2 | ✅ | 180-205 |
| InferContactPlane | 1 | 2 | ✅ | 213-232 |

### `src/AssemblyChain.Core/Toolkit/Geometry/PlaneOperations.cs`

* LOC / SLOC: 341 / 293
* Classes: PlaneOperations
* Methods: 10
* Total complexity: 25
* Await count: 0
* Documentation lines: 75

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| GroupFacesByPlanes | 2 | 3 | ✅ | 21-47 |
| ArePlanesCoplanar | 3 | 2 | ✅ | 57-64 |
| FitPlaneFromCurves | 2 | 3 | ✅ | 73-95 |
| AreFacesCoplanarAndClose | 3 | 2 | ✅ | 105-116 |
| GetFacePlane | 2 | 3 | ✅ | 125-151 |
| ComputeFaceIntersectionGeometry | 6 | 6 | ✅ | 164-214 |
| GetFaceVertices | 2 | 1 | ✅ | 223-230 |
| ProjectPolylineToPlane | 2 | 2 | ✅ | 239-249 |
| ComputePolygonIntersection2D | 3 | 2 | ✅ | 259-280 |
| ConvertPolygon2DTo3D | 2 | 1 | ✅ | 289-292 |

### `src/AssemblyChain.Core/Toolkit/Intersection/BrepBrepIntersect.cs`

* LOC / SLOC: 308 / 268
* Classes: BrepBrepIntersect, IntersectionOptions, IntersectionResult
* Methods: 8
* Total complexity: 20
* Await count: 0
* Documentation lines: 33

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ComputeIntersection | 3 | 2 | ✅ | 42-87 |
| ComputeSurfaceIntersections | 3 | 2 | ✅ | 96-116 |
| MergeCoplanarIntersections | 2 | 4 | ✅ | 125-171 |
| CanMergeCurves | 3 | 2 | ✅ | 179-197 |
| MergeCurveGroup | 2 | 1 | ✅ | 203-214 |
| ExtractPointsFromCurves | 3 | 3 | ✅ | 220-245 |
| SamplePointsOnCurve | 2 | 2 | ✅ | 254-277 |
| ComputeMultipleIntersections | 2 | 4 | ✅ | 283-299 |

### `src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs`

* LOC / SLOC: 366 / 317
* Classes: MeshMeshIntersect, IntersectionOptions, IntersectionResult, ContactDetectionResult
* Methods: 7
* Total complexity: 18
* Await count: 0
* Documentation lines: 33

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| DetectContactsBasedOnIntersection | 5 | 1 | ✅ | 60-111 |
| ComputeIntersection | 3 | 2 | ✅ | 122-198 |
| ExtractPointsFromIntersections | 2 | 4 | ✅ | 207-246 |
| ComputeMultipleIntersections | 2 | 4 | ✅ | 254-270 |
| BoundingBoxCheck | 3 | 2 | ✅ | 278-285 |
| ApproximateIntersection | 4 | 2 | ✅ | 291-335 |
| SamplePointsInBoundingBox | 2 | 3 | ✅ | 345-359 |

### `src/AssemblyChain.Core/Toolkit/Math/Clustering.cs`

* LOC / SLOC: 317 / 291
* Classes: Clustering, KMeansOptions, KMeansResult
* Methods: 12
* Total complexity: 39
* Await count: 0
* Documentation lines: 3

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| KMeans | 3 | 4 | ⚠️ | 31-81 |
| InitializeKMeansPlusPlus | 3 | 4 | ⚠️ | 84-114 |
| InitializeRandom | 3 | 2 | ⚠️ | 117-127 |
| FindNearestCentroid | 2 | 3 | ⚠️ | 130-143 |
| UpdateCentroids | 3 | 2 | ⚠️ | 146-169 |
| CalculateInertia | 3 | 2 | ⚠️ | 172-181 |
| HierarchicalClustering | 2 | 2 | ⚠️ | 184-210 |
| ClusterDistance | 2 | 4 | ⚠️ | 213-224 |
| DBSCAN | 3 | 5 | ⚠️ | 227-268 |
| FindNeighbors | 3 | 3 | ⚠️ | 271-279 |
| SilhouetteScore | 3 | 5 | ⚠️ | 282-300 |
| AverageDistanceToCluster | 4 | 3 | ⚠️ | 303-311 |

### `src/AssemblyChain.Core/Toolkit/Math/ConvexCone.cs`

* LOC / SLOC: 194 / 170
* Classes: ConvexCone, Halfspace, Cone
* Methods: 22
* Total complexity: 40
* Await count: 0
* Documentation lines: 9

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Halfspace | 2 | 1 | ⚠️ | 21-25 |
| Halfspace | 2 | 1 | ⚠️ | 28-32 |
| SignedDistance | 1 | 1 | ⚠️ | 35-37 |
| Contains | 2 | 1 | ⚠️ | 40-42 |
| AddHalfspace | 1 | 1 | ⚠️ | 53-58 |
| AddHalfspace | 2 | 1 | ⚠️ | 54-58 |
| Contains | 2 | 1 | ⚠️ | 55-58 |
| IsEmpty | 0 | 1 | ⚠️ | 56-58 |
| GetExtremeRays | 0 | 1 | ⚠️ | 57-58 |
| CreateHalfspaceFromContact | 3 | 1 | ⚠️ | 60-62 |
| CreateConeFromContacts | 1 | 3 | ⚠️ | 65-73 |
| IntersectCones | 2 | 3 | ⚠️ | 76-81 |
| ComputeExtremeRays | 1 | 1 | ⚠️ | 84-86 |
| IsDirectionFeasible | 3 | 1 | ⚠️ | 89-91 |
| FindConeBoundary | 1 | 1 | ⚠️ | 94-96 |
| ComputeDualCone | 1 | 3 | ⚠️ | 99-107 |
| IsPointed | 1 | 1 | ⚠️ | 110-113 |
| GetDimension | 1 | 4 | ⚠️ | 116-122 |
| AreCoplanar | 1 | 4 | ⚠️ | 125-136 |
| GenerateMotionRays | 3 | 4 | ⚠️ | 139-159 |
| Slerp | 3 | 2 | ⚠️ | 162-171 |
| GenerateUniformRays | 1 | 3 | ⚠️ | 174-188 |

### `src/AssemblyChain.Core/Toolkit/Math/LinearAlgebra.cs`

* LOC / SLOC: 173 / 158
* Classes: LinearAlgebra
* Methods: 10
* Total complexity: 18
* Await count: 0
* Documentation lines: 37

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| GramSchmidtOrthogonalize | 1 | 4 | ✅ | 16-37 |
| ProjectOnto | 2 | 2 | ✅ | 43-48 |
| OrthogonalComplement | 1 | 1 | ✅ | 54-61 |
| AngleBetween | 2 | 2 | ✅ | 67-74 |
| AreLinearlyDependent | 3 | 2 | ✅ | 80-84 |
| Determinant | 3 | 1 | ✅ | 90-94 |
| SolveLinearSystem | 4 | 2 | ✅ | 100-107 |
| Rank | 2 | 2 | ✅ | 113-117 |
| NullSpace | 1 | 1 | ✅ | 123-126 |
| NullSpace | 2 | 1 | ✅ | 132-142 |

### `src/AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs`

* LOC / SLOC: 353 / 312
* Classes: MeshPreprocessor, PreprocessingOptions, PreprocessingResult
* Methods: 5
* Total complexity: 8
* Await count: 0
* Documentation lines: 19

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| CreateBalanced | 0 | 1 | ⚠️ | 33-42 |
| CreateFast | 0 | 1 | ⚠️ | 45-54 |
| PreprocessMesh | 2 | 1 | ✅ | 82-197 |
| CreatePreprocessedMesh | 2 | 2 | ✅ | 203-254 |
| GenerateReport | 1 | 3 | ✅ | 308-350 |

### `src/AssemblyChain.Core/Toolkit/Mesh/MeshSpatialIndex.cs`

* LOC / SLOC: 211 / 185
* Classes: MeshSpatialIndex
* Methods: 12
* Total complexity: 26
* Await count: 0
* Documentation lines: 41

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| MeshSpatialIndex | 2 | 1 | ✅ | 24-33 |
| BuildIndex | 0 | 3 | ✅ | 39-51 |
| GetNearbyFaces | 2 | 4 | ✅ | 60-74 |
| GetFacesInRegion | 1 | 7 | ✅ | 82-116 |
| GetStatistics | 0 | 1 | ✅ | 122-131 |
| GetCellKey | 1 | 1 | ✅ | 137-144 |
| GetXFromKey | 1 | 1 | ✅ | 150-210 |
| GetYFromKey | 1 | 1 | ✅ | 151-210 |
| GetZFromKey | 1 | 1 | ⚠️ | 152-210 |
| GetCellKeyFromXYZ | 3 | 1 | ✅ | 157-159 |
| GetNearbyCells | 2 | 4 | ✅ | 165-185 |
| CalculateFaceCenter | 2 | 1 | ✅ | 191-208 |

### `src/AssemblyChain.Core/Toolkit/Mesh/MeshToBrep.cs`

* LOC / SLOC: 223 / 194
* Classes: MeshToBrep, ConversionOptions, ConversionResult
* Methods: 4
* Total complexity: 11
* Await count: 0
* Documentation lines: 28

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ConvertToBrep | 2 | 2 | ✅ | 41-87 |
| ComputeMeshArea | 1 | 3 | ✅ | 128-165 |
| ComputeBrepArea | 1 | 4 | ✅ | 171-184 |
| ValidateConversion | 4 | 2 | ✅ | 190-216 |

### `src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs`

* LOC / SLOC: 495 / 436
* Classes: MeshOptimizer, OptimizationOptions, OptimizationResult, Point3dComparer
* Methods: 14
* Total complexity: 31
* Await count: 0
* Documentation lines: 23

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| OptimizeMesh | 2 | 1 | ✅ | 47-99 |
| WeldDuplicateVertices | 3 | 2 | ⚠️ | 102-119 |
| ReduceVertices | 3 | 2 | ⚠️ | 122-196 |
| UnifyMeshNormals | 2 | 1 | ⚠️ | 199-209 |
| SmoothMesh | 3 | 2 | ⚠️ | 212-258 |
| RemoveRedundantVertices | 2 | 3 | ✅ | 264-295 |
| SimplifyMesh | 2 | 3 | ✅ | 301-348 |
| ImproveMeshQuality | 2 | 3 | ✅ | 354-391 |
| UpdateFacesReferencingVertex | 3 | 6 | ⚠️ | 394-405 |
| GetVertexNeighbors | 2 | 4 | ⚠️ | 408-429 |
| CalculateFaceArea | 2 | 1 | ⚠️ | 432-467 |
| Point3dComparer | 1 | 1 | ⚠️ | 474-476 |
| Equals | 2 | 1 | ⚠️ | 479-481 |
| GetHashCode | 1 | 1 | ⚠️ | 484-491 |

### `src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs`

* LOC / SLOC: 369 / 329
* Classes: MeshRepair, RepairOptions, RepairResult
* Methods: 9
* Total complexity: 19
* Await count: 0
* Documentation lines: 12

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| RepairMesh | 2 | 2 | ✅ | 43-82 |
| FillMeshHoles | 3 | 2 | ⚠️ | 85-138 |
| FixNonManifoldEdges | 3 | 2 | ⚠️ | 141-161 |
| RemoveDuplicateFaces | 3 | 3 | ⚠️ | 164-202 |
| HealNakedEdges | 3 | 1 | ⚠️ | 205-209 |
| GroupNakedEdgesIntoLoops | 2 | 3 | ⚠️ | 212-289 |
| CalculateLoopArea | 2 | 2 | ⚠️ | 292-319 |
| TryFillHole | 2 | 2 | ⚠️ | 322-350 |
| GetFaceSignature | 1 | 2 | ⚠️ | 353-366 |

### `src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs`

* LOC / SLOC: 341 / 304
* Classes: MeshValidator, ValidationOptions, ValidationResult
* Methods: 9
* Total complexity: 19
* Await count: 0
* Documentation lines: 18

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ValidateMesh | 2 | 1 | ✅ | 42-103 |
| FinalValidation | 2 | 1 | ✅ | 109-112 |
| ValidateMeshForContactDetection | 3 | 1 | ✅ | 118-122 |
| CheckTopology | 3 | 3 | ⚠️ | 125-151 |
| CheckDegenerateFaces | 3 | 5 | ⚠️ | 154-188 |
| CheckNormals | 2 | 2 | ⚠️ | 191-235 |
| CheckBoundingBox | 3 | 2 | ⚠️ | 238-267 |
| IsDegenerateFace | 2 | 1 | ⚠️ | 270-315 |
| FindDuplicateFaces | 1 | 3 | ⚠️ | 318-338 |

### `src/AssemblyChain.Core/Toolkit/Utils/CacheManager.cs`

* LOC / SLOC: 230 / 209
* Classes: CacheManager, CacheItem, CacheStatistics, GlobalCache
* Methods: 12
* Total complexity: 27
* Await count: 1
* Documentation lines: 46

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| CacheManager | 2 | 1 | ⚠️ | 21-26 |
| Get | 1 | 4 | ✅ | 32-44 |
| Set | 3 | 2 | ✅ | 50-62 |
| GetOrCreate | 3 | 2 | ✅ | 68-74 |
| GetOrCreateAsync | 3 | 2 | ✅ | 80-86 |
| Remove | 1 | 2 | ✅ | 92-95 |
| Clear | 0 | 2 | ✅ | 101-104 |
| GetStatistics | 0 | 2 | ✅ | 110-123 |
| CleanupExpiredItems | 1 | 3 | ✅ | 129-139 |
| CleanupOldItems | 0 | 4 | ✅ | 145-158 |
| EstimateMemoryUsage | 0 | 1 | ✅ | 164-168 |
| Dispose | 0 | 2 | ⚠️ | 171-176 |

### `src/AssemblyChain.Core/Toolkit/Utils/ContactDetectionHelpers.cs`

* LOC / SLOC: 26 / 25
* Classes: ContactDetectionHelpers
* Methods: 1
* Total complexity: 2
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| IsContactBlocking | 2 | 2 | ✅ | 15-23 |

### `src/AssemblyChain.Core/Toolkit/Utils/ExtremeRayExtractor.cs`

* LOC / SLOC: 81 / 72
* Classes: ExtremeRayExtractor
* Methods: 2
* Total complexity: 12
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| Extract | 2 | 6 | ✅ | 16-42 |
| SampleRaysEvenly | 2 | 6 | ⚠️ | 45-75 |

### `src/AssemblyChain.Core/Toolkit/Utils/GroupCandidates.cs`

* LOC / SLOC: 118 / 103
* Classes: GroupCandidates
* Methods: 4
* Total complexity: 19
* Await count: 0
* Documentation lines: 18

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| GenerateCandidates | 2 | 4 | ✅ | 16-43 |
| GenerateSubgroups | 2 | 3 | ✅ | 49-57 |
| GenerateCombinations | 2 | 7 | ✅ | 63-78 |
| EvaluateCandidate | 2 | 5 | ✅ | 84-99 |

### `src/AssemblyChain.Core/Toolkit/Utils/HalfspaceCone.cs`

* LOC / SLOC: 47 / 41
* Classes: HalfspaceCone
* Methods: 3
* Total complexity: 6
* Await count: 0
* Documentation lines: 12

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| IsPointInHalfspace | 3 | 1 | ✅ | 16-19 |
| IsDirectionFeasible | 3 | 4 | ✅ | 25-33 |
| FindConeBoundary | 1 | 1 | ✅ | 39-41 |

### `src/AssemblyChain.Core/Toolkit/Utils/Hashing.cs`

* LOC / SLOC: 103 / 92
* Classes: Hashing
* Methods: 10
* Total complexity: 11
* Await count: 0
* Documentation lines: 12

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| ForAssembly | 1 | 2 | ⚠️ | 16-24 |
| ForContacts | 2 | 1 | ⚠️ | 27-30 |
| ForGraphs | 2 | 1 | ⚠️ | 33-36 |
| ForMotion | 2 | 1 | ⚠️ | 39-42 |
| ForConstraints | 2 | 1 | ⚠️ | 45-48 |
| ForSolver | 2 | 1 | ⚠️ | 51-54 |
| ForCentroid | 2 | 1 | ✅ | 60-66 |
| ForArea | 2 | 1 | ✅ | 72-76 |
| ForPlane | 2 | 1 | ✅ | 82-91 |
| ComputeHash | 1 | 1 | ⚠️ | 94-99 |

### `src/AssemblyChain.Core/Toolkit/Utils/JsonSerializer.cs`

* LOC / SLOC: 278 / 248
* Classes: JsonSerializer, SerializationOptions, SerializationException, Point3dConverter, Vector3dConverter, PlaneConverter, BoundingBoxConverter, GuidConverter
* Methods: 18
* Total complexity: 40
* Await count: 0
* Documentation lines: 3

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| JsonSerializer | 0 | 1 | ⚠️ | 17-33 |
| Serialize | 2 | 1 | ⚠️ | 44-80 |
| SaveToFile | 3 | 1 | ⚠️ | 95-98 |
| WriteJson | 3 | 1 | ⚠️ | 119-126 |
| ReadJson | 4 | 7 | ⚠️ | 129-147 |
| CanConvert | 1 | 1 | ⚠️ | 150-151 |
| WriteJson | 3 | 1 | ⚠️ | 155-162 |
| ReadJson | 4 | 7 | ⚠️ | 165-183 |
| CanConvert | 1 | 1 | ⚠️ | 186-187 |
| WriteJson | 3 | 1 | ⚠️ | 191-197 |
| ReadJson | 4 | 6 | ⚠️ | 200-217 |
| CanConvert | 1 | 1 | ⚠️ | 220-221 |
| WriteJson | 3 | 1 | ⚠️ | 225-231 |
| ReadJson | 4 | 6 | ⚠️ | 234-251 |
| CanConvert | 1 | 1 | ⚠️ | 254-255 |
| WriteJson | 3 | 1 | ⚠️ | 259-261 |
| ReadJson | 4 | 1 | ⚠️ | 264-266 |
| CanConvert | 1 | 1 | ⚠️ | 269-270 |

### `src/AssemblyChain.Core/Toolkit/Utils/ParallelProcessor.cs`

* LOC / SLOC: 196 / 176
* Classes: ParallelProcessor, ParallelProcessingConfig
* Methods: 2
* Total complexity: 6
* Await count: 3
* Documentation lines: 24

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| IsParallelProcessingSupported | 0 | 1 | ✅ | 163-165 |
| GetRecommendedParallelism | 1 | 5 | ✅ | 171-177 |

### `src/AssemblyChain.Core/Toolkit/Utils/PerformanceMonitor.cs`

* LOC / SLOC: 224 / 199
* Classes: PerformanceMonitor, PerformanceMonitorHelper
* Methods: 15
* Total complexity: 20
* Await count: 0
* Documentation lines: 66

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| PerformanceMonitor | 0 | 1 | ✅ | 21-24 |
| StartTimer | 1 | 1 | ✅ | 31-34 |
| StopTimer | 1 | 2 | ✅ | 41-49 |
| GetDuration | 1 | 1 | ✅ | 57-59 |
| LogDebug | 1 | 1 | ✅ | 66-70 |
| GenerateReport | 0 | 3 | ✅ | 77-105 |
| GetStatistics | 0 | 1 | ✅ | 112-126 |
| Reset | 0 | 1 | ✅ | 132-135 |
| GetOperationNames | 0 | 1 | ✅ | 141-143 |
| IsRunning | 1 | 1 | ✅ | 151-153 |
| GetRunningOperations | 0 | 1 | ✅ | 160-162 |
| GetMonitor | 1 | 2 | ✅ | 178-184 |
| RemoveMonitor | 1 | 1 | ✅ | 191-193 |
| ClearAll | 0 | 1 | ✅ | 199-201 |
| GenerateGlobalReport | 0 | 2 | ✅ | 208-221 |

### `src/AssemblyChain.Core/Toolkit/Utils/Tolerance.cs`

* LOC / SLOC: 171 / 150
* Classes: Tolerance, ToleranceSettings, ToleranceContext
* Methods: 16
* Total complexity: 21
* Await count: 0
* Documentation lines: 6

| Method | Params | Complexity | Docs | Span |
| --- | --- | --- | --- | --- |
| GetAdaptiveTolerance | 1 | 2 | ⚠️ | 37-40 |
| GetAdaptiveTolerance | 1 | 2 | ⚠️ | 43-47 |
| Equal | 3 | 1 | ⚠️ | 50-53 |
| IsZero | 2 | 1 | ⚠️ | 56-59 |
| PointsEqual | 3 | 1 | ⚠️ | 62-65 |
| VectorsParallel | 3 | 2 | ⚠️ | 68-73 |
| PlanesCoplanar | 3 | 2 | ⚠️ | 76-82 |
| IsSignificantArea | 1 | 1 | ⚠️ | 85-87 |
| IsSignificantVolume | 1 | 1 | ⚠️ | 90-92 |
| RoundToTolerance | 2 | 1 | ⚠️ | 95-98 |
| ToleranceContext | 1 | 1 | ⚠️ | 104-107 |
| Dispose | 0 | 1 | ⚠️ | 109-111 |
| CreateContext | 1 | 1 | ⚠️ | 115-117 |
| CreateRobustContext | 1 | 1 | ⚠️ | 120-136 |
| ValidateSettings | 2 | 2 | ⚠️ | 139-157 |
| GetDescription | 0 | 1 | ⚠️ | 160-165 |

## Issues and Risks

* **P2** – Documentation: src/AssemblyChain.Core/Contact/ContactGraphBuilder.cs has 3/4 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Contact/ContactModel.cs has 5/15 undocumented methods
* **P1** – LargeFile: src/AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs is very large (771 LOC, 16 methods)
* **P2** – Documentation: src/AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs has 4/16 undocumented methods
* **P0** – HighComplexity: src/AssemblyChain.Core/Contact/Detection/NarrowPhase/MeshContactDetector.cs:DetectContactsWithIntersectionLines has cyclomatic complexity ~12
* **P2** – Documentation: src/AssemblyChain.Core/Contact/Detection/NarrowPhase/NarrowPhaseDetection.cs has 1/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/Common/Entity.cs has 3/3 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/Common/ValueObject.cs has 2/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/Entities/Assembly.cs has 1/12 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/Entities/Joint.cs has 1/5 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/Services/DomainServices.cs has 11/16 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/ValueObjects/MaterialProperties.cs has 2/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/ValueObjects/PartGeometry.cs has 1/3 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Domain/ValueObjects/PhysicsProperties.cs has 2/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Graph/ConstraintGraphBuilder.cs has 3/4 undocumented methods
* **P1** – LargeFile: src/AssemblyChain.Core/Graph/GNNAnalyzer.cs is very large (583 LOC, 14 methods)
* **P2** – Documentation: src/AssemblyChain.Core/Graph/GNNAnalyzer.cs has 2/14 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Graph/GraphViews.cs has 5/5 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Model/AssemblyModel.cs has 1/1 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Model/AssemblyModelFactory.cs has 1/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Model/ConstraintModel.cs has 6/6 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Model/GraphModel.cs has 7/7 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Model/MotionModel.cs has 8/8 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Model/SolverModel.cs has 6/6 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Motion/MotionEvaluator.cs has 5/6 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Motion/PoseEstimator.cs has 2/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Motion/PoseTypes.cs has 1/1 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Solver/CSPSolver.cs has 1/1 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Solver/MILPSolver.cs has 1/1 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Solver/SATSolver.cs has 1/1 undocumented methods
* **P1** – LargeFile: src/AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs is very large (496 LOC, 11 methods)
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs has 5/11 undocumented methods
* **P0** – HighComplexity: src/AssemblyChain.Core/Toolkit/Brep/PlanarOps.cs:DetectCoplanarContacts has cyclomatic complexity ~18
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Math/Clustering.cs has 12/12 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Math/ConvexCone.cs has 22/22 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Mesh/MeshPreprocessor.cs has 2/5 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Mesh/MeshSpatialIndex.cs has 1/12 undocumented methods
* **P1** – LargeFile: src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs is very large (495 LOC, 14 methods)
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshOptimizer.cs has 10/14 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshRepair.cs has 8/9 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Mesh/Preprocessing/MeshValidator.cs has 6/9 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Utils/CacheManager.cs has 2/12 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Utils/ExtremeRayExtractor.cs has 1/2 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Utils/Hashing.cs has 7/10 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Utils/JsonSerializer.cs has 18/18 undocumented methods
* **P2** – Documentation: src/AssemblyChain.Core/Toolkit/Utils/Tolerance.cs has 16/16 undocumented methods
* **P1** – Duplication: Detected 138 duplicated code fragments
* **P1** – Coupling: Namespace dependency graph shows high fan-out; consider modular boundaries.

## Duplicate Fragments

```csharp
        {
            public List<(int i, int j)> CandidatePairs { get; set; } = new List<(int, int)>();
            public TimeSpan ExecutionTime { get; set; }
            public int TotalPairs { get; set; }
            public double ReductionRatio { get; set; }
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 26
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 86

```csharp
                    }
                }
                result.CandidatePairs = candidatePairs.ToList();
                result.TotalPairs = boundingBoxes.Count * (boundingBoxes.Count - 1) / 2;
                result.ReductionRatio = result.TotalPairs > 0 ?
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 68
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 133

```csharp
                }
                result.CandidatePairs = candidatePairs.ToList();
                result.TotalPairs = boundingBoxes.Count * (boundingBoxes.Count - 1) / 2;
                result.ReductionRatio = result.TotalPairs > 0 ?
                    (double)result.CandidatePairs.Count / result.TotalPairs : 0.0;
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 69
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 134

```csharp
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 75
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 139
  * src/AssemblyChain.Core/Toolkit/Intersection/BrepBrepIntersect.cs @ line 70
  * src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs @ line 170
  * src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs @ line 288

```csharp
                result.ExecutionTime = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 76
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 140
  * src/AssemblyChain.Core/Toolkit/Intersection/BrepBrepIntersect.cs @ line 71
  * src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs @ line 171
  * src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs @ line 289

```csharp
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 77
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 141
  * src/AssemblyChain.Core/Toolkit/Intersection/BrepBrepIntersect.cs @ line 72
  * src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs @ line 172
  * src/AssemblyChain.Core/Toolkit/Intersection/MeshMeshIntersect.cs @ line 290

```csharp
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                // Log error but don't throw - return empty result
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 78
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 142

```csharp
        {
            var boundingBoxes = meshes.Select(m => m?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }
        /// <summary>
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 127
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 204

```csharp
        {
            var boundingBoxes = breps.Select(b => b?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }
        /// <summary>
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 135
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 212

```csharp
        {
            var boundingBoxes = geometries.Select(g => g?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }
        /// <summary>
```
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/RTreeBroadPhase.cs @ line 143
  * src/AssemblyChain.Core/Contact/Detection/BroadPhase/SweepAndPrune.cs @ line 220

## Recommendations

### Architecture

* Consolidate namespace dependencies to reduce fan-out; enforce clear boundaries between Domain, Graph and Motion layers.
* Introduce interfaces or abstractions where namespaces depend on each other cyclically to break potential cycles.

### Module-Level

* Split oversized files into focused components aligning with single responsibility.
* Share reusable math/helpers via Toolkit namespace to avoid duplication.

### Function-Level

* Refactor high-complexity methods (>10) using guard clauses or extracting helpers.
* Document public APIs with XML summaries to improve discoverability and maintainability.

### Engineering

* Integrate this audit in CI to track metric drift and enforce quality gates.
* Add unit tests for hotspots before refactoring to preserve behaviour.

## Roadmap

1. **Phase 1 – Visibility**: Run audit in CI, baseline metrics, triage P0/P1 issues.
2. **Phase 2 – Stabilise**: Address high-complexity methods and eliminate duplication hotspots.
3. **Phase 3 – Optimise**: Revisit architecture boundaries and implement caching/performance enhancements informed by profiling.