using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Planning.Model;
using AssemblyChain.Graphs;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Geometry.Contact
{
    /// <summary>
    /// Builds blocking graphs from contact information.
    /// </summary>
    public static class ContactGraphBuilder
    {
        /// <summary>
        /// Builds a GraphModel from contact data.
        /// </summary>
        public static GraphModel BuildGraph(
            ContactModel contacts,
            GraphOptions options)
        {
            var contactRelations = contacts.Relations;

            // Build minimal placeholder graphs from contacts
            var nodeSet = new HashSet<int>();
            foreach (var c in contacts.Contacts)
            {
                if (TryParsePartIndex(c.PartAId, out int a)) nodeSet.Add(a);
                if (TryParsePartIndex(c.PartBId, out int b)) nodeSet.Add(b);
            }

            var directionalGraph = new BlockingGraph();
            directionalGraph.Nodes.AddRange(nodeSet);
            var nonDirectionalGraph = new NonDirectionalBlockingGraph();

            var inDegrees = CalculateInDegrees(directionalGraph);
            var sccs = FindStronglyConnectedComponents(directionalGraph);
            var allBlockingEdges = directionalGraph.Edges.ToList();

            var hash = $"graph_{contacts.Hash}_{options.UseDirected}_{options.OnlyBlocking}";

            return new GraphModel(
                directionalGraph,
                nonDirectionalGraph,
                inDegrees,
                sccs,
                allBlockingEdges,
                hash
            );
        }


        private static bool TryParsePartIndex(string partId, out int index)
        {
            if (partId != null && partId.StartsWith("P") && int.TryParse(partId.Substring(1), out index))
            {
                return true;
            }
            index = -1;
            return false;
        }

        private static IReadOnlyDictionary<int, int> CalculateInDegrees(BlockingGraph graph)
        {
            var inDegrees = new Dictionary<int, int>();
            foreach (var node in graph.Nodes) inDegrees[node] = 0;
            foreach (var edge in graph.Edges)
            {
                if (inDegrees.ContainsKey(edge.To))
                {
                    inDegrees[edge.To]++;
                }
            }
            return inDegrees;
        }

        private static IReadOnlyList<StronglyConnectedComponent> FindStronglyConnectedComponents(BlockingGraph graph)
        {
            var components = new List<StronglyConnectedComponent>();
            foreach (var node in graph.Nodes)
            {
                var scc = new StronglyConnectedComponent(
                    ComponentId: node,
                    Members: new[] { node },
                    HasExternalOutgoing: graph.Edges.Any(e => e.From == node)
                );
                components.Add(scc);
            }
            return components;
        }
    }
}



