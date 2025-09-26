using System.Collections.Generic;
using AssemblyChain.Analysis;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Gh.Kernel.Legacy;
using AssemblyChain.Planning;

namespace AssemblyChain.Gh.Components.Legacy;

public sealed class StabilityCheckComponent : AssemblyChainComponentBase
{
    public StabilityCheckComponent()
        : base("Stability Check", "Stability", "Compute stability margin", "AssemblyChain", "Validation")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        var grounded = dataAccess.GetInput<IEnumerable<string>>(1) ?? new[] { "base" };
        if (assemblyWrapper is null)
        {
            dataAccess.SetOutput(0, 0.0);
            return;
        }

        var analyzer = new StabilityAnalyzer();
        var result = analyzer.Compute(assemblyWrapper.Value, grounded);
        dataAccess.SetOutput(0, result.Margin);
    }
}

public sealed class PathFeasibilityComponent : AssemblyChainComponentBase
{
    public PathFeasibilityComponent()
        : base("Path Feasibility", "Feasible", "Validate path feasibility", "AssemblyChain", "Validation")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var part = dataAccess.GetInput<Part>(0);
        var placed = dataAccess.GetInput<IEnumerable<Part>>(1) ?? System.Array.Empty<Part>();
        if (part is null)
        {
            dataAccess.SetOutput(0, false);
            return;
        }

        var checker = new PathFeasibilityChecker();
        var result = checker.IsFeasible(part, placed);
        dataAccess.SetOutput(0, result);
    }
}
