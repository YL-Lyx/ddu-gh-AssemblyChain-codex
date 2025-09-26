using System;
using System.Collections.Generic;
using AssemblyChain.Constraints;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Geometry.ContactDetection;
using AssemblyChain.Graphs;
using AssemblyChain.Planning;

namespace AssemblyChain.Gh.Kernel.Legacy;

public sealed class GhAssembly
{
    public GhAssembly(Assembly value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Assembly Value { get; }
}

public sealed class GhPlan
{
    public GhPlan(AssemblyPlan value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public AssemblyPlan Value { get; }
}

public sealed class GhContacts
{
    public GhContacts(IReadOnlyList<Contact> contacts)
    {
        Value = contacts ?? throw new ArgumentNullException(nameof(contacts));
    }

    public IReadOnlyList<Contact> Value { get; }
}

public sealed class GhDirectionCones
{
    public GhDirectionCones(IReadOnlyList<DirectionCone> cones)
    {
        Value = cones ?? throw new ArgumentNullException(nameof(cones));
    }

    public IReadOnlyList<DirectionCone> Value { get; }
}

public sealed class GhGraph
{
    public GhGraph(Graph graph)
    {
        Value = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public Graph Value { get; }
}

public sealed class GhDirectedGraph
{
    public GhDirectedGraph(DirectedGraph graph)
    {
        Value = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public DirectedGraph Value { get; }
}
