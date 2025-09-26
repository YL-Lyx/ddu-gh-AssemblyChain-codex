using System.Collections.Generic;
using System.IO;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;
using AssemblyChain.IO;
using FluentAssertions;
using Xunit;

namespace AssemblyChain.Core.Tests.DomainModel;

public class AssemblySerializationTests
{
    [Fact]
    public void RoundTripAssemblyJson()
    {
        var assembly = new Assembly(
            "demo",
            new List<Part>
            {
                new("a", "PartA", 1.0, new Point3d(0, 0, 0), new List<GeometryPrimitive>
                {
                    new("prim", GeometryPrimitiveType.Point, new[] { new Point3d(0, 0, 0) })
                }),
            },
            new List<Joint>());

        using var stream = new MemoryStream();
        AssemblySerializer.Save(stream, assembly);
        stream.Position = 0;
        var loaded = AssemblySerializer.Load(stream);
        loaded.Id.Should().Be("demo");
        loaded.Parts.Should().HaveCount(1);
        loaded.Parts[0].Geometry.Should().HaveCount(1);
    }

    [Fact]
    public void RoundTripPlanJson()
    {
        var plan = new AssemblyPlan("plan", new List<PlanStep> { new(0, "Place", "a") }, true);
        using var stream = new MemoryStream();
        PlanSerializer.Save(stream, plan);
        stream.Position = 0;
        var loaded = PlanSerializer.Load(stream);
        loaded.Name.Should().Be("plan");
        loaded.Steps.Should().ContainSingle();
    }
}
