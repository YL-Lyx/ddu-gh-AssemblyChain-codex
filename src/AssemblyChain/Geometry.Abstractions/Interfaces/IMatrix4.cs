using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Represents a 4x4 matrix abstraction used for transforms.
/// </summary>
public interface IMatrix4
{
    double this[int row, int column] { get; }

    IEnumerable<double> GetRow(int rowIndex);

    IEnumerable<double> GetColumn(int columnIndex);

    double Determinant { get; }

    IMatrix4 Multiply(IMatrix4 other);
}
