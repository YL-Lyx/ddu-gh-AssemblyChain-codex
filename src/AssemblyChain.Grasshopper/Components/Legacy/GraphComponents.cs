using AssemblyChain.Gh.Kernel.Legacy;
using AssemblyChain.Graphs;

namespace AssemblyChain.Gh.Components.Legacy;

public sealed class BuildAdjacencyComponent : AssemblyChainComponentBase
{
    public BuildAdjacencyComponent()
        : base("Build Adjacency", "Adjacency", "Build adjacency graph", "AssemblyChain", "Graphs")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        if (assemblyWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhGraph));
            return;
        }

        var graph = new AdjacencyGraphBuilder().Build(assemblyWrapper.Value);
        dataAccess.SetOutput(0, new GhGraph(graph));
    }
}

public sealed class NdbgToDbgComponent : AssemblyChainComponentBase
{
    public NdbgToDbgComponent()
        : base("NDBG to DBG", "DBG", "Convert non-directional blocking graph to directed", "AssemblyChain", "Graphs")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var conesWrapper = dataAccess.GetInput<GhDirectionCones>(0);
        if (conesWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhDirectedGraph));
            return;
        }

        var directed = new DirectedBlockingGraphBuilder().Build(conesWrapper.Value);
        dataAccess.SetOutput(0, new GhDirectedGraph(directed));
    }
}
