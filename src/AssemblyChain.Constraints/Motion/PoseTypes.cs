using Rhino.Geometry;

namespace AssemblyChain.Constraints.Motion
{
    public readonly struct PoseCandidate
    {
        public Transform World { get; }
        public double Score { get; }
        public PoseCandidate(Transform world, double score = 1.0)
        {
            World = world;
            Score = score;
        }
    }
}

