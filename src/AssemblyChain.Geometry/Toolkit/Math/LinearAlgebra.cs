using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Math
{
    /// <summary>
    /// Small linear algebra utilities for geometric computations.
    /// </summary>
    public static class LinearAlgebra
    {
        /// <summary>
        /// Gram-Schmidt orthogonalization for a set of vectors.
        /// </summary>
        public static IReadOnlyList<Vector3d> GramSchmidtOrthogonalize(IReadOnlyList<Vector3d> vectors)
        {
            var orthogonalized = new List<Vector3d>();
            const double tolerance = 1e-10;
            if (vectors == null) return orthogonalized;

            foreach (var vector in vectors)
            {
                var orthogonal = new Vector3d(vector);
                // Subtract projections onto previously orthogonalized vectors
                foreach (var existing in orthogonalized)
                {
                    var projection = ProjectOnto(orthogonal, existing);
                    orthogonal -= projection;
                }
                if (orthogonal.Length > tolerance)
                {
                    orthogonal.Unitize();
                    orthogonalized.Add(orthogonal);
                }
            }
            return orthogonalized;
        }

        /// <summary>
        /// Projects vector A onto vector B.
        /// </summary>
        public static Vector3d ProjectOnto(Vector3d a, Vector3d b)
        {
            if (b.Length < 1e-10) return Vector3d.Zero;
            var bUnit = b; bUnit.Unitize();
            var dot = Vector3d.Multiply(a, bUnit);
            return bUnit * dot;
        }

        /// <summary>
        /// Computes the orthogonal complement of a vector.
        /// </summary>
        public static Vector3d OrthogonalComplement(Vector3d vector)
        {
            var notParallel = System.Math.Abs(vector.X) > System.Math.Abs(vector.Y)
                ? new Vector3d(-vector.Z, 0, vector.X)
                : new Vector3d(0, -vector.Z, vector.Y);
            var orthogonal = Vector3d.CrossProduct(vector, notParallel);
            orthogonal.Unitize();
            return orthogonal;
        }

        /// <summary>
        /// Computes the angle between two vectors in radians.
        /// </summary>
        public static double AngleBetween(Vector3d a, Vector3d b)
        {
            if (a.Length < 1e-10 || b.Length < 1e-10) return 0;
            var aUnit = a; aUnit.Unitize();
            var bUnit = b; bUnit.Unitize();
            var dot = Vector3d.Multiply(aUnit, bUnit);
            dot = System.Math.Max(-1, System.Math.Min(1, dot));
            return System.Math.Acos(dot);
        }

        /// <summary>
        /// Checks if two vectors are linearly dependent.
        /// </summary>
        public static bool AreLinearlyDependent(Vector3d a, Vector3d b, double tolerance = 1e-10)
        {
            if (a.Length < tolerance || b.Length < tolerance) return true;
            var cross = Vector3d.CrossProduct(a, b);
            return cross.Length < tolerance;
        }

        /// <summary>
        /// Computes the determinant of a 3x3 matrix formed by three vectors.
        /// </summary>
        public static double Determinant(Vector3d a, Vector3d b, Vector3d c)
        {
            return a.X * (b.Y * c.Z - b.Z * c.Y) -
                   a.Y * (b.X * c.Z - b.Z * c.X) +
                   a.Z * (b.X * c.Y - b.Y * c.X);
        }

        /// <summary>
        /// Solves a system of linear equations Ax = b for 3x3 systems.
        /// </summary>
        public static Vector3d? SolveLinearSystem(Vector3d a1, Vector3d a2, Vector3d a3, Vector3d b)
        {
            var det = Determinant(a1, a2, a3);
            if (System.Math.Abs(det) < 1e-10) return null;
            var x = Determinant(b, a2, a3) / det;
            var y = Determinant(a1, b, a3) / det;
            var z = Determinant(a1, a2, b) / det;
            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Computes the rank of a set of vectors.
        /// </summary>
        public static int Rank(IReadOnlyList<Vector3d> vectors, double tolerance = 1e-10)
        {
            if (vectors == null || vectors.Count == 0) return 0;
            var orthogonalized = GramSchmidtOrthogonalize(vectors);
            return orthogonalized.Count;
        }

        /// <summary>
        /// Computes the null space of a vector (orthogonal complement).
        /// </summary>
        public static IReadOnlyList<Vector3d> NullSpace(Vector3d vector)
        {
            var orthogonal = OrthogonalComplement(vector);
            return new[] { orthogonal };
        }

        /// <summary>
        /// Computes the null space of two vectors.
        /// </summary>
        public static IReadOnlyList<Vector3d> NullSpace(Vector3d a, Vector3d b)
        {
            var orthogonalized = GramSchmidtOrthogonalize(new[] { a, b });
            if (orthogonalized.Count >= 2) return Array.Empty<Vector3d>();
            if (orthogonalized.Count == 1)
            {
                var basis1 = orthogonalized[0];
                var basis2 = OrthogonalComplement(basis1);
                return new[] { basis1, basis2 };
            }
            return new[] { Vector3d.XAxis, Vector3d.YAxis };
        }

        /// <summary>
        /// Performs QR decomposition of a matrix formed by vectors (columns).
        /// Returns Q(3xn) and R(nxn).
        /// </summary>
        public static (double[,] Q, double[,] R) QRDecomposition(IReadOnlyList<Vector3d> vectors)
        {
            int n = vectors?.Count ?? 0;
            var Q = new double[3, n];
            var R = new double[n, n];
            var ortho = GramSchmidtOrthogonalize(vectors ?? Array.Empty<Vector3d>());
            for (int i = 0; i < n; i++)
            {
                var v = i < ortho.Count ? ortho[i] : Vector3d.Zero;
                Q[0, i] = v.X; Q[1, i] = v.Y; Q[2, i] = v.Z;
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    R[i, j] = Vector3d.Multiply(ortho.ElementAtOrDefault(i), vectors[j]);
                }
            }
            return (Q, R);
        }
    }
}



