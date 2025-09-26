using AssemblyChain.Geometry.Abstractions.Primitives;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Describes an axis-aligned bounding box.
/// </summary>
public interface IAABB
{
    /// <summary>
    /// Gets the minimum corner of the bounding box.
    /// </summary>
    Point3 Min { get; }

    /// <summary>
    /// Gets the maximum corner of the bounding box.
    /// </summary>
    Point3 Max { get; }

    /// <summary>
    /// Expands the bounding box by the specified tolerance.
    /// </summary>
    /// <param name="tolerance">The tolerance profile used for expansion.</param>
    IAABB Expand(IToleranceProfile tolerance);
}
