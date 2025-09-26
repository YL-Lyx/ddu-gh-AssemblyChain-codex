using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Defines the narrow phase collision detection behaviour.
/// </summary>
public interface INarrowPhase
{
    IEnumerable<IContact> DetectContacts(IMeshLite meshA, IMeshLite meshB, IEnumerable<(int A, int B)> candidatePairs);
}
