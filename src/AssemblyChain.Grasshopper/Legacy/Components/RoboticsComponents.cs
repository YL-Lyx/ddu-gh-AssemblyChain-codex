using AssemblyChain.GH.Data;
#if !GRASSHOPPER
using AssemblyChain.GH.Stubs;
#endif
using AssemblyChain.Robotics;

namespace AssemblyChain.GH.Components;

public sealed class UrScriptExportComponent : AssemblyChainComponentBase
{
    public UrScriptExportComponent()
        : base("URScript Export", "URScript", "Export URScript from a plan", "AssemblyChain", "Robotics")
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

        var exporter = new UrScriptExporter();
        var script = exporter.Export(planWrapper.Value);
        dataAccess.SetOutput(0, script);
    }
}

public sealed class Ur10LiveComponent : AssemblyChainComponentBase
{
    public Ur10LiveComponent()
        : base("UR10 Live", "UR10", "Simulated UR10 RTDE client", "AssemblyChain", "Robotics")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var planWrapper = dataAccess.GetInput<GhPlan>(0);
        if (planWrapper is null)
        {
            dataAccess.SetOutput(0, 0);
            return;
        }

        var client = new Ur10RtdeClient();
        client.SendProgramAsync(new UrScriptExporter().Export(planWrapper.Value)).Wait();
        dataAccess.SetOutput(0, client.SentPrograms.Count);
    }
}

public sealed class SchunkEghComponent : AssemblyChainComponentBase
{
    public SchunkEghComponent()
        : base("Schunk EGH", "EGH", "Simulate Schunk EGH control", "AssemblyChain", "Robotics")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var command = dataAccess.GetInput<string>(0);
        var client = new SchunkEghClient();
        if (!string.IsNullOrWhiteSpace(command))
        {
            client.SendCommandAsync(command).Wait();
        }

        dataAccess.SetOutput(0, client.Commands.Count);
    }
}
