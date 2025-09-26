using System;

namespace AssemblyChain.Graphs
{

/// <summary>
/// Options for graph construction.
/// </summary>
public readonly record struct GraphOptions(
    bool UseDirected = true,
    bool OnlyBlocking = true,
    string GraphType = "DBG"
);
}
