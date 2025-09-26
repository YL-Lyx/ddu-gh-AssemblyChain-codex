using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Domain.Entities;
using Rhino.Geometry;

namespace AssemblyChain.Planning.Model
{
    /// <summary>
    /// Read-only assembly model.
    /// Contains parts, bounding box, index mapping, and version hash for caching.
    /// </summary>
    public sealed class AssemblyModel
    {
        /// <summary>
        /// The parts that make up this assembly.
        /// </summary>
        public IReadOnlyList<Part> Parts { get; }

        /// <summary>
        /// The bounding box of the entire assembly.
        /// </summary>
        public BoundingBox BoundingBox { get; }

        /// <summary>
        /// Mapping from part index IDs to their positions in the Parts list.
        /// </summary>
        public IReadOnlyDictionary<int, int> IndexToPosition { get; }

        /// <summary>
        /// Version hash for caching and change detection.
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Assembly name/identifier.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Total number of parts in the assembly.
        /// </summary>
        public int PartCount => Parts.Count;

        /// <summary>
        /// Whether the assembly has valid geometry.
        /// </summary>
        public bool HasValidGeometry => Parts.All(p => p.HasValidGeometry);

        internal AssemblyModel(IReadOnlyList<Part> parts, string name, string hash)
        {
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));
            Name = string.IsNullOrWhiteSpace(name) ? $"Assembly_{Guid.NewGuid():N}" : name;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));

            // Calculate bounding box
            var bbox = BoundingBox.Empty;
            bool initialized = false;
            foreach (var part in Parts)
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
            BoundingBox = bbox;

            // Build index mapping
            var indexToPosition = new Dictionary<int, int>(Parts.Count);
            for (int i = 0; i < Parts.Count; i++)
            {
                indexToPosition[Parts[i].IndexId] = i;
            }
            IndexToPosition = indexToPosition;
        }
    }
}



