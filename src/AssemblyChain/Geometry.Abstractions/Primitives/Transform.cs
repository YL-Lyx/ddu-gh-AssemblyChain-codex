using System;
using System.Collections.Generic;
using AssemblyChain.Geometry.Abstractions.Interfaces;

namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Represents a 4x4 homogeneous transform matrix.
/// </summary>
public readonly struct Transform : IMatrix4
{
    private readonly double[,] matrix;

    public Transform(double[,] matrix)
    {
        if (matrix.GetLength(0) != 4 || matrix.GetLength(1) != 4)
        {
            throw new ArgumentException("Transform matrix must be 4x4.", nameof(matrix));
        }

        this.matrix = (double[,])matrix.Clone();
    }

    public double this[int row, int column] => matrix[row, column];

    public IEnumerable<double> GetRow(int rowIndex)
    {
        for (var column = 0; column < 4; column++)
        {
            yield return matrix[rowIndex, column];
        }
    }

    public IEnumerable<double> GetColumn(int columnIndex)
    {
        for (var row = 0; row < 4; row++)
        {
            yield return matrix[row, columnIndex];
        }
    }

    public double Determinant
    {
        get
        {
            // Basic Laplace expansion for clarity over performance.
            double det = 0d;
            for (var column = 0; column < 4; column++)
            {
                det += matrix[0, column] * Cofactor(0, column);
            }

            return det;
        }
    }

    public IMatrix4 Multiply(IMatrix4 other)
    {
        var result = new double[4, 4];
        for (var row = 0; row < 4; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                double sum = 0d;
                for (var k = 0; k < 4; k++)
                {
                    sum += matrix[row, k] * other[k, column];
                }

                result[row, column] = sum;
            }
        }

        return new Transform(result);
    }

    private double Cofactor(int row, int column)
    {
        var minor = Minor(row, column);
        var sign = ((row + column) % 2) == 0 ? 1d : -1d;
        return sign * Determinant3x3(minor);
    }

    private static double[,] Minor(int row, int column, double[,] source)
    {
        var minor = new double[3, 3];
        var mi = 0;
        for (var i = 0; i < 4; i++)
        {
            if (i == row)
            {
                continue;
            }

            var mj = 0;
            for (var j = 0; j < 4; j++)
            {
                if (j == column)
                {
                    continue;
                }

                minor[mi, mj] = source[i, j];
                mj++;
            }

            mi++;
        }

        return minor;
    }

    private double[,] Minor(int row, int column) => Minor(row, column, matrix);

    private static double Determinant3x3(double[,] m)
    {
        return m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1])
             - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0])
             + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
    }
}
