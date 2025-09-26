using System.Collections.Generic;
using AssemblyChain.Core.Toolkit.Extensions;
using FluentAssertions;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Toolkit
{
    public class HalfspaceConeExtensionsTests
    {
        [Fact]
        public void Point_inside_halfspace_returns_true()
        {
            var point = new Point3d(1, 1, 1);
            var normal = new Vector3d(0, 0, 1);
            var origin = new Point3d(0, 0, 0);

            point.IsInHalfspace(normal, origin).Should().BeTrue();
        }

        [Fact]
        public void Point_outside_halfspace_returns_false()
        {
            var point = new Point3d(0, 0, -1);
            var normal = new Vector3d(0, 0, 1);
            var origin = new Point3d(0, 0, 0);

            point.IsInHalfspace(normal, origin).Should().BeFalse();
        }

        [Fact]
        public void Point_near_halfspace_boundary_respects_tolerance()
        {
            var point = new Point3d(0, 0, -1e-10);
            var normal = new Vector3d(0, 0, 1);
            var origin = new Point3d(0, 0, 0);

            point.IsInHalfspace(normal, origin, tolerance: 1e-9).Should().BeTrue();
        }

        [Fact]
        public void Direction_feasible_when_all_dots_non_negative()
        {
            var direction = new Vector3d(0, 0, 1);
            var normals = new List<Vector3d>
            {
                new Vector3d(0, 0, 1),
                new Vector3d(0, 1, 0)
            };

            direction.SatisfiesHalfspaceConstraints(normals).Should().BeTrue();
        }

        [Fact]
        public void Direction_infeasible_when_any_dot_negative()
        {
            var direction = new Vector3d(0, 0, -1);
            var normals = new List<Vector3d>
            {
                new Vector3d(0, 0, 1),
                new Vector3d(0, 1, 0)
            };

            direction.SatisfiesHalfspaceConstraints(normals).Should().BeFalse();
        }

        [Fact]
        public void Direction_feasible_when_no_constraints()
        {
            var direction = new Vector3d(1, 0, 0);

            direction.SatisfiesHalfspaceConstraints(null).Should().BeTrue();
        }

        [Fact]
        public void Cone_boundary_returns_copy_of_normals()
        {
            var normals = new List<Vector3d>
            {
                new Vector3d(1, 0, 0),
                new Vector3d(0, 1, 0)
            };

            var boundary = normals.ToConeBoundary();

            boundary.Should().NotBeSameAs(normals);
            boundary.Should().BeEquivalentTo(normals);
        }

        [Fact]
        public void Cone_boundary_handles_null_collection()
        {
            var boundary = ((IEnumerable<Vector3d>)null).ToConeBoundary();

            boundary.Should().BeEmpty();
        }
    }
}
