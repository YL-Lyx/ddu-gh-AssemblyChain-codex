using System.Collections.Generic;
using AssemblyChain.Analysis;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.GH.Data;
#if !GRASSHOPPER
using AssemblyChain.GH.Stubs;
#endif
using AssemblyChain.Planning;

namespace AssemblyChain.GH.Components;

public sealed class TreeSearchPlanningComponent : AssemblyChainComponentBase
{
    public TreeSearchPlanningComponent()
        : base("Plan (Tree Search)", "PlanTree", "Compute an assembly plan using tree search", "AssemblyChain", "Planning")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        var graphWrapper = dataAccess.GetInput<GhGraph>(1);
        var grounded = dataAccess.GetInput<IEnumerable<string>>(2) ?? new[] { "base" };
        if (assemblyWrapper is null || graphWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhPlan));
            return;
        }

        var solver = new TreeSearchSolver(new StabilityAnalyzer());
        var plan = solver.Solve(assemblyWrapper.Value, graphWrapper.Value, grounded);
        dataAccess.SetOutput(0, new GhPlan(plan));
    }
}

public sealed class SatPlanningComponent : AssemblyChainComponentBase
{
    public SatPlanningComponent()
        : base("Plan (SAT)", "PlanSat", "Placeholder SAT-based planner", "AssemblyChain", "Planning")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        var graphWrapper = dataAccess.GetInput<GhGraph>(1);
        if (assemblyWrapper is null || graphWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhPlan));
            return;
        }

        // Reuse the tree search solver as a stand-in to keep the UI functional.
        var solver = new TreeSearchSolver(new StabilityAnalyzer());
        var plan = solver.Solve(assemblyWrapper.Value, graphWrapper.Value, new[] { assemblyWrapper.Value.Parts[0].Id });
        dataAccess.SetOutput(0, new GhPlan(plan));
    }
}

public sealed class SamplingPlanningComponent : AssemblyChainComponentBase
{
    public SamplingPlanningComponent()
        : base("Plan (Sampling)", "PlanSample", "Placeholder sampling-based planner", "AssemblyChain", "Planning")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        if (assemblyWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhPlan));
            return;
        }

        var steps = new List<PlanStep>();
        int index = 0;
        foreach (var part in assemblyWrapper.Value.Parts)
        {
            steps.Add(new PlanStep(index++, "Place", part.Id));
        }

        var plan = new AssemblyPlan("Sampling", steps, true);
        dataAccess.SetOutput(0, new GhPlan(plan));
    }
}
