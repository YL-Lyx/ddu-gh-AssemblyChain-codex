using System.Collections.Generic;
using AssemblyChain.Geometry.Abstractions.Primitives;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Represents a lightweight mesh abstraction suitable for analysis pipelines.
/// </summary>
public interface IMeshLite
{
    IReadOnlyList<Point3> Vertices { get; }

    IReadOnlyList<(int A, int B, int C)> Faces { get; }

    IAABB Bounds { get; }
}
