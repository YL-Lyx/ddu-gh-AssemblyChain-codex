using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.Core.Contracts;

namespace AssemblyChain.Core.Toolkit.Math
{
    /// <summary>
    /// Utilities for halfspace and convex cone computations.
    /// </summary>
    public static class ConvexCone
    {
        private static IVectorOps _vectorOps = new VectorOps();

        /// <summary>
        /// Configures the vector operations provider used by cone computations.
        /// </summary>
        public static void ConfigureVectorOps(IVectorOps vectorOps)
        {
            _vectorOps = vectorOps ?? throw new ArgumentNullException(nameof(vectorOps));
        }

        /// <summary>
        /// Represents a halfspace defined by ax + by + cz <= d
        /// </summary>
        public struct Halfspace
        {
            public Vector3d Normal;
            public double Offset;

            public Halfspace(Vector3d normal, double offset)
            {
                Normal = normal;
                Normal.Unitize();
                Offset = offset;
            }

            public Halfspace(Vector3d normal, Point3d point)
            {
                Normal = normal;
                Normal.Unitize();
                Offset = Vector3d.Multiply(Normal, new Vector3d(point.X, point.Y, point.Z));
            }

            public double SignedDistance(Point3d point)
            {
                return Vector3d.Multiply(Normal, new Vector3d(point.X, point.Y, point.Z)) - Offset;
            }

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

            public void AddHalfspace(Halfspace halfspace) => Halfspaces.Add(halfspace);
            public void AddHalfspace(Vector3d normal, double offset) => Halfspaces.Add(new Halfspace(normal, offset));
            public bool Contains(Point3d point, double tolerance = 0) => Halfspaces.All(h => h.Contains(point, tolerance));
            public bool IsEmpty() => Halfspaces.Count == 0;
            public IReadOnlyList<Vector3d> GetExtremeRays() => Halfspaces.Select(h => h.Normal).ToList();
        }

        public static Halfspace CreateHalfspaceFromContact(Vector3d contactNormal, Point3d contactPoint, bool isSeparating = true)
        {
            return isSeparating ? new Halfspace(contactNormal, contactPoint) : new Halfspace(-contactNormal, contactPoint);
        }

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

        public static Cone IntersectCones(Cone cone1, Cone cone2)
        {
            var result = new Cone();
            if (cone1 != null) result.Halfspaces.AddRange(cone1.Halfspaces);
            if (cone2 != null) result.Halfspaces.AddRange(cone2.Halfspaces);
            return result;
        }

        public static IReadOnlyList<Vector3d> ComputeExtremeRays(Cone cone)
        {
            return (cone?.Halfspaces ?? new List<Halfspace>()).Select(h => h.Normal).Distinct().ToList();
        }

        public static bool IsDirectionFeasible(Vector3d direction, Cone cone, double tolerance = 1e-9)
        {
            return cone != null && cone.Contains(Point3d.Origin + direction, tolerance);
        }

        public static IReadOnlyList<Vector3d> FindConeBoundary(Cone cone)
        {
            return ComputeExtremeRays(cone);
        }

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

        public static bool IsPointed(Cone cone)
        {
            var extremeRays = ComputeExtremeRays(cone);
            return extremeRays.Count >= 3;
        }

        public static int GetDimension(Cone cone)
        {
            var extremeRays = ComputeExtremeRays(cone);
            if (extremeRays.Count == 0) return 0;
            if (extremeRays.Count == 1) return 1;
            if (extremeRays.Count == 2) return 2;
            return AreCoplanar(extremeRays) ? 2 : 3;
        }

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
                var angle = _vectorOps.AngleBetween(ray1, ray2) * 180.0 / System.Math.PI;
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



