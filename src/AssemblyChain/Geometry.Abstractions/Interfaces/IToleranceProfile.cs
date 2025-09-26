namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Describes tolerances used throughout geometry processing pipelines.
/// </summary>
public interface IToleranceProfile
{
    double Linear { get; }

    double Angular { get; }
}
