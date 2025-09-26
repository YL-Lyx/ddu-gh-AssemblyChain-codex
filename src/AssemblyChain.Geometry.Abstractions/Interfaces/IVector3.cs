namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Represents a 3D vector abstraction.
/// </summary>
public interface IVector3
{
    double X { get; }

    double Y { get; }

    double Z { get; }

    double Length { get; }

    IVector3 Normalize();

    double Dot(IVector3 other);

    IVector3 Cross(IVector3 other);
}
