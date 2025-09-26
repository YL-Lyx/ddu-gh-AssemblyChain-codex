using System;

namespace AssemblyChain.Constraints.Motion
{

/// <summary>
/// Options for motion analysis.
/// </summary>
public readonly record struct MotionOptions(
    double AngleTolDeg = 5.0,
    double FeasTol = 1e-9,
    bool OnlyBlocking = true,
    int MaxGroupSize = 4
);
}
