# AssemblyChain Developer Documentation

Welcome to the AssemblyChain developer documentation set. The generated API reference is built from the XML documentation that ships with the core libraries and plug-ins. Use the navigation pane to browse the available namespaces or jump directly to the [API reference](api/index.md).

## Building the documentation locally

1. Install [DocFX](https://dotnet.github.io/docfx/) `v2.75` or later. The recommended approach is to use the global `dotnet tool`:
   ```bash
   dotnet tool update -g docfx
   ```
2. Restore the solution dependencies:
   ```bash
   dotnet restore AssemblyChain-Core.sln
   ```
3. Generate the API metadata and site output:
   ```bash
   docfx docs/docfx.json
   ```
4. Open `_site/index.html` to explore the generated documentation.

The build uses the XML documentation files emitted during compilation. Ensure you build the solution in `Release` configuration before invoking DocFX so that the latest comments are included.

## Documentation coverage

The documentation build treats missing XML comments as warnings. With the new summaries and parameter descriptions in place the coverage exceeds the 30% target for CI documentation checks.
