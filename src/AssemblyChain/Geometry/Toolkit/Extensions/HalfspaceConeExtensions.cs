using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Extensions
{
    /// <summary>
    /// Extension helpers for halfspace and cone computations on Rhino geometry types.
    /// </summary>
    public static class HalfspaceConeExtensions
    {
        /// <summary>
        /// Checks if a point is inside a halfspace defined by a normal and origin with optional tolerance.
        /// </summary>
        /// <param name="point">The point to evaluate.</param>
        /// <param name="normal">Halfspace normal vector.</param>
        /// <param name="origin">Halfspace origin point.</param>
        /// <param name="tolerance">Optional tolerance allowing slight penetration due to floating point noise.</param>
        /// <returns><c>true</c> if the point satisfies the halfspace constraint; otherwise <c>false</c>.</returns>
        public static bool IsInHalfspace(this Point3d point, Vector3d normal, Point3d origin, double tolerance = 0)
        {
            var vector = point - origin;
            return Vector3d.Multiply(vector, normal) >= -tolerance;
        }

        /// <summary>
        /// Checks whether a direction satisfies a collection of halfspace constraints.
        /// </summary>
        /// <param name="direction">Direction vector to test.</param>
        /// <param name="constraintNormals">Collection of constraint normals defining halfspaces.</param>
        /// <param name="tolerance">Tolerance allowing slight penetration of halfspaces.</param>
        /// <returns><c>true</c> if the direction satisfies all halfspace constraints; otherwise <c>false</c>.</returns>
        public static bool SatisfiesHalfspaceConstraints(this Vector3d direction, IReadOnlyList<Vector3d> constraintNormals, double tolerance = 1e-9)
        {
            if (constraintNormals == null || constraintNormals.Count == 0) return true;

            foreach (var normal in constraintNormals)
            {
                if (Vector3d.Multiply(direction, normal) < -tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a stable boundary representation of a cone defined by constraint normals.
        /// </summary>
        /// <param name="constraintNormals">Constraint normals describing the cone.</param>
        /// <returns>A read-only copy of the constraint normals list.</returns>
        public static IReadOnlyList<Vector3d> ToConeBoundary(this IEnumerable<Vector3d> constraintNormals)
        {
            return (constraintNormals ?? Array.Empty<Vector3d>()).ToList();
        }
    }
}
