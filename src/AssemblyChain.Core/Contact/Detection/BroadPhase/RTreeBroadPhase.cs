using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Contact.Detection.BroadPhase
{
    /// <summary>
    /// R-Tree based broad phase collision detection.
    /// Uses spatial indexing for efficient neighbor finding.
    /// </summary>
    public static class RTreeBroadPhase
    {
        /// <summary>
        /// R-Tree options.
        /// </summary>
        public class RTreeOptions
        {
            public double ExpansionFactor { get; set; } = 1.1;
            public int MaxLeafSize { get; set; } = 4;
            public bool UseAabb { get; set; } = true;
        }

        /// <summary>
        /// Result of R-Tree broad phase.
        /// </summary>
        public class RTreeResult
        {
            public List<(int i, int j)> CandidatePairs { get; set; } = new List<(int, int)>();
            public TimeSpan ExecutionTime { get; set; }
            public int TotalPairs { get; set; }
            public double ReductionRatio { get; set; }
            public int TreeNodes { get; set; }
        }

        /// <summary>
        /// Performs R-Tree based broad phase on bounding boxes.
        /// </summary>
        public static RTreeResult Execute(IReadOnlyList<BoundingBox> boundingBoxes, RTreeOptions options = null)
        {
            options ??= new RTreeOptions();
            var result = new RTreeResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (boundingBoxes == null || boundingBoxes.Count == 0)
                    return result;

                // Create R-Tree from bounding boxes
                var rtree = CreateRTree(boundingBoxes, options);

                // Find candidate pairs using R-Tree queries
                var candidatePairs = new HashSet<(int, int)>();

                for (int i = 0; i < boundingBoxes.Count; i++)
                {
                    var bbox = boundingBoxes[i];
                    if (!bbox.IsValid) continue;

                    // Expand bounding box for query
                    var expandedBbox = ExpandBoundingBox(bbox, options.ExpansionFactor);

                    // Query R-Tree for overlapping objects
                    var overlappingIds = new System.Collections.Generic.List<int>();
                    rtree.Search(expandedBbox, (sender, args) =>
                    {
                        overlappingIds.Add(args.Id);
                    });

                    // Add pairs with objects that come after i (to avoid duplicates)
                    foreach (var j in overlappingIds.Where(id => id > i))
                    {
                        if (!candidatePairs.Contains((i, j)) && !candidatePairs.Contains((j, i)))
                        {
                            candidatePairs.Add((i, j));
                        }
                    }
                }

                result.CandidatePairs = candidatePairs.ToList();
                result.TotalPairs = boundingBoxes.Count * (boundingBoxes.Count - 1) / 2;
                result.ReductionRatio = result.TotalPairs > 0 ?
                    (double)result.CandidatePairs.Count / result.TotalPairs : 0.0;
                result.TreeNodes = rtree.Count;

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                // Log error but don't throw - return empty result
                System.Diagnostics.Debug.WriteLine($"R-Tree broad phase failed: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Creates an R-Tree from bounding boxes.
        /// </summary>
        private static RTree CreateRTree(IReadOnlyList<BoundingBox> boundingBoxes, RTreeOptions options)
        {
            var rtree = new RTree();

            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                var bbox = boundingBoxes[i];
                if (!bbox.IsValid) continue;

                var expandedBbox = options.UseAabb ? bbox : ExpandBoundingBox(bbox, options.ExpansionFactor);
                rtree.Insert(expandedBbox, i);
            }

            return rtree;
        }

        /// <summary>
        /// Expands a bounding box by a factor.
        /// </summary>
        private static BoundingBox ExpandBoundingBox(BoundingBox bbox, double factor)
        {
            if (!bbox.IsValid) return bbox;

            var center = new Point3d(
                (bbox.Min.X + bbox.Max.X) / 2.0,
                (bbox.Min.Y + bbox.Max.Y) / 2.0,
                (bbox.Min.Z + bbox.Max.Z) / 2.0
            );

            var halfSize = new Vector3d(
                (bbox.Max.X - bbox.Min.X) / 2.0 * factor,
                (bbox.Max.Y - bbox.Min.Y) / 2.0 * factor,
                (bbox.Max.Z - bbox.Min.Z) / 2.0 * factor
            );

            return new BoundingBox(
                new Point3d(center.X - halfSize.X, center.Y - halfSize.Y, center.Z - halfSize.Z),
                new Point3d(center.X + halfSize.X, center.Y + halfSize.Y, center.Z + halfSize.Z)
            );
        }

        /// <summary>
        /// Performs R-Tree broad phase on meshes.
        /// </summary>
        public static RTreeResult ExecuteOnMeshes(IReadOnlyList<Rhino.Geometry.Mesh> meshes, RTreeOptions options = null)
        {
            var boundingBoxes = meshes.Select(m => m?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Performs R-Tree broad phase on Breps.
        /// </summary>
        public static RTreeResult ExecuteOnBreps(IReadOnlyList<Rhino.Geometry.Brep> breps, RTreeOptions options = null)
        {
            var boundingBoxes = breps.Select(b => b?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Performs R-Tree broad phase on generic geometry.
        /// </summary>
        public static RTreeResult ExecuteOnGeometry(IReadOnlyList<Rhino.Geometry.GeometryBase> geometries, RTreeOptions options = null)
        {
            var boundingBoxes = geometries.Select(g => g?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Performs R-Tree broad phase with custom bounding box computation.
        /// </summary>
        public static RTreeResult ExecuteWithCustomBoxes(
            IReadOnlyList<Point3d> centers,
            IReadOnlyList<Vector3d> sizes,
            RTreeOptions options = null)
        {
            options ??= new RTreeOptions();

            var boundingBoxes = new List<BoundingBox>();
            for (int i = 0; i < centers.Count && i < sizes.Count; i++)
            {
                var center = centers[i];
                var size = sizes[i];

                var bbox = new BoundingBox(
                    new Point3d(center.X - size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2),
                    new Point3d(center.X + size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2)
                );

                boundingBoxes.Add(bbox);
            }

            return Execute(boundingBoxes, options);
        }
    }
}



