using System.Collections.Generic;
using AssemblyChain.Analysis;
using AssemblyChain.Constraints;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;
using AssemblyChain.Geometry.ContactDetection;
using AssemblyChain.Graphs;
using AssemblyChain.Planning;
using FluentAssertions;
using Xunit;

namespace AssemblyChain.Core.Tests.Planning;

public class TreeSearchSolverTests
{
    [Fact]
    public void SolveProducesFeasiblePlanForSimpleAssembly()
    {
        var basePart = new Part(
            "base",
            "Base",
            10,
            new Point3d(0, 0, 0),
            new List<GeometryPrimitive>
            {
                new("base-face", GeometryPrimitiveType.Face, new[]
                {
                    new Point3d(-0.5, -0.5, 0),
                    new Point3d(0.5, -0.5, 0),
                    new Point3d(0.5, 0.5, 0),
                    new Point3d(-0.5, 0.5, 0),
                }),
            });
        var topPart = new Part(
            "top",
            "Top",
            5,
            new Point3d(0, 0, 0.5),
            new List<GeometryPrimitive>
            {
                new("top-face", GeometryPrimitiveType.Face, new[]
                {
                    new Point3d(-0.4, -0.4, 0.5),
                    new Point3d(0.4, -0.4, 0.5),
                    new Point3d(0.4, 0.4, 0.5),
                    new Point3d(-0.4, 0.4, 0.5),
                }),
            });

        var assembly = new Assembly(
            "simple",
            new List<Part> { basePart, topPart },
            new List<Joint> { new("j1", "base", "top", "stack") });

        var contacts = new ContactDetector().DetectContacts(assembly);
        _ = new DirectionConeBuilder().BuildCones(assembly, contacts);
        var adjacency = new AdjacencyGraphBuilder().Build(assembly);
        var solver = new TreeSearchSolver(new StabilityAnalyzer());
        var plan = solver.Solve(assembly, adjacency, new[] { "base" });

        plan.IsValid.Should().BeTrue();
        plan.Steps.Should().HaveCount(1);
        plan.Steps[0].PartId.Should().Be("top");
    }
}
