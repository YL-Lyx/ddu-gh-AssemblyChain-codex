using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Vector algebra contract used by geometric algorithms.
    /// </summary>
    public interface IVectorOps
    {
        IReadOnlyList<Vector3d> GramSchmidtOrthogonalize(IReadOnlyList<Vector3d> vectors);
        Vector3d ProjectOnto(Vector3d a, Vector3d b);
        Vector3d OrthogonalComplement(Vector3d vector);
        double AngleBetween(Vector3d a, Vector3d b);
        bool AreLinearlyDependent(Vector3d a, Vector3d b, double tolerance = 1e-10);
        double Determinant(Vector3d a, Vector3d b, Vector3d c);
        Vector3d? SolveLinearSystem(Vector3d a1, Vector3d a2, Vector3d a3, Vector3d b);
        int Rank(IReadOnlyList<Vector3d> vectors, double tolerance = 1e-10);
        IReadOnlyList<Vector3d> NullSpace(Vector3d vector);
        IReadOnlyList<Vector3d> NullSpace(Vector3d a, Vector3d b);
        (double[,] Q, double[,] R) QRDecomposition(IReadOnlyList<Vector3d> vectors);
    }
}
