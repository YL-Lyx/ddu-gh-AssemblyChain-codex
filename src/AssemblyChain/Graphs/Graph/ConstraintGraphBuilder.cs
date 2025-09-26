using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Planning.Model;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Graphs
{
    /// <summary>
    /// Builds constraint graphs by combining graph and motion information.
    /// </summary>
    public static class ConstraintGraphBuilder
    {
        /// <summary>
        /// Builds a ConstraintModel from graph and motion models.
        /// </summary>
        public static ConstraintModel BuildConstraints(
            GraphModel graph,
            MotionModel motion)
        {
            // Build part constraints
            var partConstraints = BuildPartConstraints(graph, motion);

            // Build group constraints
            var groupConstraints = BuildGroupConstraints(graph, motion);

            // Generate hash
            var hash = $"constraint_{graph.Hash}_{motion.Hash}";

            return new ConstraintModel(
                graph,
                motion,
                partConstraints,
                groupConstraints,
                hash
            );
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<string>> BuildPartConstraints(
            GraphModel graph, MotionModel motion)
        {
            var partConstraints = new Dictionary<int, List<string>>();

            // For each part, collect constraints from both graph and motion models
            var allParts = graph.DirectionalBlockingGraph.Nodes.Union(motion.PartMotionRays.Keys).Distinct();

            foreach (var partIndex in allParts)
            {
                var constraints = new List<string>();

                // Graph constraints
                var inDegree = graph.GetInDegree(partIndex);
                constraints.Add($"Blocked by {inDegree} incoming edge(s) in graph");

                // Motion constraints
                var motionRays = motion.GetPartMotionRays(partIndex);
                constraints.Add($"Constrained by {motionRays.Count} motion direction(s)");

                // Strongly connected component info
                var scc = graph.GetComponentForNode(partIndex);
                if (scc != null)
                {
                    constraints.Add($"Part of SCC {scc.ComponentId} with {scc.Members.Count} parts");
                }

                partConstraints[partIndex] = constraints;
            }

            return partConstraints.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<string>)kvp.Value
            );
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildGroupConstraints(
            GraphModel graph, MotionModel motion)
        {
            var groupConstraints = new Dictionary<string, List<string>>();

            // For each group, collect constraints
            foreach (var groupKey in motion.GetAllGroupKeys())
            {
                var constraints = new List<string>();

                // Parse group members
                var partIndices = MotionModel.ParseGroupKey(groupKey);

                // Check if all parts are in the same SCC
                var sccs = partIndices.Select(p => graph.GetComponentForNode(p)).Where(s => s != null).Distinct().ToList();
                if (sccs.Count == 1)
                {
                    var scc = sccs[0];
                    constraints.Add($"All parts in SCC {scc.ComponentId}");
                    if (scc.HasExternalOutgoing)
                    {
                        constraints.Add("SCC has external outgoing edges");
                    }
                }

                // Motion constraints for the group
                var motionRays = motion.GetGroupMotionRays(groupKey);
                constraints.Add($"Group constrained by {motionRays.Count} motion direction(s)");

                // Check external blocking for the group
                var externalBlocking = CheckExternalBlocking(partIndices, graph);
                if (externalBlocking.Count > 0)
                {
                    constraints.Add($"External blocking from parts: {string.Join(", ", externalBlocking)}");
                }

                groupConstraints[groupKey] = constraints;
            }

            return groupConstraints.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<string>)kvp.Value
            );
        }

        private static IReadOnlyList<int> CheckExternalBlocking(IEnumerable<int> groupParts, GraphModel graph)
        {
            var groupSet = new HashSet<int>(groupParts);
            var externalBlockers = new HashSet<int>();

            foreach (var edge in graph.DirectionalBlockingGraph.Edges)
            {
                if (groupSet.Contains(edge.To) && !groupSet.Contains(edge.From))
                {
                    externalBlockers.Add(edge.From);
                }
            }

            return externalBlockers.OrderBy(x => x).ToList();
        }
    }
}



