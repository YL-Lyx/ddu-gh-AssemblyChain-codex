using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Brep
{
    /// <summary>
    /// Partial class hosting supporting types for planar operations.
    /// </summary>
    public static partial class PlanarOps
    {
        /// <summary>
        /// Options for planar operations.
        /// </summary>
        public class PlanarOptions
        {
            /// <summary>
            /// Gets or sets the tolerance used to consider faces coplanar.
            /// </summary>
            public double CoplanarTolerance { get; set; } = 1e-3;

            /// <summary>
            /// Gets or sets the minimum face area to keep.
            /// </summary>
            public double AreaTolerance { get; set; } = 1e-6;

            /// <summary>
            /// Gets or sets a value indicating whether to merge coplanar faces.
            /// </summary>
            public bool MergeCoplanarFaces { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether to extract planar faces.
            /// </summary>
            public bool ExtractPlanarFaces { get; set; } = true;

            /// <summary>
            /// Gets or sets the minimum number of faces required per plane bucket.
            /// </summary>
            public int MinFaceCount { get; set; } = 1;
        }

        /// <summary>
        /// Result of planar operations.
        /// </summary>
        public class PlanarResult
        {
            /// <summary>
            /// Gets or sets grouped planar faces per plane.
            /// </summary>
            public Dictionary<Plane, List<BrepFace>> PlanarFaces { get; set; } = new();

            /// <summary>
            /// Gets or sets the list of unique planes.
            /// </summary>
            public List<Plane> Planes { get; set; } = new();

            /// <summary>
            /// Gets or sets a value indicating whether the operation succeeded.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets warning messages.
            /// </summary>
            public List<string> Warnings { get; set; } = new();

            /// <summary>
            /// Gets or sets error messages.
            /// </summary>
            public List<string> Errors { get; set; } = new();
        }
    }
}
