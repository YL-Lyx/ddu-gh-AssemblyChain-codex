using AssemblyChain.Geometry.Abstractions.Interfaces;

namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Lightweight implementation of <see cref="IVector3"/>.
/// </summary>
public readonly record struct Vector3(double X, double Y, double Z) : IVector3
{
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z);

    public IVector3 Normalize()
    {
        var length = Length;
        return length == 0d ? new Vector3(0d, 0d, 0d) : new Vector3(X / length, Y / length, Z / length);
    }

    public double Dot(IVector3 other) => X * other.X + Y * other.Y + Z * other.Z;

    public IVector3 Cross(IVector3 other) => new Vector3(
        Y * other.Z - Z * other.Y,
        Z * other.X - X * other.Z,
        X * other.Y - Y * other.X);
}
