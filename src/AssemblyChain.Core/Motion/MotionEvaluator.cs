using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Contact;
using Rhino.Geometry;

namespace AssemblyChain.Core.Motion
{
    /// <summary>
    /// Evaluates motion feasibility for parts and groups.
    /// </summary>
    public static class MotionEvaluator
    {
        /// <summary>
        /// Builds motion constraints from contact information.
        /// </summary>
        public static MotionModel EvaluateMotion(
            ContactModel contacts,
            MotionOptions options)
        {
            var partMotionRays = ComputePartMotionRays(contacts, options);
            var groupMotionRays = ComputeGroupMotionRays(contacts, options);
            var hash = $"motion_{contacts.Hash}_{options.AngleTolDeg}_{options.FeasTol}";
            return new MotionModel(partMotionRays, groupMotionRays, hash);
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<Vector3d>> ComputePartMotionRays(
            ContactModel contacts, MotionOptions options)
        {
            var partRays = new Dictionary<int, IReadOnlyList<Vector3d>>();
            var partIndices = contacts.NeighborMap.Keys.Union(
                contacts.NeighborMap.Values.SelectMany(set => set)).Distinct().ToList();

            foreach (var partIndex in partIndices)
            {
                var rays = ComputeMotionRaysForPart(partIndex, contacts, options);
                partRays[partIndex] = rays;
            }

            return partRays;
        }

        private static IReadOnlyList<Vector3d> ComputeMotionRaysForPart(
            int partIndex, ContactModel contacts, MotionOptions options)
        {
            var constraintNormals = new List<Vector3d>();

            foreach (var contact in contacts.GetContactsForPart(partIndex))
            {
                // Simplified: assume a ContactData has a constraint direction stored as a vector if available
                // Placeholder: skip if such info is absent; in practice map from contact data to normals
            }

            if (constraintNormals.Count == 0)
                return Array.Empty<Vector3d>();

            return ConeIntersection.ComputeExtremeRays(constraintNormals, options);
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<Vector3d>> ComputeGroupMotionRays(
            ContactModel contacts, MotionOptions options)
        {
            var groupRays = new Dictionary<string, IReadOnlyList<Vector3d>>();
            var partIndices = contacts.NeighborMap.Keys.ToList();

            foreach (var size in new[] { 2, 3, 4 })
            {
                foreach (var group in GenerateCombinations(partIndices, size))
                {
                    var groupKey = string.Join("-", group.OrderBy(i => i));
                    var rays = ComputeMotionRaysForGroup(group, contacts, options);
                    groupRays[groupKey] = rays;
                }
            }

            return groupRays;
        }

        private static IReadOnlyList<Vector3d> ComputeMotionRaysForGroup(
            IEnumerable<int> groupParts, ContactModel contacts, MotionOptions options)
        {
            var constraintNormals = new List<Vector3d>();
            var groupSet = new HashSet<int>(groupParts);

            foreach (var partIndex in groupSet)
            {
                foreach (var contact in contacts.GetContactsForPart(partIndex))
                {
                    // Placeholder: derive normals from contact if needed
                }
            }

            if (constraintNormals.Count == 0)
                return Array.Empty<Vector3d>();

            return ConeIntersection.ComputeExtremeRays(constraintNormals, options);
        }

        private static IEnumerable<int[]> GenerateCombinations(IReadOnlyList<int> items, int size)
        {
            if (size <= 0 || items.Count < size)
                yield break;

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
    }
}



