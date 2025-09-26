// 改造目的：抽取接触区契约，避免检测流程直接依赖具体 ContactModel。
// 兼容性注意：保持原有属性名称与结构，外部可通过 Contracts.ContactZone 使用。
using Rhino.Geometry;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Defines the geometric region where a contact occurs.
    /// </summary>
    public record ContactZone
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactZone"/> record.
        /// </summary>
        /// <param name="geometry">The Rhino geometry describing the contact support.</param>
        /// <param name="area">Measured surface area.</param>
        /// <param name="length">Measured linear extent when the zone is an edge.</param>
        /// <param name="volume">Measured volume for volumetric contacts.</param>
        public ContactZone(GeometryBase geometry, double area = 0.0, double length = 0.0, double volume = 0.0)
        {
            Geometry = geometry;
            Area = area;
            Length = length;
            Volume = volume;
        }

        /// <summary>
        /// Gets the geometry describing the zone.
        /// </summary>
        public GeometryBase Geometry { get; }

        /// <summary>
        /// Gets the contact surface area.
        /// </summary>
        public double Area { get; }

        /// <summary>
        /// Gets the contact length if edge-like.
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Gets the contact volume for volumetric descriptions.
        /// </summary>
        public double Volume { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"ContactZone(Area={Area:F4}, Length={Length:F4}, Volume={Volume:F4})";
        }
    }
}
