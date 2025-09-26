namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Represents a line segment between two points.
/// </summary>
public readonly record struct Line3(Point3 Start, Point3 End)
{
    public double Length => System.Math.Sqrt(
        System.Math.Pow(End.X - Start.X, 2) +
        System.Math.Pow(End.Y - Start.Y, 2) +
        System.Math.Pow(End.Z - Start.Z, 2));
}
