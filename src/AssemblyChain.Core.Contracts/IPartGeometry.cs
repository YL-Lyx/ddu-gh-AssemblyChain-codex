using Rhino.Geometry;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Abstraction describing geometric and physical properties of a part required by contact detection.
    /// </summary>
    public interface IPartGeometry
    {
        /// <summary>Gets the unique identifier of the part.</summary>
        int Id { get; }

        /// <summary>Gets the stable index identifier used in assembly models.</summary>
        int IndexId { get; }

        /// <summary>Gets the display name of the part.</summary>
        string Name { get; }

        /// <summary>Gets a value indicating whether the part provides valid geometry for processing.</summary>
        bool HasValidGeometry { get; }

        /// <summary>Gets the canonical geometry type name.</summary>
        string OriginalGeometryType { get; }

        /// <summary>Gets the raw geometry associated with the part.</summary>
        GeometryBase? OriginalGeometry { get; }

        /// <summary>Gets the processed mesh representation, when available.</summary>
        Mesh? Mesh { get; }

        /// <summary>Gets the spatial bounds of the part.</summary>
        BoundingBox BoundingBox { get; }

        /// <summary>Gets the friction coefficient used for contact response.</summary>
        double FrictionCoefficient { get; }

        /// <summary>Gets the restitution coefficient used for contact response.</summary>
        double RestitutionCoefficient { get; }
    }
}
