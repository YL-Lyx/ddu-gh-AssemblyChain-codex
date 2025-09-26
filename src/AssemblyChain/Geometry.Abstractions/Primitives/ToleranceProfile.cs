using AssemblyChain.Geometry.Abstractions.Interfaces;

namespace AssemblyChain.Geometry.Abstractions.Primitives;

/// <summary>
/// Value type implementing <see cref="IToleranceProfile"/>.
/// </summary>
public readonly record struct ToleranceProfile(double Linear, double Angular) : IToleranceProfile;
