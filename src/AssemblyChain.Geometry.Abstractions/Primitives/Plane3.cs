using AssemblyChain.Geometry.Abstractions.Interfaces;

namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Represents a geometric plane defined by an origin and normal.
/// </summary>
public readonly record struct Plane3(Point3 Origin, Vector3 Normal)
{
    public Plane3(Point3 origin, IVector3 normal)
        : this(origin, new Vector3(normal.X, normal.Y, normal.Z))
    {
    }

    public double DistanceTo(Point3 point)
    {
        var normalized = Normal.Normalize();
        var vector = new Vector3(point.X - Origin.X, point.Y - Origin.Y, point.Z - Origin.Z);
        return vector.Dot(normalized);
    }
}
