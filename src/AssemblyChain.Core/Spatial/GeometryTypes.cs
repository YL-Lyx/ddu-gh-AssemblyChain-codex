using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyChain.Core.Spatial;

/// <summary>
/// Lightweight immutable 3D vector with common operations used throughout the planning pipeline.
/// </summary>
public readonly record struct Vector3d(double X, double Y, double Z)
{
    public static Vector3d Zero => new(0, 0, 0);

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3d Normalize()
    {
        var length = Length;
        return length < 1e-9 ? Zero : new Vector3d(X / length, Y / length, Z / length);
    }

    public static Vector3d operator +(Vector3d a, Vector3d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3d operator -(Vector3d a, Vector3d b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3d operator *(Vector3d v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3d operator /(Vector3d v, double scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static double Dot(Vector3d a, Vector3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3d Cross(Vector3d a, Vector3d b)
        => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    public static double Distance(Vector3d a, Vector3d b) => (a - b).Length;
}

/// <summary>
/// Immutable 3D point record.
/// </summary>
public readonly record struct Point3d(double X, double Y, double Z)
{
    public static Point3d operator +(Point3d p, Vector3d v) => new(p.X + v.X, p.Y + v.Y, p.Z + v.Z);

    public static Vector3d operator -(Point3d a, Point3d b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
}

/// <summary>
/// Axis-aligned bounding box helper used for broad-phase collision checks.
/// </summary>
public readonly record struct BoundingBox(Point3d Min, Point3d Max)
{
    public bool Contains(Point3d point, double tolerance = 1e-6)
        => point.X <= Max.X + tolerance && point.X >= Min.X - tolerance
        && point.Y <= Max.Y + tolerance && point.Y >= Min.Y - tolerance
        && point.Z <= Max.Z + tolerance && point.Z >= Min.Z - tolerance;

    public static BoundingBox FromPoints(IEnumerable<Point3d> points)
    {
        if (points is null)
        {
            throw new ArgumentNullException(nameof(points));
        }

        var list = points.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one point is required to create a bounding box.", nameof(points));
        }

        var minX = list.Min(p => p.X);
        var minY = list.Min(p => p.Y);
        var minZ = list.Min(p => p.Z);
        var maxX = list.Max(p => p.X);
        var maxY = list.Max(p => p.Y);
        var maxZ = list.Max(p => p.Z);

        return new BoundingBox(new Point3d(minX, minY, minZ), new Point3d(maxX, maxY, maxZ));
    }
}

/// <summary>
/// Planar half-space representation of a constraint.
/// </summary>
public readonly record struct HalfSpace(Vector3d Normal, double Offset)
{
    public bool Contains(Point3d point, double tolerance = 1e-6)
        => Vector3d.Dot(Normal, new Vector3d(point.X, point.Y, point.Z)) <= Offset + tolerance;
}

/// <summary>
/// Simple polygon defined on the XY plane for stability evaluations.
/// </summary>
public sealed class Polygon2D
{
    private readonly IReadOnlyList<(double X, double Y)> _points;

    public Polygon2D(IEnumerable<(double X, double Y)> points)
    {
        _points = points?.ToList() ?? throw new ArgumentNullException(nameof(points));
        if (_points.Count < 3)
        {
            throw new ArgumentException("A polygon requires at least three points.", nameof(points));
        }
    }

    public IReadOnlyList<(double X, double Y)> Points => _points;

    public double Area
    {
        get
        {
            double sum = 0;
            for (int i = 0; i < _points.Count; i++)
            {
                var (x1, y1) = _points[i];
                var (x2, y2) = _points[(i + 1) % _points.Count];
                sum += x1 * y2 - x2 * y1;
            }

            return Math.Abs(sum) * 0.5;
        }
    }

    public bool ContainsPoint((double X, double Y) point)
    {
        var (px, py) = point;
        bool inside = false;
        for (int i = 0, j = _points.Count - 1; i < _points.Count; j = i++)
        {
            var (xi, yi) = _points[i];
            var (xj, yj) = _points[j];
            bool intersect = ((yi > py) != (yj > py))
                && (px < (xj - xi) * (py - yi) / (yj - yi + 1e-12) + xi);
            if (intersect)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public double DistanceToEdge((double X, double Y) point)
    {
        var (px, py) = point;
        double minDistance = double.MaxValue;
        for (int i = 0; i < _points.Count; i++)
        {
            var (x1, y1) = _points[i];
            var (x2, y2) = _points[(i + 1) % _points.Count];

            double dx = x2 - x1;
            double dy = y2 - y1;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared < 1e-9)
            {
                continue;
            }

            double t = ((px - x1) * dx + (py - y1) * dy) / lengthSquared;
            t = Math.Clamp(t, 0, 1);

            double projX = x1 + t * dx;
            double projY = y1 + t * dy;
            double distance = Math.Sqrt(Math.Pow(px - projX, 2) + Math.Pow(py - projY, 2));
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }
}
