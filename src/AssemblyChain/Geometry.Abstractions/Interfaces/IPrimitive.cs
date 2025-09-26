using AssemblyChain.Geometry.Abstractions.Primitives;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Marker interface for geometry primitives that can provide bounding volume information.
/// </summary>
public interface IPrimitive
{
    /// <summary>
    /// Gets the axis-aligned bounding box that encloses the primitive.
    /// </summary>
    IAABB GetBounds();
}
