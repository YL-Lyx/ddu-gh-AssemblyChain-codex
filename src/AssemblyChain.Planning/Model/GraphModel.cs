using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Planning.Model
{
    /// <summary>
    /// Read-only graph model.
    /// Contains blocking graphs, strongly connected components, and version hash for caching.
    /// </summary>
    public sealed class GraphModel
    {
        public BlockingGraph DirectionalBlockingGraph { get; }
        public NonDirectionalBlockingGraph NonDirectionalBlockingGraph { get; }
        public IReadOnlyDictionary<int, int> InDegrees { get; }
        public IReadOnlyList<StronglyConnectedComponent> StronglyConnectedComponents { get; }
        public IReadOnlyList<BlockingEdge> AllBlockingEdges { get; }
        public string Hash { get; }

        public int NodeCount => DirectionalBlockingGraph.Nodes.Count;
        public int EdgeCount => DirectionalBlockingGraph.Edges.Count;
        public int ComponentCount => StronglyConnectedComponents.Count;

        internal GraphModel(
            BlockingGraph directionalBlockingGraph,
            NonDirectionalBlockingGraph nonDirectionalBlockingGraph,
            IReadOnlyDictionary<int, int> inDegrees,
            IReadOnlyList<StronglyConnectedComponent> stronglyConnectedComponents,
            IReadOnlyList<BlockingEdge> allBlockingEdges,
            string hash)
        {
            DirectionalBlockingGraph = directionalBlockingGraph ?? throw new ArgumentNullException(nameof(directionalBlockingGraph));
            NonDirectionalBlockingGraph = nonDirectionalBlockingGraph ?? throw new ArgumentNullException(nameof(nonDirectionalBlockingGraph));
            InDegrees = inDegrees ?? throw new ArgumentNullException(nameof(inDegrees));
            StronglyConnectedComponents = stronglyConnectedComponents ?? throw new ArgumentNullException(nameof(stronglyConnectedComponents));
            AllBlockingEdges = allBlockingEdges ?? throw new ArgumentNullException(nameof(allBlockingEdges));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public int GetInDegree(int nodeIndex)
        {
            return InDegrees.TryGetValue(nodeIndex, out var degree) ? degree : 0;
        }

        public IEnumerable<int> GetFreeParts()
        {
            return InDegrees.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key);
        }

        public StronglyConnectedComponent GetComponentForNode(int nodeIndex)
        {
            return StronglyConnectedComponents.FirstOrDefault(scc => scc.Members.Contains(nodeIndex));
        }

        public bool AreInSameComponent(int nodeA, int nodeB)
        {
            var componentA = GetComponentForNode(nodeA);
            var componentB = GetComponentForNode(nodeB);
            return componentA != null && componentB != null && componentA.ComponentId == componentB.ComponentId;
        }
    }

    // Minimal placeholder types to compile; replace with real ones if available
    public class BlockingGraph { public List<int> Nodes { get; } = new(); public List<BlockingEdge> Edges { get; } = new(); }
    public class NonDirectionalBlockingGraph { }
    public class BlockingEdge { public int FromId { get; set; } public int ToId { get; set; } public int From => FromId; public int To => ToId; }
    public class StronglyConnectedComponent 
    { 
        public int ComponentId { get; set; } 
        public HashSet<int> Members { get; } = new(); 
        public bool HasExternalOutgoing { get; set; }
        public StronglyConnectedComponent() {}
        public StronglyConnectedComponent(int ComponentId, IEnumerable<int> Members, bool HasExternalOutgoing)
        {
            this.ComponentId = ComponentId;
            if (Members != null) this.Members = new HashSet<int>(Members);
            this.HasExternalOutgoing = HasExternalOutgoing;
        }
    }
}

