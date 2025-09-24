using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Read-only motion model.
    /// Contains motion cones (extreme rays) for parts and groups based on contact constraints.
    /// </summary>
    public sealed class MotionModel
    {
        public IReadOnlyDictionary<int, IReadOnlyList<Vector3d>> PartMotionRays { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<Vector3d>> GroupMotionRays { get; }
        public string Hash { get; }
        public int PartCount => PartMotionRays.Count;
        public int GroupCount => GroupMotionRays.Count;

        internal MotionModel(
            IReadOnlyDictionary<int, IReadOnlyList<Vector3d>> partMotionRays,
            IReadOnlyDictionary<string, IReadOnlyList<Vector3d>> groupMotionRays,
            string hash)
        {
            PartMotionRays = partMotionRays ?? throw new ArgumentNullException(nameof(partMotionRays));
            GroupMotionRays = groupMotionRays ?? throw new ArgumentNullException(nameof(groupMotionRays));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public IReadOnlyList<Vector3d> GetPartMotionRays(int partIndex)
        {
            return PartMotionRays.TryGetValue(partIndex, out var rays) ? rays : Array.Empty<Vector3d>();
        }

        public IReadOnlyList<Vector3d> GetGroupMotionRays(IEnumerable<int> partIndices)
        {
            var sortedIndices = partIndices.OrderBy(i => i).ToArray();
            var groupKey = string.Join("-", sortedIndices);
            return GetGroupMotionRays(groupKey);
        }

        public IReadOnlyList<Vector3d> GetGroupMotionRays(string groupKey)
        {
            return GroupMotionRays.TryGetValue(groupKey, out var rays) ? rays : Array.Empty<Vector3d>();
        }

        public bool IsMotionFeasible(int partIndex, Vector3d motionVector, double tolerance = 1e-9)
        {
            var rays = GetPartMotionRays(partIndex);
            if (rays.Count == 0) return true; // No constraints

            foreach (var ray in rays)
            {
                var dot = Vector3d.Multiply(ray, motionVector);
                if (dot < -tolerance) return false;
            }
            return true;
        }

        public bool IsGroupMotionFeasible(IEnumerable<int> partIndices, Vector3d motionVector, double tolerance = 1e-9)
        {
            var rays = GetGroupMotionRays(partIndices);
            if (rays.Count == 0) return true; // No constraints

            foreach (var ray in rays)
            {
                var dot = Vector3d.Multiply(ray, motionVector);
                if (dot < -tolerance) return false;
            }
            return true;
        }

        public IEnumerable<string> GetAllGroupKeys()
        {
            return GroupMotionRays.Keys;
        }

        public static int[] ParseGroupKey(string groupKey)
        {
            return groupKey.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }
    }
}



