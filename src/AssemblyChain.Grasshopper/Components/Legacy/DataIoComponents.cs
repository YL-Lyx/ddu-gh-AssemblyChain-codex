using System.Collections.Generic;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;
using AssemblyChain.Gh.Kernel.Legacy;
using AssemblyChain.IO;

namespace AssemblyChain.Gh.Components.Legacy;

public sealed class ImportAssemblyComponent : AssemblyChainComponentBase
{
    public ImportAssemblyComponent()
        : base("Import Assembly", "ImportAsm", "Load an assembly from disk", "AssemblyChain", "Data & IO")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var path = dataAccess.GetInput<string>(0);
        if (string.IsNullOrWhiteSpace(path))
        {
            dataAccess.SetOutput(0, default(GhAssembly));
            return;
        }

        var assembly = AssemblySerializer.LoadFromFile(path);
        dataAccess.SetOutput(0, new GhAssembly(assembly));
    }
}

public sealed class ExportPlanComponent : AssemblyChainComponentBase
{
    public ExportPlanComponent()
        : base("Export Plan", "ExportPlan", "Write a plan.json file", "AssemblyChain", "Data & IO")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var planWrapper = dataAccess.GetInput<GhPlan>(0);
        var path = dataAccess.GetInput<string>(1);
        if (planWrapper is null || string.IsNullOrWhiteSpace(path))
        {
            dataAccess.SetOutput(0, false);
            return;
        }

        PlanSerializer.SaveToFile(path, planWrapper.Value);
        dataAccess.SetOutput(0, true);
    }
}

public sealed class MakePartComponent : AssemblyChainComponentBase
{
    public MakePartComponent()
        : base("Make Part", "Part", "Construct a simple part", "AssemblyChain", "Data & IO")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var id = dataAccess.GetInput<string>(0) ?? "part";
        var name = dataAccess.GetInput<string>(1) ?? id;
        var mass = dataAccess.GetInput<double>(2);
        if (mass <= 0)
        {
            mass = 1;
        }

        var center = dataAccess.GetInput<Point3d>(3);
        if (center == default)
        {
            center = new Point3d(0, 0, 0);
        }

        var primitive = new GeometryPrimitive(
            $"{id}-primitive",
            GeometryPrimitiveType.Point,
            new[] { center });
        var part = new Part(id, name, mass, center, new List<GeometryPrimitive> { primitive });
        dataAccess.SetOutput(0, part);
    }
}

public sealed class MakeJointComponent : AssemblyChainComponentBase
{
    public MakeJointComponent()
        : base("Make Joint", "Joint", "Construct a joint", "AssemblyChain", "Data & IO")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var id = dataAccess.GetInput<string>(0) ?? "joint";
        var a = dataAccess.GetInput<string>(1) ?? "a";
        var b = dataAccess.GetInput<string>(2) ?? "b";
        var type = dataAccess.GetInput<string>(3) ?? "generic";
        dataAccess.SetOutput(0, new Joint(id, a, b, type));
    }
}
