using System;
using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Represents a bounding volume hierarchy abstraction used during collision detection.
/// </summary>
public interface IBvh
{
    IAABB Bounds { get; }

    IEnumerable<IBvh> Children { get; }

    bool IsLeaf { get; }

    IReadOnlyList<int> Primitives { get; }

    void Traverse(Action<IBvh> visitor);
}
