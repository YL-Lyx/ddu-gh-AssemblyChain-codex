using System.Collections.Generic;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Constraints.Motion
{
    public sealed class PoseEstimator
    {
        private readonly Plane _supportPlane;
        private readonly int _maxCandidates;

        public PoseEstimator(Plane supportPlane, int maxCandidates = 3)
        {
            _supportPlane = supportPlane;
            _maxCandidates = maxCandidates;
        }

        public List<PoseCandidate> GenerateCandidates(IReadOnlyList<Part> parts)
        {
            var list = new List<PoseCandidate>();
            list.Add(new PoseCandidate(Transform.Identity, 1.0));
            return list;
        }
    }
}

