using System;
using System.Collections.Generic;
using AssemblyChain.Core.Toolkit.Math;
using FsCheck;
using FsCheck.Xunit;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Toolkit.Math;

/// <summary>
/// FsCheck generators tailored for Rhino's vector and point types.
/// </summary>
public static class ConvexConeArbitraries
{
    private const double MaxMagnitude = 1e6;

    /// <summary>
    /// Builds an <see cref="Arbitrary{T}"/> that produces finite <see cref="Vector3d"/> values.
    /// </summary>
    /// <returns>An <see cref="Arbitrary{T}"/> instance.</returns>
    public static Arbitrary<Vector3d> Vector3d()
    {
        return Arb.From(
            from x in FiniteDouble()
            from y in FiniteDouble()
            from z in FiniteDouble()
            select new Vector3d(x, y, z));
    }

    /// <summary>
    /// Builds an <see cref="Arbitrary{T}"/> that produces finite <see cref="Point3d"/> values.
    /// </summary>
    /// <returns>An <see cref="Arbitrary{T}"/> instance.</returns>
    public static Arbitrary<Point3d> Point3d()
    {
        return Arb.From(
            from x in FiniteDouble()
            from y in FiniteDouble()
            from z in FiniteDouble()
            select new Point3d(x, y, z));
    }

    private static Gen<double> FiniteDouble()
    {
        return Arb.Generate<double>()
            .Where(value => !double.IsNaN(value) && !double.IsInfinity(value) && System.Math.Abs(value) <= MaxMagnitude);
    }
}

/// <summary>
/// Property-based regression tests for <see cref="ConvexCone"/> helpers.
/// </summary>
public class ConvexConePropertyTests
{
    private const double Tolerance = 1e-9;

    static ConvexConePropertyTests()
    {
        Arb.Register<ConvexConeArbitraries>();
    }

    /// <summary>
    /// Halfspaces generated from contacts must classify the contact point as lying on the boundary.
    /// </summary>
    [Property(MaxTest = 200)]
    public void HalfspaceCreatedFromContactContainsTheContactPoint(Vector3d normal, Point3d point, bool isSeparating)
    {
        var halfspace = ConvexCone.CreateHalfspaceFromContact(normal, point, isSeparating);
        var distance = halfspace.SignedDistance(point);
        Assert.InRange(distance, -Tolerance, Tolerance);
    }

    /// <summary>
    /// Creating a cone from contacts must yield the same number of halfspaces as provided normals.
    /// </summary>
    [Property(MaxTest = 200)]
    public void ConeCreationPreservesConstraintCount(Vector3d[] normals)
    {
        var cone = ConvexCone.CreateConeFromContacts(normals);
        Assert.Equal(normals?.Length ?? 0, cone.Halfspaces.Count);
    }

    /// <summary>
    /// Constraint normals should accept their opposing directions and reject their own orientation.
    /// </summary>
    [Property(MaxTest = 200)]
    public void DirectionsOpposingConstraintNormalsAreFeasible(Vector3d[] normals)
    {
        var cone = ConvexCone.CreateConeFromContacts(normals);
        if (normals == null)
        {
            return;
        }

        foreach (var rawNormal in normals)
        {
            var normal = NormalizeLikeHalfspace(rawNormal);
            if (!normal.IsValid || normal.IsZero)
            {
                continue;
            }

            Assert.True(ConvexCone.IsDirectionFeasible(-normal, cone), "Opposite direction should be feasible");
            Assert.False(ConvexCone.IsDirectionFeasible(normal, cone), "Same direction should not be feasible");
        }
    }

    /// <summary>
    /// Extreme ray computation must deduplicate identical halfspace normals.
    /// </summary>
    [Property(MaxTest = 200)]
    public void ExtremeRaysAreDistinct(Vector3d[] normals)
    {
        var cone = ConvexCone.CreateConeFromContacts(normals);
        var extremeRays = ConvexCone.ComputeExtremeRays(cone);
        var expectedUnique = new HashSet<Vector3d>(new Vector3dEqualityComparer());
        foreach (var halfspace in cone.Halfspaces)
        {
            expectedUnique.Add(halfspace.Normal);
        }

        Assert.Equal(expectedUnique.Count, extremeRays.Count);
    }

    /// <summary>
    /// The dual cone should negate every normal without altering the halfspace count.
    /// </summary>
    [Property(MaxTest = 200)]
    public void DualConeNegatesOriginalNormals(Vector3d[] normals)
    {
        var cone = ConvexCone.CreateConeFromContacts(normals);
        var dual = ConvexCone.ComputeDualCone(cone);

        var original = cone?.Halfspaces ?? new List<ConvexCone.Halfspace>();
        var dualNormals = dual?.Halfspaces ?? new List<ConvexCone.Halfspace>();

        Assert.Equal(original.Count, dualNormals.Count);

        for (int i = 0; i < original.Count; i++)
        {
            var expected = -original[i].Normal;
            var actual = dualNormals[i].Normal;
            Assert.True(Vector3dEqualityComparer.AreEqual(expected, actual),
                $"Dual normal at index {i} did not negate the original normal");
        }
    }

    private static Vector3d NormalizeLikeHalfspace(Vector3d vector)
    {
        var result = vector;
        result.Unitize();
        return result;
    }

    private sealed class Vector3dEqualityComparer : IEqualityComparer<Vector3d>
    {
        public bool Equals(Vector3d x, Vector3d y) => AreEqual(x, y);

        public int GetHashCode(Vector3d obj)
        {
            return HashCode.Combine(
                System.Math.Round(obj.X, 9),
                System.Math.Round(obj.Y, 9),
                System.Math.Round(obj.Z, 9));
        }

        public static bool AreEqual(Vector3d x, Vector3d y)
        {
            var difference = x - y;
            return difference.Length <= Tolerance;
        }
    }
}
