using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Utils
{
    /// <summary>
    /// Utilities for halfspace and cone computations.
    /// </summary>
    public static class HalfspaceCone
    {
        /// <summary>
        /// Checks if a point is inside a halfspace defined by a normal and origin.
        /// </summary>
        public static bool IsPointInHalfspace(Point3d point, Vector3d normal, Point3d origin)
        {
            var vector = point - origin;
            return Vector3d.Multiply(vector, normal) >= 0;
        }

        /// <summary>
        /// Computes the intersection of multiple halfspaces to test a feasible direction.
        /// </summary>
        public static bool IsDirectionFeasible(Vector3d direction, IReadOnlyList<Vector3d> constraintNormals, double tolerance = 1e-9)
        {
            if (constraintNormals == null || constraintNormals.Count == 0) return true;
            foreach (var normal in constraintNormals)
            {
                var dot = Vector3d.Multiply(direction, normal);
                if (dot < -tolerance) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the boundary set (simplified) of the cone defined by constraint normals.
        /// </summary>
        public static IReadOnlyList<Vector3d> FindConeBoundary(IReadOnlyList<Vector3d> constraintNormals)
        {
            return (constraintNormals ?? Array.Empty<Vector3d>()).ToList();
        }
    }
}



