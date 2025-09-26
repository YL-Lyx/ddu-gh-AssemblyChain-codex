using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AssemblyChain.Constraints;
using AssemblyChain.Core.DomainModel;

namespace AssemblyChain.Graphs;

/// <summary>
/// Builds a simple undirected adjacency graph from joints and detected contacts.
/// </summary>
public sealed class AdjacencyGraphBuilder
{
    public Graph Build(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        var adjacency = assembly.Parts.ToImmutableDictionary(p => p.Id, _ => new HashSet<string>());
        foreach (var joint in assembly.Joints)
        {
            adjacency[joint.PartA].Add(joint.PartB);
            adjacency[joint.PartB].Add(joint.PartA);
        }

        return new Graph(adjacency.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<string>)kvp.Value.ToImmutableHashSet()));
    }
}

/// <summary>
/// Builds the non-directional blocking graph (NDBG) by converting direction cones into undirected blocking relationships.
/// </summary>
public sealed class NonDirectionalBlockingGraphBuilder
{
    public Graph Build(Assembly assembly, IEnumerable<DirectionCone> cones)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(cones);
        var adjacency = assembly.Parts.ToImmutableDictionary(p => p.Id, _ => new HashSet<string>());
        foreach (var cone in cones)
        {
            adjacency[cone.PartA].Add(cone.PartB);
            adjacency[cone.PartB].Add(cone.PartA);
        }

        return new Graph(adjacency.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<string>)kvp.Value.ToImmutableHashSet()));
    }
}

/// <summary>
/// Converts a non-directional blocking graph into a directed blocking graph by orienting edges using the cone axes.
/// </summary>
public sealed class DirectedBlockingGraphBuilder
{
    public DirectedGraph Build(IEnumerable<DirectionCone> cones)
    {
        ArgumentNullException.ThrowIfNull(cones);
        var adjacency = new Dictionary<string, HashSet<string>>();
        foreach (var cone in cones)
        {
            if (!adjacency.TryGetValue(cone.PartA, out var listA))
            {
                listA = new HashSet<string>();
                adjacency[cone.PartA] = listA;
            }

            listA.Add(cone.PartB);
        }

        return new DirectedGraph(adjacency.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<string>)kvp.Value.ToImmutableHashSet()));
    }
}

/// <summary>
/// Lightweight immutable adjacency graph representation.
/// </summary>
public sealed record Graph(IReadOnlyDictionary<string, IReadOnlyCollection<string>> Adjacency)
{
    public IReadOnlyCollection<string> Neighbours(string node) => Adjacency.TryGetValue(node, out var neighbours)
        ? neighbours
        : Array.Empty<string>();
}

/// <summary>
/// Lightweight immutable directed graph representation.
/// </summary>
public sealed record DirectedGraph(IReadOnlyDictionary<string, IReadOnlyCollection<string>> Adjacency)
{
    public IReadOnlyCollection<string> Successors(string node) => Adjacency.TryGetValue(node, out var neighbours)
        ? neighbours
        : Array.Empty<string>();
}
