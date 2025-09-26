using AssemblyChain.Geometry.Abstractions.Primitives;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Represents a detected contact between two mesh elements.
/// </summary>
public interface IContact
{
    int ContactId { get; }

    int SourceA { get; }

    int SourceB { get; }

    Point3 Position { get; }

    IVector3 Normal { get; }

    double PenetrationDepth { get; }
}
