using System.Collections.Generic;
using System.Threading.Tasks;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Domain.Interfaces
{
    /// <summary>
    /// Domain service interface for assembly operations
    /// </summary>
    public interface IAssemblyService
    {
        /// <summary>
        /// Creates a new assembly from parts
        /// </summary>
        Task<Assembly> CreateAssemblyAsync(string name, IEnumerable<Part> parts, string description = null);

        /// <summary>
        /// Validates assembly structure and constraints
        /// </summary>
        Task<AssemblyValidationResult> ValidateAssemblyAsync(Assembly assembly);

        /// <summary>
        /// Merges two assemblies into one
        /// </summary>
        Task<Assembly> MergeAssembliesAsync(Assembly assembly1, Assembly assembly2, string mergedName);

        /// <summary>
        /// Splits an assembly into multiple sub-assemblies
        /// </summary>
        Task<IEnumerable<Assembly>> SplitAssemblyAsync(Assembly assembly, IEnumerable<IEnumerable<Part>> partGroups);

        /// <summary>
        /// Calculates assembly properties (mass, center of mass, etc.)
        /// </summary>
        Task<AssemblyProperties> CalculatePropertiesAsync(Assembly assembly);

        /// <summary>
        /// Finds potential collision points within the assembly
        /// </summary>
        Task<IEnumerable<CollisionInfo>> DetectCollisionsAsync(Assembly assembly);

        /// <summary>
        /// Optimizes part placement for manufacturing or assembly
        /// </summary>
        Task<Assembly> OptimizeLayoutAsync(Assembly assembly);
    }

    /// <summary>
    /// Result of assembly validation
    /// </summary>
    public class AssemblyValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();
    }

    /// <summary>
    /// Calculated properties of an assembly
    /// </summary>
    public class AssemblyProperties
    {
        public double TotalMass { get; set; }
        public Rhino.Geometry.Point3d CenterOfMass { get; set; }
        public Rhino.Geometry.Vector3d PrincipalAxes { get; set; }
        public Rhino.Geometry.BoundingBox BoundingBox { get; set; }
        public double Volume { get; set; }
        public double SurfaceArea { get; set; }
    }

    /// <summary>
    /// Collision information between parts
    /// </summary>
    public class CollisionInfo
    {
        public Part PartA { get; set; }
        public Part PartB { get; set; }
        public Rhino.Geometry.Point3d CollisionPoint { get; set; }
        public double PenetrationDepth { get; set; }
    }
}
