namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Represents a 3D point in space.
/// </summary>
public readonly record struct Point3(double X, double Y, double Z)
{
    public static Point3 Origin => new(0d, 0d, 0d);
}
