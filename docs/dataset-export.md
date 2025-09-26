# Dataset Export Guide

The `DatasetExporter` writes JSON artifacts summarising the assembly, contact graph and solver outcome. Use the facade helper:

```csharp
var facade = new AssemblyChainFacade();
var dataset = facade.ExportDataset(assemblyModel, contactModel, solverResult);
```

Generated files include metadata fields:
- `assemblyName`, `partCount`, `contactCount`
- `solverSummary` and raw `solverMetadata`
- Optional `geometry` array when `IncludeGeometry` is enabled

Output directory defaults to `dataset` and can be configured via `DatasetExportOptions`.
