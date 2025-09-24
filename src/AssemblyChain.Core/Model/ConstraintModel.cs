using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Read-only constraint model.
    /// Combines graph and motion constraints into unified rules.
    /// </summary>
    public sealed class ConstraintModel
    {
        public GraphModel GraphModel { get; }
        public MotionModel MotionModel { get; }
        public IReadOnlyDictionary<int, IReadOnlyList<string>> PartConstraints { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<string>> GroupConstraints { get; }
        public string Hash { get; }

        internal ConstraintModel(
            GraphModel graphModel,
            MotionModel motionModel,
            IReadOnlyDictionary<int, IReadOnlyList<string>> partConstraints,
            IReadOnlyDictionary<string, IReadOnlyList<string>> groupConstraints,
            string hash)
        {
            GraphModel = graphModel ?? throw new ArgumentNullException(nameof(graphModel));
            MotionModel = motionModel ?? throw new ArgumentNullException(nameof(motionModel));
            PartConstraints = partConstraints ?? throw new ArgumentNullException(nameof(partConstraints));
            GroupConstraints = groupConstraints ?? throw new ArgumentNullException(nameof(groupConstraints));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public IReadOnlyList<string> GetPartConstraints(int partIndex)
        {
            return PartConstraints.TryGetValue(partIndex, out var constraints) ? constraints : Array.Empty<string>();
        }

        public IReadOnlyList<string> GetGroupConstraints(string groupKey)
        {
            return GroupConstraints.TryGetValue(groupKey, out var constraints) ? constraints : Array.Empty<string>();
        }

        public IReadOnlyList<string> GetGroupConstraints(IEnumerable<int> partIndices)
        {
            var sortedIndices = partIndices.OrderBy(i => i).ToArray();
            var groupKey = string.Join("-", sortedIndices);
            return GetGroupConstraints(groupKey);
        }

        public bool CanPartMove(int partIndex, Vector3d direction, double tolerance = 1e-9)
        {
            if (GraphModel.GetInDegree(partIndex) > 0)
                return false;
            return MotionModel.IsMotionFeasible(partIndex, direction, tolerance);
        }

        public bool CanGroupMove(IEnumerable<int> partIndices, Vector3d direction, double tolerance = 1e-9)
        {
            var indices = partIndices as int[] ?? partIndices.ToArray();
            foreach (var index in indices)
            {
                if (GraphModel.GetInDegree(index) > 0)
                    return false;
            }
            return MotionModel.IsGroupMotionFeasible(indices, direction, tolerance);
        }
    }
}



