using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Gh.Kernel.Legacy;
using AssemblyChain.Planning;

namespace AssemblyChain.Gh.Components.Legacy;

public sealed class AnimatePlanComponent : AssemblyChainComponentBase
{
    public AnimatePlanComponent()
        : base("Animate Plan", "Animate", "Produce a list of poses for animation", "AssemblyChain", "Simulation")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var planWrapper = dataAccess.GetInput<GhPlan>(0);
        if (planWrapper is null)
        {
            dataAccess.SetOutput(0, System.Array.Empty<PlanStep>());
            return;
        }

        dataAccess.SetOutput(0, planWrapper.Value.Steps);
    }
}

public sealed class SequenceDiagramComponent : AssemblyChainComponentBase
{
    public SequenceDiagramComponent()
        : base("Sequence Diagram", "Seq", "Produce a textual representation of the plan", "AssemblyChain", "Simulation")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var planWrapper = dataAccess.GetInput<GhPlan>(0);
        if (planWrapper is null)
        {
            dataAccess.SetOutput(0, string.Empty);
            return;
        }

        var lines = planWrapper.Value.Steps.Select(step => $"{step.Index}: {step.Action} {step.PartId}");
        dataAccess.SetOutput(0, string.Join("\n", lines));
    }
}
