using System;
using System.Collections.Generic;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Domain.Common;
using AssemblyChain.Core.Domain.ValueObjects;

namespace AssemblyChain.Core.Domain.Entities
{
    /// <summary>
    /// Domain entity representing a mechanical part
    /// </summary>
    public class Part : Entity, IPartGeometry
    {
        /// <summary>
        /// Name of the part
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Geometry of the part
        /// </summary>
        public PartGeometry Geometry { get; private set; }

        /// <summary>
        /// Physics properties of the part
        /// </summary>
        public PhysicsProperties Physics { get; private set; }

        /// <summary>
        /// Material properties of the part
        /// </summary>
        public MaterialProperties Material { get; private set; }

        /// <summary>
        /// Additional metadata for the part
        /// </summary>
        public Dictionary<string, object> Metadata { get; private set; }

        /// <summary>
        /// Whether this part has valid geometry
        /// </summary>
        public bool HasValidGeometry => Geometry?.HasValidGeometry ?? false;

        /// <summary>
        /// Whether this part has physics properties
        /// </summary>
        public bool HasPhysics => Physics != null;

        /// <summary>
        /// Creates a new Part with geometry only
        /// </summary>
        public Part(int id, string name, PartGeometry geometry)
            : base(id)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            if (id != geometry.IndexId)
                throw new ArgumentException($"Part ID {id} must match geometry IndexId {geometry.IndexId}", nameof(id));

            Name = name ?? $"Part_{id}";
            Geometry = geometry;
            Physics = PhysicsProperties.Default;
            Material = MaterialProperties.Steel; // Default material
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a new Part with geometry and physics
        /// </summary>
        public Part(int id, string name, PartGeometry geometry, PhysicsProperties physics)
            : base(id)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            if (id != geometry.IndexId)
                throw new ArgumentException($"Part ID {id} must match geometry IndexId {geometry.IndexId}", nameof(id));

            Name = name ?? $"Part_{id}";
            Geometry = geometry;
            Physics = physics ?? PhysicsProperties.Default;
            Material = MaterialProperties.Steel;
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a complete Part with all properties
        /// </summary>
        public Part(int id, string name, PartGeometry geometry, PhysicsProperties physics, MaterialProperties material)
            : base(id)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            if (id != geometry.IndexId)
                throw new ArgumentException($"Part ID {id} must match geometry IndexId {geometry.IndexId}", nameof(id));

            Name = name ?? $"Part_{id}";
            Geometry = geometry;
            Physics = physics ?? PhysicsProperties.Default;
            Material = material ?? MaterialProperties.Steel;
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Updates the part name
        /// </summary>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            Name = name;
        }

        /// <summary>
        /// Updates the geometry
        /// </summary>
        public void UpdateGeometry(PartGeometry geometry)
        {
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        /// <summary>
        /// Updates the physics properties
        /// </summary>
        public void UpdatePhysics(PhysicsProperties physics)
        {
            Physics = physics ?? PhysicsProperties.Default;
        }

        /// <summary>
        /// Updates the material properties
        /// </summary>
        public void UpdateMaterial(MaterialProperties material)
        {
            Material = material ?? MaterialProperties.Steel;
        }

        /// <summary>
        /// Creates a physics-enabled version of this part
        /// </summary>
        public Part WithPhysics(PhysicsProperties physics)
        {
            return new Part(Id, Name, Geometry, physics, Material);
        }

        /// <summary>
        /// Creates a version with different material
        /// </summary>
        public Part WithMaterial(MaterialProperties material)
        {
            return new Part(Id, Name, Geometry, Physics, material);
        }

        /// <summary>
        /// Gets the bounding box of the part
        /// </summary>
        public Rhino.Geometry.BoundingBox BoundingBox => Geometry?.BoundingBox ?? Rhino.Geometry.BoundingBox.Empty;

        /// <summary>
        /// Gets the index ID from the geometry
        /// </summary>
        public int IndexId => Geometry?.IndexId ?? -1;

        /// <summary>
        /// Gets the mesh from the geometry
        /// </summary>
        public Rhino.Geometry.Mesh Mesh => Geometry?.Mesh;

        /// <summary>
        /// Gets the original geometry from the geometry object
        /// </summary>
        public Rhino.Geometry.GeometryBase OriginalGeometry => Geometry?.OriginalGeometry;

        // Placeholder for OriginalGeometryType (will be refined later)
        public string OriginalGeometryType => Geometry?.OriginalGeometryType ?? "Mesh";

        /// <inheritdoc />
        public double FrictionCoefficient => Material?.FrictionCoefficient ?? 0.5;

        /// <inheritdoc />
        public double RestitutionCoefficient => Material?.RestitutionCoefficient ?? 0.1;
    }
}

