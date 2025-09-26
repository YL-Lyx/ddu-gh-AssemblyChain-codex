using System.Collections.Generic;
using AssemblyChain.GH.Components;
using AssemblyChain.GH.Data;
using AssemblyChain.GH.Stubs;
using AssemblyChain.IO;
using FluentAssertions;
using Xunit;

namespace AssemblyChain.Core.Tests.GhIntegration;

public class GhPipelineTests
{
    [Fact]
    public void PipelineRunsFromAssemblyToPlan()
    {
        var assemblyPath = System.IO.Path.Combine(TestContext.ProjectRoot, "samples", "assembly", "simple-assembly.json");
        var assembly = AssemblySerializer.LoadFromFile(assemblyPath);
        var assemblyWrapper = new GhAssembly(assembly);

        var contactComponent = new ContactDetectorComponent();
        var contactAccess = new GhDataAccess();
        contactAccess.SetInput(0, assemblyWrapper);
        contactComponent.SolveInstance(contactAccess);
        var contacts = contactAccess.GetOutput<GhContacts>(0);
        contacts.Should().NotBeNull();

        var coneComponent = new DirectionConeComponent();
        var coneAccess = new GhDataAccess();
        coneAccess.SetInput(0, assemblyWrapper);
        coneAccess.SetInput(1, contacts);
        coneComponent.SolveInstance(coneAccess);
        var cones = coneAccess.GetOutput<GhDirectionCones>(0);
        cones.Should().NotBeNull();

        var adjacencyComponent = new BuildAdjacencyComponent();
        var adjacencyAccess = new GhDataAccess();
        adjacencyAccess.SetInput(0, assemblyWrapper);
        adjacencyComponent.SolveInstance(adjacencyAccess);
        var adjacency = adjacencyAccess.GetOutput<GhGraph>(0);
        adjacency.Should().NotBeNull();

        var planner = new TreeSearchPlanningComponent();
        var planningAccess = new GhDataAccess();
        planningAccess.SetInput(0, assemblyWrapper);
        planningAccess.SetInput(1, adjacency);
        planningAccess.SetInput(2, new[] { "base" });
        planner.SolveInstance(planningAccess);
        var plan = planningAccess.GetOutput<GhPlan>(0);
        plan.Should().NotBeNull();
        plan!.Value.IsValid.Should().BeTrue();
    }
}

internal static class TestContext
{
    private static readonly string _projectRoot = LocateProjectRoot();

    public static string ProjectRoot => _projectRoot;

    private static string LocateProjectRoot()
    {
        var directory = System.IO.Directory.GetCurrentDirectory();
        while (!System.IO.File.Exists(System.IO.Path.Combine(directory, "AssemblyChain-Core.sln")))
        {
            directory = System.IO.Directory.GetParent(directory)?.FullName
                ?? throw new System.InvalidOperationException("Unable to locate project root");
        }

        return directory;
    }
}
