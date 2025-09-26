using AssemblyChain.Constraints;
using AssemblyChain.GH.Data;
using AssemblyChain.Geometry.ContactDetection;
#if !GRASSHOPPER
using AssemblyChain.GH.Stubs;
#endif
using AssemblyChain.Graphs;

namespace AssemblyChain.GH.Components;

/// <summary>
/// Builds the non-directional blocking graph from a set of contacts.
/// </summary>
public sealed class BuildNdbgComponent : AssemblyChainComponentBase
{
    public BuildNdbgComponent()
        : base("Build NDBG", "NDBG", "Build non-directional blocking graph", "AssemblyChain", "Graphs")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        var contactsWrapper = dataAccess.GetInput<GhContacts>(1);
        if (assemblyWrapper is null || contactsWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhGraph));
            return;
        }

        var cones = new DirectionConeBuilder().BuildCones(assemblyWrapper.Value, contactsWrapper.Value);
        var graph = new NonDirectionalBlockingGraphBuilder().Build(assemblyWrapper.Value, cones);
        dataAccess.SetOutput(0, new GhGraph(graph));
        dataAccess.SetOutput(1, new GhDirectionCones(cones));
    }
}
