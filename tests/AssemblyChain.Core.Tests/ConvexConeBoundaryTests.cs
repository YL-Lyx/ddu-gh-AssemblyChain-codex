using System.Collections.Generic;
using AssemblyChain.Core.Toolkit.Math;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Toolkit.Math;

/// <summary>
/// Boundary-focused tests safeguarding <see cref="ConvexCone"/> edge cases.
/// </summary>
public class ConvexConeBoundaryTests
{
    private const double Tolerance = 1e-9;

    /// <summary>
    /// Zero normals should remain zero and classify the seed point as lying on the boundary.
    /// </summary>
    [Fact]
    public void CreateHalfspaceFromContact_WithZeroNormal_KeepsZeroNormal()
    {
        var zero = Vector3d.Zero;
        var point = new Point3d(5, -3, 2);

        var halfspace = ConvexCone.CreateHalfspaceFromContact(zero, point);

        Assert.True(halfspace.Normal.EpsilonEquals(Vector3d.Zero, Tolerance));
        Assert.InRange(halfspace.SignedDistance(point), -Tolerance, Tolerance);
    }

    /// <summary>
    /// Non-separating halfspaces should flip the input normal.
    /// </summary>
    [Fact]
    public void CreateHalfspaceFromContact_NonSeparating_FlipsNormal()
    {
        var normal = new Vector3d(1, 2, 3);
        var point = Point3d.Origin;

        var halfspace = ConvexCone.CreateHalfspaceFromContact(normal, point, isSeparating: false);

        normal.Unitize();
        Assert.True(halfspace.Normal.EpsilonEquals(-normal, Tolerance));
    }

    /// <summary>
    /// Null input should yield an empty cone instead of throwing.
    /// </summary>
    [Fact]
    public void CreateConeFromContacts_WithNullInput_ReturnsEmptyCone()
    {
        var cone = ConvexCone.CreateConeFromContacts(null);

        Assert.True(cone.IsEmpty());
    }

    /// <summary>
    /// An empty list should produce a cone with no halfspaces.
    /// </summary>
    [Fact]
    public void CreateConeFromContacts_WithEmptyList_HasNoHalfspaces()
    {
        var cone = ConvexCone.CreateConeFromContacts(new List<Vector3d>());

        Assert.Empty(cone.Halfspaces);
    }

    /// <summary>
    /// Intersecting with a null cone should return the other operand unchanged.
    /// </summary>
    [Fact]
    public void IntersectCones_WithNullFirst_ReturnsSecond()
    {
        var second = new ConvexCone.Cone();
        second.AddHalfspace(Vector3d.XAxis, 0);

        var result = ConvexCone.IntersectCones(null, second);

        Assert.Single(result.Halfspaces);
    }

    /// <summary>
    /// Intersecting with a null cone should return the original operand unchanged.
    /// </summary>
    [Fact]
    public void IntersectCones_WithNullSecond_ReturnsFirst()
    {
        var first = new ConvexCone.Cone();
        first.AddHalfspace(Vector3d.YAxis, 0);

        var result = ConvexCone.IntersectCones(first, null);

        Assert.Single(result.Halfspaces);
    }

    /// <summary>
    /// Null cone inputs should produce no extreme rays.
    /// </summary>
    [Fact]
    public void ComputeExtremeRays_WithNullCone_ReturnsEmpty()
    {
        var rays = ConvexCone.ComputeExtremeRays(null);

        Assert.Empty(rays);
    }

    /// <summary>
    /// Feasibility checks against null cones should return false.
    /// </summary>
    [Fact]
    public void IsDirectionFeasible_WithNullCone_ReturnsFalse()
    {
        var feasible = ConvexCone.IsDirectionFeasible(Vector3d.ZAxis, null);

        Assert.False(feasible);
    }

    /// <summary>
    /// Requesting zero rays should return an empty collection.
    /// </summary>
    [Fact]
    public void GenerateMotionRays_WithZeroRequest_ReturnsEmpty()
    {
        var cone = new ConvexCone.Cone();

        var rays = ConvexCone.GenerateMotionRays(cone, numRays: 0);

        Assert.Empty(rays);
    }

    /// <summary>
    /// A single extreme ray should be returned without interpolation.
    /// </summary>
    [Fact]
    public void GenerateMotionRays_WithSingleExtremeRay_ReturnsNormalizedRay()
    {
        var cone = new ConvexCone.Cone();
        cone.AddHalfspace(Vector3d.XAxis, 0);

        var rays = ConvexCone.GenerateMotionRays(cone, numRays: 8);

        Assert.Single(rays);
        Assert.True(rays[0].EpsilonEquals(Vector3d.XAxis, Tolerance));
    }

    /// <summary>
    /// An empty cone is zero dimensional.
    /// </summary>
    [Fact]
    public void GetDimension_WithNoHalfspaces_ReturnsZero()
    {
        var cone = new ConvexCone.Cone();

        Assert.Equal(0, ConvexCone.GetDimension(cone));
    }

    /// <summary>
    /// A single halfspace yields a one-dimensional feasible region.
    /// </summary>
    [Fact]
    public void GetDimension_WithSingleHalfspace_ReturnsOne()
    {
        var cone = new ConvexCone.Cone();
        cone.AddHalfspace(Vector3d.XAxis, 0);

        Assert.Equal(1, ConvexCone.GetDimension(cone));
    }

    /// <summary>
    /// Two orthogonal halfspaces define a planar feasible region.
    /// </summary>
    [Fact]
    public void GetDimension_WithTwoDistinctHalfspaces_ReturnsTwo()
    {
        var cone = new ConvexCone.Cone();
        cone.AddHalfspace(Vector3d.XAxis, 0);
        cone.AddHalfspace(Vector3d.YAxis, 0);

        Assert.Equal(2, ConvexCone.GetDimension(cone));
    }

    /// <summary>
    /// Three orthogonal halfspaces should produce a full three-dimensional cone.
    /// </summary>
    [Fact]
    public void GetDimension_WithThreeNonCoplanarHalfspaces_ReturnsThree()
    {
        var cone = new ConvexCone.Cone();
        cone.AddHalfspace(new Vector3d(1, 0, 0), 0);
        cone.AddHalfspace(new Vector3d(0, 1, 0), 0);
        cone.AddHalfspace(new Vector3d(0, 0, 1), 0);

        Assert.Equal(3, ConvexCone.GetDimension(cone));
    }

    /// <summary>
    /// Computing the dual cone for a null input should return an empty cone.
    /// </summary>
    [Fact]
    public void ComputeDualCone_WithNullCone_ReturnsEmpty()
    {
        var dual = ConvexCone.ComputeDualCone(null);

        Assert.True(dual.IsEmpty());
    }
}
