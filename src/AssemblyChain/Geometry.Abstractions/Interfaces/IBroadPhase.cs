using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Defines the broad phase contact detection behaviour.
/// </summary>
public interface IBroadPhase
{
    IEnumerable<(int A, int B)> FindPairs(IMeshLite meshA, IMeshLite meshB);
}
