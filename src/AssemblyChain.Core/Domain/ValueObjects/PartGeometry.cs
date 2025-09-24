using System;
using System.Collections.Generic;
using AssemblyChain.Core.Domain.Common;
using Rhino.Geometry;

namespace AssemblyChain.Core.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing the geometric data of a part
    /// </summary>
    public class PartGeometry : ValueObject
    {
        /// <summary>
        /// Unique identifier for this part geometry
        /// </summary>
        public int IndexId { get; }

        /// <summary>
        /// The mesh geometry representing this part
        /// </summary>
        public Mesh Mesh { get; }

        /// <summary>
        /// Name of the part geometry
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Original geometry before any processing
        /// </summary>
        public GeometryBase OriginalGeometry { get; set; }

        /// <summary>
        /// Type of the original geometry (Mesh, Brep, etc.)
        /// </summary>
        public string OriginalGeometryType { get; set; }

        /// <summary>
        /// Additional metadata for the part
        /// </summary>
        public Dictionary<string, object> Metadata { get; }

        /// <summary>
        /// Whether this part has valid geometry
        /// </summary>
        public bool HasValidGeometry => Mesh != null && Mesh.IsValid && Mesh.Vertices.Count > 0;

        /// <summary>
        /// Creates a new PartGeometry
        /// </summary>
        public PartGeometry(int indexId, Mesh mesh)
        {
            IndexId = indexId;
            Mesh = mesh?.DuplicateMesh() ?? throw new ArgumentNullException(nameof(mesh));
            Name = $"PartGeometry_{indexId}";
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a PartGeometry with custom properties
        /// </summary>
        public PartGeometry(int indexId, Mesh mesh, string name, GeometryBase originalGeometry, string originalGeometryType)
        {
            IndexId = indexId;
            Mesh = mesh?.DuplicateMesh() ?? throw new ArgumentNullException(nameof(mesh));
            Name = name ?? $"PartGeometry_{indexId}";
            OriginalGeometry = originalGeometry;
            OriginalGeometryType = originalGeometryType ?? "Mesh";
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the bounding box of the geometry
        /// </summary>
        public BoundingBox BoundingBox => Mesh.GetBoundingBox(false);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Mesh; // Note: This might not be ideal for large meshes, but follows value object semantics
            yield return OriginalGeometryType;
        }
    }
}


