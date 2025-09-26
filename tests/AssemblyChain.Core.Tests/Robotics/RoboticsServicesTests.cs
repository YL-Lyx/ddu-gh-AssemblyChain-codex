using System.Collections.Generic;
using System.Threading.Tasks;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;
using AssemblyChain.Robotics;
using FluentAssertions;
using Xunit;

namespace AssemblyChain.Core.Tests.Robotics;

public class RoboticsServicesTests
{
    [Fact]
    public void UrScriptExporterWritesPoseWhenAvailable()
    {
        var plan = new AssemblyPlan(
            "plan",
            new List<PlanStep>
            {
                new(0, "Place", "base", new Pose(new Point3d(1, 2, 3), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1))),
            },
            true);

        var exporter = new UrScriptExporter();
        var script = exporter.Export(plan);
        script.Should().Contain("movej");
    }

    [Fact]
    public async Task DummyExecutorCapturesStepsAsync()
    {
        var plan = new AssemblyPlan("plan", new List<PlanStep> { new(0, "Place", "a") }, true);
        var executor = new DummyRobotExecutor();
        await executor.ExecuteAsync(plan);
        executor.ExecutedSteps.Should().ContainSingle();
    }
}
