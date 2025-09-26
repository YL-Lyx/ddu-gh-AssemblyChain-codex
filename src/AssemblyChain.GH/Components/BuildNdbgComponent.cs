#if GRASSHOPPER
using GH_ComponentBase = Grasshopper.Kernel.GH_Component;
#else
using GH_ComponentBase = AssemblyChain.GH.Stubs.GhComponentBase;
#endif

namespace AssemblyChain.GH.Components;

/// <summary>
/// Grasshopper component stub that will build a non-directional blocking graph once the core graph service lands.
/// </summary>
public sealed class BuildNdbgComponent : GH_ComponentBase
{
    public BuildNdbgComponent()
        : base("Build NDBG", "NDBG", "Build non-directional blocking graph", "AssemblyChain", "Graphs")
    {
    }

    /// <summary>
    /// Placeholder execution logic that documents the intended dependency on the core graph builder.
    /// </summary>
    public override void SolveInstance()
    {
#if GRASSHOPPER
        // TODO: Wire up IGraphBuilder from AssemblyChain.Graphs once implemented.
        throw new System.NotImplementedException("Graph builder integration pending.");
#else
        base.SolveInstance();
#endif
    }
}
