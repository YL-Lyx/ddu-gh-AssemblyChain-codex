using System;

namespace AssemblyChain.Core.Contact
{

/// <summary>
/// Options for contact detection.
/// </summary>
public readonly record struct DetectionOptions(
    double Tolerance = 1e-6,
    double MinPatchArea = 1e-6,
    string BroadPhase = "sap"
);

}
