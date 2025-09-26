using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Math
{
    /// <summary>
    /// Utilities for halfspace and convex cone computations.
    /// </summary>
    public static class ConvexCone
    {
        /// <summary>
        /// Represents a halfspace defined by ax + by + cz <= d
        /// </summary>
        public struct Halfspace
        {
            public Vector3d Normal;
            public double Offset;

            /// <summary>
            /// Initializes a new instance of the <see cref="Halfspace"/> struct using an explicit offset.
            /// </summary>
            /// <param name="normal">The outward facing halfspace normal.</param>
            /// <param name="offset">The signed offset from the origin.</param>
            public Halfspace(Vector3d normal, double offset)
            {
                Normal = normal;
                Normal.Unitize();
                Offset = offset;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Halfspace"/> struct constrained to pass through a point.
            /// </summary>
            /// <param name="normal">The outward facing halfspace normal.</param>
            /// <param name="point">A point that lies on the halfspace boundary.</param>
            public Halfspace(Vector3d normal, Point3d point)
            {
                Normal = normal;
                Normal.Unitize();
                Offset = Vector3d.Multiply(Normal, new Vector3d(point.X, point.Y, point.Z));
            }

            /// <summary>
            /// Computes the signed distance from the halfspace boundary to the specified point.
            /// </summary>
            /// <param name="point">The sample point.</param>
            /// <returns>A negative value for inside points, zero on the boundary and positive outside.</returns>
            public double SignedDistance(Point3d point)
            {
                return Vector3d.Multiply(Normal, new Vector3d(point.X, point.Y, point.Z)) - Offset;
            }

            /// <summary>
            /// Determines whether the point lies within or on the boundary of the halfspace.
            /// </summary>
            /// <param name="point">The sample point.</param>
            /// <param name="tolerance">Optional tolerance to relax the containment test.</param>
            /// <returns><see langword="true"/> if the point lies inside the halfspace; otherwise, <see langword="false"/>.</returns>
            public bool Contains(Point3d point, double tolerance = 0)
            {
                return SignedDistance(point) <= tolerance;
            }
        }

        /// <summary>
        /// Represents a convex cone as intersection of halfspaces.
        /// </summary>
        public class Cone
        {
            public List<Halfspace> Halfspaces { get; } = new List<Halfspace>();

            /// <summary>
            /// Appends a pre-constructed halfspace to the cone definition.
            /// </summary>
            /// <param name="halfspace">The halfspace to append.</param>
            public void AddHalfspace(Halfspace halfspace) => Halfspaces.Add(halfspace);

            /// <summary>
            /// Appends a halfspace constructed from a normal and offset to the cone definition.
            /// </summary>
            /// <param name="normal">The outward facing halfspace normal.</param>
            /// <param name="offset">The signed offset from the origin.</param>
            public void AddHalfspace(Vector3d normal, double offset) => Halfspaces.Add(new Halfspace(normal, offset));

            /// <summary>
            /// Tests whether a point is contained within the cone.
            /// </summary>
            /// <param name="point">The sample point.</param>
            /// <param name="tolerance">Optional tolerance to relax the containment test.</param>
            /// <returns><see langword="true"/> when the point satisfies all halfspace constraints.</returns>
            public bool Contains(Point3d point, double tolerance = 0) => Halfspaces.All(h => h.Contains(point, tolerance));

            /// <summary>
            /// Determines whether the cone currently contains any halfspaces.
            /// </summary>
            /// <returns><see langword="true"/> if no halfspaces are defined.</returns>
            public bool IsEmpty() => Halfspaces.Count == 0;

            /// <summary>
            /// Returns the set of halfspace normals that define the cone boundary.
            /// </summary>
            /// <returns>A copy of the halfspace normals.</returns>
            public IReadOnlyList<Vector3d> GetExtremeRays() => Halfspaces.Select(h => h.Normal).ToList();
        }

        /// <summary>
        /// Creates a halfspace definition derived from a contact normal and point.
        /// </summary>
        /// <param name="contactNormal">The contact normal.</param>
        /// <param name="contactPoint">The contact point.</param>
        /// <param name="isSeparating">If <see langword="true"/>, keeps the original normal; otherwise, flips it.</param>
        /// <returns>A normalized halfspace representing the contact constraint.</returns>
        public static Halfspace CreateHalfspaceFromContact(Vector3d contactNormal, Point3d contactPoint, bool isSeparating = true)
        {
            return isSeparating ? new Halfspace(contactNormal, contactPoint) : new Halfspace(-contactNormal, contactPoint);
        }

        /// <summary>
        /// Builds a convex cone from a collection of contact constraint normals.
        /// </summary>
        /// <param name="constraintNormals">The constraint normals that bound the cone.</param>
        /// <returns>A cone that aggregates each supplied halfspace constraint.</returns>
        public static Cone CreateConeFromContacts(IReadOnlyList<Vector3d> constraintNormals)
        {
            var cone = new Cone();
            if (constraintNormals == null) return cone;
            foreach (var normal in constraintNormals)
            {
                cone.AddHalfspace(normal, 0);
            }
            return cone;
        }

        /// <summary>
        /// Computes the intersection of two cones by aggregating their halfspaces.
        /// </summary>
        /// <param name="cone1">The first cone.</param>
        /// <param name="cone2">The second cone.</param>
        /// <returns>A cone representing the combined constraint set.</returns>
        public static Cone IntersectCones(Cone cone1, Cone cone2)
        {
            var result = new Cone();
            if (cone1 != null) result.Halfspaces.AddRange(cone1.Halfspaces);
            if (cone2 != null) result.Halfspaces.AddRange(cone2.Halfspaces);
            return result;
        }

        /// <summary>
        /// Extracts the unique extreme rays that form the boundary of the cone.
        /// </summary>
        /// <param name="cone">The cone to evaluate.</param>
        /// <returns>A distinct list of normalized boundary rays.</returns>
        public static IReadOnlyList<Vector3d> ComputeExtremeRays(Cone cone)
        {
            return (cone?.Halfspaces ?? new List<Halfspace>()).Select(h => h.Normal).Distinct().ToList();
        }

        /// <summary>
        /// Tests whether a direction vector lies inside the cone.
        /// </summary>
        /// <param name="direction">The direction vector to evaluate.</param>
        /// <param name="cone">The cone describing the feasible region.</param>
        /// <param name="tolerance">Optional tolerance to relax the containment test.</param>
        /// <returns><see langword="true"/> if the direction is feasible.</returns>
        public static bool IsDirectionFeasible(Vector3d direction, Cone cone, double tolerance = 1e-9)
        {
            return cone != null && cone.Contains(Point3d.Origin + direction, tolerance);
        }

        /// <summary>
        /// Retrieves the cone boundary rays without performing additional computations.
        /// </summary>
        /// <param name="cone">The cone to evaluate.</param>
        /// <returns>The set of extreme rays.</returns>
        public static IReadOnlyList<Vector3d> FindConeBoundary(Cone cone)
        {
            return ComputeExtremeRays(cone);
        }

        /// <summary>
        /// Computes the dual cone formed by negating every halfspace normal.
        /// </summary>
        /// <param name="cone">The source cone.</param>
        /// <returns>The dual cone.</returns>
        public static Cone ComputeDualCone(Cone cone)
        {
            var dual = new Cone();
            if (cone == null) return dual;
            foreach (var halfspace in cone.Halfspaces)
            {
                dual.AddHalfspace(-halfspace.Normal, 0);
            }
            return dual;
        }

        /// <summary>
        /// Determines whether the cone is pointed, i.e. the extreme rays do not lie on the same line.
        /// </summary>
        /// <param name="cone">The cone to evaluate.</param>
        /// <returns><see langword="true"/> if at least three unique rays are present.</returns>
        public static bool IsPointed(Cone cone)
        {
            var extremeRays = ComputeExtremeRays(cone);
            return extremeRays.Count >= 3;
        }

        /// <summary>
        /// Estimates the topological dimension of the cone based on its extreme rays.
        /// </summary>
        /// <param name="cone">The cone to evaluate.</param>
        /// <returns>The estimated dimension ranging from 0 to 3.</returns>
        public static int GetDimension(Cone cone)
        {
            var extremeRays = ComputeExtremeRays(cone);
            if (extremeRays.Count == 0) return 0;
            if (extremeRays.Count == 1) return 1;
            if (extremeRays.Count == 2) return 2;
            return AreCoplanar(extremeRays) ? 2 : 3;
        }

        /// <summary>
        /// Tests whether a set of vectors are coplanar using their cross product.
        /// </summary>
        /// <param name="vectors">The vectors to evaluate.</param>
        /// <returns><see langword="true"/> if the vectors are coplanar.</returns>
        private static bool AreCoplanar(IReadOnlyList<Vector3d> vectors)
        {
            if (vectors.Count < 3) return true;
            var v1 = vectors[0];
            var v2 = vectors[1];
            var normal = Vector3d.CrossProduct(v1, v2);
            for (int i = 2; i < vectors.Count; i++)
            {
                var dot = Vector3d.Multiply(normal, vectors[i]);
                if (System.Math.Abs(dot) > 1e-10) return false;
            }
            return true;
        }

        /// <summary>
        /// Generates a set of feasible motion rays by interpolating between extreme rays.
        /// </summary>
        /// <param name="cone">The cone describing the feasible region.</param>
        /// <param name="numRays">Requested number of fallback rays when no extreme rays are available.</param>
        /// <param name="minAngle">Minimum angular resolution between interpolated rays in degrees.</param>
        /// <returns>A list of normalized direction vectors.</returns>
        public static IReadOnlyList<Vector3d> GenerateMotionRays(Cone cone, int numRays = 32, double minAngle = 5.0)
        {
            var extremeRays = ComputeExtremeRays(cone);
            if (extremeRays.Count == 0) return GenerateUniformRays(numRays);

            var motionRays = new List<Vector3d>();
            for (int i = 0; i < extremeRays.Count; i++)
            {
                var ray1 = extremeRays[i];
                var ray2 = extremeRays[(i + 1) % extremeRays.Count];
                motionRays.Add(ray1);
                var angle = LinearAlgebra.AngleBetween(ray1, ray2) * 180.0 / System.Math.PI;
                var numIntermediate = System.Math.Max(1, (int)(angle / minAngle));
                for (int j = 1; j < numIntermediate; j++)
                {
                    var t = j / (double)numIntermediate;
                    var interpolated = Slerp(ray1, ray2, t);
                    motionRays.Add(interpolated);
                }
            }
            return motionRays.Distinct().ToList();
        }

        /// <summary>
        /// Interpolates between two vectors on the unit sphere using spherical linear interpolation.
        /// </summary>
        /// <param name="a">The starting vector.</param>
        /// <param name="b">The ending vector.</param>
        /// <param name="t">Interpolation parameter between 0 and 1.</param>
        /// <returns>The interpolated unit vector.</returns>
        private static Vector3d Slerp(Vector3d a, Vector3d b, double t)
        {
            var dot = Vector3d.Multiply(a, b);
            dot = System.Math.Max(-1, System.Math.Min(1, dot));
            if (System.Math.Abs(dot - 1) < 1e-10) return a;
            var theta = System.Math.Acos(dot);
            var sinTheta = System.Math.Sin(theta);
            var w1 = System.Math.Sin((1 - t) * theta) / sinTheta;
            var w2 = System.Math.Sin(t * theta) / sinTheta;
            return a * w1 + b * w2;
        }

        /// <summary>
        /// Generates a uniform distribution of unit vectors using a spherical Fibonacci lattice.
        /// </summary>
        /// <param name="count">The number of vectors to generate.</param>
        /// <returns>A list of uniformly distributed unit vectors.</returns>
        private static IReadOnlyList<Vector3d> GenerateUniformRays(int count)
        {
            var rays = new List<Vector3d>();
            if (count <= 0) return rays;
            var phi = System.Math.PI * (3 - System.Math.Sqrt(5));
            for (int i = 0; i < count; i++)
            {
                var y = 1 - (i / (double)(count - 1)) * 2;
                var radius = System.Math.Sqrt(1 - y * y);
                var theta = phi * i;
                var x = System.Math.Cos(theta) * radius;
                var z = System.Math.Sin(theta) * radius;
                rays.Add(new Vector3d(x, y, z));
            }
            return rays;
        }
    }
}



