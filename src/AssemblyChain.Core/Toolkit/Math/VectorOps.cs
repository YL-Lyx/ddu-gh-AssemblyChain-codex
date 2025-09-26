using System.Collections.Generic;
using AssemblyChain.Core.Contracts;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Math
{
    /// <summary>
    /// Default implementation of <see cref="IVectorOps"/> delegating to existing linear algebra utilities.
    /// </summary>
    public sealed class VectorOps : IVectorOps
    {
        public IReadOnlyList<Vector3d> GramSchmidtOrthogonalize(IReadOnlyList<Vector3d> vectors) => LinearAlgebra.GramSchmidtOrthogonalize(vectors);
        public Vector3d ProjectOnto(Vector3d a, Vector3d b) => LinearAlgebra.ProjectOnto(a, b);
        public Vector3d OrthogonalComplement(Vector3d vector) => LinearAlgebra.OrthogonalComplement(vector);
        public double AngleBetween(Vector3d a, Vector3d b) => LinearAlgebra.AngleBetween(a, b);
        public bool AreLinearlyDependent(Vector3d a, Vector3d b, double tolerance = 1e-10) => LinearAlgebra.AreLinearlyDependent(a, b, tolerance);
        public double Determinant(Vector3d a, Vector3d b, Vector3d c) => LinearAlgebra.Determinant(a, b, c);
        public Vector3d? SolveLinearSystem(Vector3d a1, Vector3d a2, Vector3d a3, Vector3d b) => LinearAlgebra.SolveLinearSystem(a1, a2, a3, b);
        public int Rank(IReadOnlyList<Vector3d> vectors, double tolerance = 1e-10) => LinearAlgebra.Rank(vectors, tolerance);
        public IReadOnlyList<Vector3d> NullSpace(Vector3d vector) => LinearAlgebra.NullSpace(vector);
        public IReadOnlyList<Vector3d> NullSpace(Vector3d a, Vector3d b) => LinearAlgebra.NullSpace(a, b);
        public (double[,] Q, double[,] R) QRDecomposition(IReadOnlyList<Vector3d> vectors) => LinearAlgebra.QRDecomposition(vectors);
    }
}
