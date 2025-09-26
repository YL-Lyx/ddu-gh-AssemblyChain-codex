using System;
using System.Collections.Generic;
using AssemblyChain.Core.Toolkit.Extensions;
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
            return point.IsInHalfspace(normal, origin);
        }

        /// <summary>
        /// Computes the intersection of multiple halfspaces to test a feasible direction.
        /// </summary>
        public static bool IsDirectionFeasible(Vector3d direction, IReadOnlyList<Vector3d> constraintNormals, double tolerance = 1e-9)
        {
            return direction.SatisfiesHalfspaceConstraints(constraintNormals, tolerance);
        }

        /// <summary>
        /// Returns the boundary set (simplified) of the cone defined by constraint normals.
        /// </summary>
        public static IReadOnlyList<Vector3d> FindConeBoundary(IReadOnlyList<Vector3d> constraintNormals)
        {
            return constraintNormals.ToConeBoundary();
        }
    }
}



