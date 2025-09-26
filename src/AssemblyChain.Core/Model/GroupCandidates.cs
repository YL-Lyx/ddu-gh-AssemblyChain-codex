using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Utilities for generating and evaluating group candidates for joint removal.
    /// </summary>
    public static class GroupCandidates
    {
        /// <summary>
        /// Generates candidate groups for joint removal based on SCC analysis.
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<int>> GenerateCandidates(GraphModel graph, int maxGroupSize = 4)
        {
            var candidates = new List<IReadOnlyList<int>>();
            if (graph == null || maxGroupSize <= 0) return candidates;

            // Use SCCs as initial groups
            foreach (var scc in graph.StronglyConnectedComponents)
            {
                if (scc.Members.Count <= maxGroupSize)
                {
                    candidates.Add(scc.Members.ToArray());
                }
                else
                {
                    // For large SCCs, generate smaller subgroups
                    var subgroups = GenerateSubgroups(scc.Members.ToList(), maxGroupSize);
                    candidates.AddRange(subgroups);
                }
            }

            // Also consider individual parts with zero in-degree
            var freeParts = graph.GetFreeParts().ToList();
            foreach (var part in freeParts)
            {
                candidates.Add(new[] { part });
            }

            return candidates;
        }

        /// <summary>
        /// Generates subgroups of size 2..maxSize from members.
        /// </summary>
        private static IEnumerable<IReadOnlyList<int>> GenerateSubgroups(IReadOnlyList<int> members, int maxSize)
        {
            for (int size = System.Math.Min(maxSize, members.Count); size >= 2; size--)
            {
                foreach (var combination in GenerateCombinations(members, size))
                {
                    yield return combination;
                }
            }
        }

        /// <summary>
        /// Generates all combinations of a given size from a set.
        /// </summary>
        private static IEnumerable<IReadOnlyList<int>> GenerateCombinations(IReadOnlyList<int> items, int size)
        {
            if (size <= 0 || items.Count < size) yield break;
            var indices = new int[size];
            for (int i = 0; i < size; i++) indices[i] = i;

            while (true)
            {
                yield return indices.Select(i => items[i]).ToArray();

                int j = size - 1;
                while (j >= 0 && indices[j] == items.Count - size + j) j--;
                if (j < 0) yield break;
                indices[j]++;
                for (int k = j + 1; k < size; k++) indices[k] = indices[k - 1] + 1;
            }
        }

        /// <summary>
        /// Evaluates the quality of a group candidate (heuristic).
        /// </summary>
        public static double EvaluateCandidate(IReadOnlyList<int> group, GraphModel graph)
        {
            if (group == null || graph == null || group.Count == 0) return 0;
            double internalConnections = 0;
            double externalConnections = 0;
            var groupSet = new HashSet<int>(group);

            foreach (var edge in graph.AllBlockingEdges)
            {
                bool fromIn = groupSet.Contains(edge.FromId);
                bool toIn = groupSet.Contains(edge.ToId);
                if (fromIn && toIn) internalConnections++;
                else if (fromIn || toIn) externalConnections++;
            }

            return internalConnections / System.Math.Max(1, externalConnections + 1);
        }

        /// <summary>
        /// Ranks candidates by evaluation score.
        /// </summary>
        public static IReadOnlyList<(IReadOnlyList<int> Group, double Score)> RankCandidates(
            IReadOnlyList<IReadOnlyList<int>> candidates, GraphModel graph)
        {
            if (candidates == null || graph == null) return Array.Empty<(IReadOnlyList<int>, double)>();
            return candidates
                .Select(group => (Group: group, Score: EvaluateCandidate(group, graph)))
                .OrderByDescending(x => x.Score)
                .ToList();
        }
    }
}



