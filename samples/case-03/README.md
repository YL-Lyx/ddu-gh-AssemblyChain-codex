# Case03 – Facade to Process Sample

This sample demonstrates an end-to-end pipeline using the `AssemblyChainFacade` to:

1. Generate a minimal three-block assembly.
2. Solve the disassembly order via the CSP backend.
3. Export the result to the robotics `process.json` format.
4. Validate the output against `process.schema.json`.

## Prerequisites
- .NET 7 SDK.
- RhinoCommon dependency (restored transitively via the core project).

## Usage
```bash
cd samples/case-03
dotnet run
```

The app writes `output/process.json` and reports schema-validation status. Feel free to inspect the JSON or load it with the Python preview script in `docs/RobotBridge.md`.

## Sample Output Structure
```json
{
  "schemaVersion": "1.0.0",
  "header": {
    "generator": "AssemblyChain",
    "author": "Case03 Sample",
    "description": "Three-block disassembly via AssemblyChain facade",
    "createdUtc": "2025-01-01T00:00:00.0000000Z"
  },
  "steps": [
    {
      "index": 0,
      "partId": "Block_0",
      "direction": [0.0, 0.0, 1.0],
      "batch": 0,
      "insert": false
    }
  ],
  "metadata": {
    "solver": "CSP",
    "feasible": true,
    "optimal": true
  }
}
```

> Actual runs include three steps (one per block) and real timestamps; the snippet above highlights the shape expected by downstream tools.
