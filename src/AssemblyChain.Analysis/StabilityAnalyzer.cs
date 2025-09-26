using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;

namespace AssemblyChain.Analysis;

/// <summary>
/// Computes a stability margin for an assembly using the center of mass and a support polygon defined by grounded parts.
/// </summary>
public sealed class StabilityAnalyzer
{
    public StabilityResult Compute(Assembly assembly, IEnumerable<string> groundedPartIds)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(groundedPartIds);
        var grounded = groundedPartIds.ToList();
        if (grounded.Count == 0)
        {
            throw new ArgumentException("At least one grounded part is required to compute stability.", nameof(groundedPartIds));
        }

        var supportPoints = new List<(double X, double Y)>();
        foreach (var partId in grounded)
        {
            var part = assembly.PartLookup[partId];
            supportPoints.Add((part.BoundingBox.Min.X, part.BoundingBox.Min.Y));
            supportPoints.Add((part.BoundingBox.Max.X, part.BoundingBox.Min.Y));
            supportPoints.Add((part.BoundingBox.Max.X, part.BoundingBox.Max.Y));
            supportPoints.Add((part.BoundingBox.Min.X, part.BoundingBox.Max.Y));
        }

        var hull = ComputeConvexHull(supportPoints);
        var polygon = new Polygon2D(hull);
        var totalMass = assembly.Parts.Sum(p => p.Mass);
        var centerOfMass = ComputeAssemblyCom(assembly);
        var projection = (centerOfMass.X, centerOfMass.Y);
        var inside = polygon.ContainsPoint(projection);
        var margin = inside ? polygon.DistanceToEdge(projection) : -polygon.DistanceToEdge(projection);
        return new StabilityResult(totalMass, centerOfMass, polygon, margin);
    }

    private static Point3d ComputeAssemblyCom(Assembly assembly)
    {
        double totalMass = 0;
        double sumX = 0, sumY = 0, sumZ = 0;
        foreach (var part in assembly.Parts)
        {
            totalMass += part.Mass;
            sumX += part.CenterOfMass.X * part.Mass;
            sumY += part.CenterOfMass.Y * part.Mass;
            sumZ += part.CenterOfMass.Z * part.Mass;
        }

        if (totalMass <= 0)
        {
            throw new InvalidOperationException("Total mass must be positive to compute a center of mass.");
        }

        return new Point3d(sumX / totalMass, sumY / totalMass, sumZ / totalMass);
    }

    private static List<(double X, double Y)> ComputeConvexHull(IReadOnlyList<(double X, double Y)> points)
    {
        var sorted = points.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        if (sorted.Count < 3)
        {
            return sorted;
        }

        var lower = new List<(double X, double Y)>();
        foreach (var point in sorted)
        {
            while (lower.Count >= 2 && Cross(lower[^2], lower[^1], point) <= 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }

            lower.Add(point);
        }

        var upper = new List<(double X, double Y)>();
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            var point = sorted[i];
            while (upper.Count >= 2 && Cross(upper[^2], upper[^1], point) <= 0)
            {
                upper.RemoveAt(upper.Count - 1);
            }

            upper.Add(point);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);
        return lower;
    }

    private static double Cross((double X, double Y) a, (double X, double Y) b, (double X, double Y) c)
        => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
}

/// <summary>
/// Result of a stability evaluation.
/// </summary>
/// <param name="TotalMass">Combined mass of all parts.</param>
/// <param name="CenterOfMass">Assembly center of mass.</param>
/// <param name="SupportPolygon">Support polygon.</param>
/// <param name="Margin">Positive when the COM is inside the support polygon.</param>
public sealed record StabilityResult(double TotalMass, Point3d CenterOfMass, Polygon2D SupportPolygon, double Margin);
