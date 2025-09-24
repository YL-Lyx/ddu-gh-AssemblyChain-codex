using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Motion
{
    /// <summary>
    /// Utilities for halfspace and cone computations.
    /// </summary>
    public static class ConeIntersection
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
        /// Checks if a direction satisfies all halfspace constraints.
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
        /// Finds the boundary of the feasible cone defined by constraint normals (simplified).
        /// </summary>
        public static IReadOnlyList<Vector3d> FindConeBoundary(IReadOnlyList<Vector3d> constraintNormals)
        {
            return (constraintNormals ?? Array.Empty<Vector3d>()).ToList();
        }

        /// <summary>
        /// Computes extreme rays from constraint intersection (simplified fallback).
        /// </summary>
        public static IReadOnlyList<Vector3d> ComputeExtremeRays(
            IReadOnlyList<Vector3d> constraintNormals,
            MotionOptions options)
        {
            // Fallback to returning the boundary set; replace with advanced extractor if available
            return FindConeBoundary(constraintNormals);
        }
    }
}



