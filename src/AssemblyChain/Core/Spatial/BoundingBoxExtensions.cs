namespace AssemblyChain.Core.Spatial;

/// <summary>
/// Common helpers for bounding box intersection tests.
/// </summary>
public static class BoundingBoxExtensions
{
    public static bool Overlaps(this BoundingBox a, BoundingBox b, double tolerance = 1e-6)
        => a.Max.X + tolerance >= b.Min.X && b.Max.X + tolerance >= a.Min.X
            && a.Max.Y + tolerance >= b.Min.Y && b.Max.Y + tolerance >= a.Min.Y
            && a.Max.Z + tolerance >= b.Min.Z && b.Max.Z + tolerance >= a.Min.Z;
}
