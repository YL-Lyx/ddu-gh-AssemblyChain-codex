using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Domain.Common;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Domain.Entities
{
    /// <summary>
    /// Domain entity representing a mechanical assembly
    /// </summary>
    public class Assembly : Entity
    {
        /// <summary>
        /// Name of the assembly
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of the assembly
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Parts that make up this assembly
        /// </summary>
        private readonly List<Part> _parts = new();
        public IReadOnlyList<Part> Parts => _parts.AsReadOnly();

        /// <summary>
        /// Sub-assemblies within this assembly
        /// </summary>
        private readonly List<Assembly> _subAssemblies = new();
        public IReadOnlyList<Assembly> SubAssemblies => _subAssemblies.AsReadOnly();

        /// <summary>
        /// Whether this assembly contains any geometric parts
        /// </summary>
        public bool HasGeometry => _parts.Any(p => p.HasValidGeometry) ||
                                   _subAssemblies.Any(sa => sa.HasGeometry);

        /// <summary>
        /// Whether this assembly contains any physics-enabled parts
        /// </summary>
        public bool HasPhysics => _parts.Any(p => p.HasPhysics) ||
                                  _subAssemblies.Any(sa => sa.HasPhysics);

        /// <summary>
        /// Total number of parts in the assembly (including sub-assemblies)
        /// </summary>
        public int TotalPartCount => _parts.Count + _subAssemblies.Sum(sa => sa.TotalPartCount);

        /// <summary>
        /// Bounding box of the entire assembly
        /// </summary>
        public Rhino.Geometry.BoundingBox BoundingBox
        {
            get
            {
                var allParts = GetAllParts();
                if (!allParts.Any())
                    return Rhino.Geometry.BoundingBox.Empty;

                var bbox = Rhino.Geometry.BoundingBox.Empty;
                bool initialized = false;

                foreach (var part in allParts.Where(p => p.HasValidGeometry))
                {
                    var partBbox = part.BoundingBox;
                    if (!initialized)
                    {
                        bbox = partBbox;
                        initialized = true;
                    }
                    else
                    {
                        bbox.Union(partBbox);
                    }
                }

                return bbox;
            }
        }

        /// <summary>
        /// Creates a new Assembly
        /// </summary>
        public Assembly(int id, string name, string description = null)
            : base(id)
        {
            Name = name ?? $"Assembly_{id}";
            Description = description;
        }

        /// <summary>
        /// Updates the assembly name
        /// </summary>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            Name = name;
        }

        /// <summary>
        /// Updates the assembly description
        /// </summary>
        public void UpdateDescription(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Adds a part to the assembly
        /// </summary>
        public void AddPart(Part part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            _parts.Add(part);
        }

        /// <summary>
        /// Removes a part from the assembly
        /// </summary>
        public bool RemovePart(Part part)
        {
            return _parts.Remove(part);
        }

        /// <summary>
        /// Removes a part by ID
        /// </summary>
        public bool RemovePart(int partId)
        {
            var part = _parts.FirstOrDefault(p => p.Id == partId);
            return part != null && _parts.Remove(part);
        }

        /// <summary>
        /// Adds a sub-assembly
        /// </summary>
        public void AddSubAssembly(Assembly subAssembly)
        {
            if (subAssembly == null) throw new ArgumentNullException(nameof(subAssembly));
            if (subAssembly.Id == Id) throw new ArgumentException("Cannot add self as sub-assembly");

            _subAssemblies.Add(subAssembly);
        }

        /// <summary>
        /// Removes a sub-assembly
        /// </summary>
        public bool RemoveSubAssembly(Assembly subAssembly)
        {
            return _subAssemblies.Remove(subAssembly);
        }

        /// <summary>
        /// Gets a part by ID (searches recursively in sub-assemblies)
        /// </summary>
        public Part GetPart(int partId)
        {
            // Search in direct parts
            var part = _parts.FirstOrDefault(p => p.Id == partId);
            if (part != null) return part;

            // Search in sub-assemblies
            foreach (var subAssembly in _subAssemblies)
            {
                part = subAssembly.GetPart(partId);
                if (part != null) return part;
            }

            return null;
        }

        /// <summary>
        /// Gets all parts recursively (including from sub-assemblies)
        /// </summary>
        public IEnumerable<Part> GetAllParts()
        {
            foreach (var part in _parts)
                yield return part;

            foreach (var subAssembly in _subAssemblies)
            {
                foreach (var part in subAssembly.GetAllParts())
                    yield return part;
            }
        }

        /// <summary>
        /// Gets all parts with physics properties
        /// </summary>
        public IEnumerable<Part> GetPhysicsParts()
        {
            return GetAllParts().Where(p => p.HasPhysics);
        }

        /// <summary>
        /// Validates the assembly structure
        /// </summary>
        public bool IsValid()
        {
            // Check for duplicate part IDs
            var allPartIds = GetAllParts().Select(p => p.Id).ToList();
            if (allPartIds.Count != allPartIds.Distinct().Count())
                return false;

            // Check for circular references in sub-assemblies
            return !HasCircularReference(new HashSet<int> { Id });
        }

        private bool HasCircularReference(HashSet<int> visitedAssemblyIds)
        {
            foreach (var subAssembly in _subAssemblies)
            {
                if (visitedAssemblyIds.Contains(subAssembly.Id))
                    return true;

                visitedAssemblyIds.Add(subAssembly.Id);
                if (subAssembly.HasCircularReference(visitedAssemblyIds))
                    return true;
                visitedAssemblyIds.Remove(subAssembly.Id);
            }
            return false;
        }
    }
}



