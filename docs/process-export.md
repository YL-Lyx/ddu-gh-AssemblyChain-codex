# Robotic Process Export

Use `AssemblyChainFacade.ExportProcess` to serialise solver results to the UR10-compatible JSON schema.

```csharp
var process = facade.ExportProcess(solverResult, new ProcessExportOptions
{
    OutputPath = "artifacts/process.json",
    Author = Environment.UserName,
    Description = "Smoke test export"
});
```

The resulting file adheres to `process.schema.json` and includes metadata such as solver identifier, feasibility flags and execution vectors.
