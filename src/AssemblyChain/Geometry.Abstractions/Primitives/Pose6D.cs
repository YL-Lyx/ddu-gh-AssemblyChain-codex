using System;
using AssemblyChain.Geometry.Abstractions.Interfaces;

namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Represents a 6 degree of freedom pose using XYZ translation and ZYX Euler angles.
/// </summary>
public readonly record struct Pose6D(Point3 Position, Vector3 EulerAngles)
{
    /// <summary>
    /// Creates a transform matrix from the pose.
    /// </summary>
    public Transform ToTransform()
    {
        var (rx, ry, rz) = (EulerAngles.X, EulerAngles.Y, EulerAngles.Z);
        var (sx, cx) = (Math.Sin(rx), Math.Cos(rx));
        var (sy, cy) = (Math.Sin(ry), Math.Cos(ry));
        var (sz, cz) = (Math.Sin(rz), Math.Cos(rz));

        var m00 = cz * cy;
        var m01 = cz * sy * sx - sz * cx;
        var m02 = cz * sy * cx + sz * sx;

        var m10 = sz * cy;
        var m11 = sz * sy * sx + cz * cx;
        var m12 = sz * sy * cx - cz * sx;

        var m20 = -sy;
        var m21 = cy * sx;
        var m22 = cy * cx;

        return new Transform(new double[,]
        {
            { m00, m01, m02, Position.X },
            { m10, m11, m12, Position.Y },
            { m20, m21, m22, Position.Z },
            { 0d,  0d,  0d,  1d }
        });
    }
}
