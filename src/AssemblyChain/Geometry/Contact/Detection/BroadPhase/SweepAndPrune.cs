using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Planning.Model;

namespace AssemblyChain.Geometry.Contact.Detection.BroadPhase
{
    /// <summary>
    /// Factory for creating broad phase collision detection algorithms.
    /// </summary>
    public static class BroadPhaseFactory
    {
        /// <summary>
        /// Creates a broad phase algorithm based on the specified type.
        /// </summary>
        public static IBroadPhase Create(string algorithmType)
        {
            return algorithmType.ToLowerInvariant() switch
            {
                "sap" or "sweepandprune" => new SweepAndPruneAlgorithm(),
                "rtree" => new RTreeAlgorithm(),
                _ => new SweepAndPruneAlgorithm() // Default
            };
        }
    }

    /// <summary>
    /// Interface for broad phase collision detection algorithms.
    /// </summary>
    public interface IBroadPhase
    {
        /// <summary>
        /// Gets candidate pairs that might be in collision.
        /// </summary>
        List<(int i, int j)> GetCandidatePairs(IReadOnlyList<Part> parts, DetectionOptions options);
    }

    /// <summary>
    /// Sweep and Prune broad phase implementation.
    /// </summary>
    public class SweepAndPruneAlgorithm : IBroadPhase
    {
        public List<(int i, int j)> GetCandidatePairs(IReadOnlyList<Part> parts, DetectionOptions options)
        {
            return SweepAndPrune.GetCandidatePairs(parts, options);
        }
    }

    /// <summary>
    /// R-Tree broad phase implementation (placeholder).
    /// </summary>
    public class RTreeAlgorithm : IBroadPhase
    {
        public List<(int i, int j)> GetCandidatePairs(IReadOnlyList<Part> parts, DetectionOptions options)
        {
            // Placeholder - would implement R-Tree based broad phase
            // For now, return all possible pairs
            var pairs = new List<(int i, int j)>();
            for (int i = 0; i < parts.Count; i++)
            {
                for (int j = i + 1; j < parts.Count; j++)
                {
                    pairs.Add((i, j));
                }
            }
            return pairs;
        }
    }

    /// <summary>
    /// Sweep and Prune broad phase collision detection algorithm.
    /// Efficiently finds potentially colliding pairs using sorted axis projections.
    /// </summary>
    public static class SweepAndPrune
    {
        /// <summary>
        /// SAP options.
        /// </summary>
        public class SapOptions
        {
            public int Axis { get; set; } = 0; // 0=X, 1=Y, 2=Z
            public double ExpansionFactor { get; set; } = 1.1;
            public bool UseAabb { get; set; } = true;
        }

        /// <summary>
        /// Result of SAP broad phase.
        /// </summary>
        public class SapResult
        {
            public List<(int i, int j)> CandidatePairs { get; set; } = new List<(int, int)>();
            public TimeSpan ExecutionTime { get; set; }
            public int TotalPairs { get; set; }
            public double ReductionRatio { get; set; }
        }

        /// <summary>
        /// Performs sweep and prune on a set of bounding boxes.
        /// </summary>
        public static SapResult Execute(IReadOnlyList<BoundingBox> boundingBoxes, SapOptions options = null)
        {
            options ??= new SapOptions();
            var result = new SapResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (boundingBoxes == null || boundingBoxes.Count == 0)
                {
                    return result;
                }

                // Create endpoint list for the chosen axis
                var endpoints = CreateEndpoints(boundingBoxes, options.Axis, options.ExpansionFactor);

                // Sort endpoints
                endpoints.Sort((a, b) => a.Position.CompareTo(b.Position));

                // Sweep and find overlapping pairs
                var activeObjects = new HashSet<int>();
                var candidatePairs = new HashSet<(int, int)>();

                foreach (var endpoint in endpoints)
                {
                    if (endpoint.IsStart)
                    {
                        // Object starts here - check against all active objects
                        foreach (var activeId in activeObjects)
                        {
                            // For symmetry, only add pairs where smaller id comes first
                            var pair = activeId < endpoint.ObjectId ?
                                (activeId, endpoint.ObjectId) :
                                (endpoint.ObjectId, activeId);
                            candidatePairs.Add(pair);
                        }

                        // Add this object to active set
                        activeObjects.Add(endpoint.ObjectId);
                    }
                    else
                    {
                        // Object ends here - remove from active set
                        activeObjects.Remove(endpoint.ObjectId);
                    }
                }

                result.CandidatePairs = candidatePairs.ToList();
                result.TotalPairs = boundingBoxes.Count * (boundingBoxes.Count - 1) / 2;
                result.ReductionRatio = result.TotalPairs > 0 ?
                    (double)result.CandidatePairs.Count / result.TotalPairs : 0.0;

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                // Log error but don't throw - return empty result
                System.Diagnostics.Debug.WriteLine($"SAP failed: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Creates endpoints for SAP algorithm.
        /// </summary>
        private static List<Endpoint> CreateEndpoints(IReadOnlyList<BoundingBox> boundingBoxes, int axis, double expansionFactor)
        {
            var endpoints = new List<Endpoint>();

            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                var bbox = boundingBoxes[i];
                if (!bbox.IsValid) continue;

                double min, max;

                switch (axis)
                {
                    case 0: // X-axis
                        min = bbox.Min.X;
                        max = bbox.Max.X;
                        break;
                    case 1: // Y-axis
                        min = bbox.Min.Y;
                        max = bbox.Max.Y;
                        break;
                    case 2: // Z-axis
                        min = bbox.Min.Z;
                        max = bbox.Max.Z;
                        break;
                    default:
                        min = bbox.Min.X;
                        max = bbox.Max.X;
                        break;
                }

                // Apply expansion factor
                var center = (min + max) / 2.0;
                var halfSize = (max - min) / 2.0 * expansionFactor;
                min = center - halfSize;
                max = center + halfSize;

                endpoints.Add(new Endpoint { Position = min, ObjectId = i, IsStart = true });
                endpoints.Add(new Endpoint { Position = max, ObjectId = i, IsStart = false });
            }

            return endpoints;
        }

        /// <summary>
        /// Endpoint structure for SAP.
        /// </summary>
        private struct Endpoint
        {
            public double Position;
            public int ObjectId;
            public bool IsStart;
        }

        /// <summary>
        /// Performs SAP on meshes using their bounding boxes.
        /// </summary>
        public static SapResult ExecuteOnMeshes(IReadOnlyList<Rhino.Geometry.Mesh> meshes, SapOptions options = null)
        {
            var boundingBoxes = meshes.Select(m => m?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Performs SAP on Breps using their bounding boxes.
        /// </summary>
        public static SapResult ExecuteOnBreps(IReadOnlyList<Rhino.Geometry.Brep> breps, SapOptions options = null)
        {
            var boundingBoxes = breps.Select(b => b?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Performs SAP on generic geometry using their bounding boxes.
        /// </summary>
        public static SapResult ExecuteOnGeometry(IReadOnlyList<Rhino.Geometry.GeometryBase> geometries, SapOptions options = null)
        {
            var boundingBoxes = geometries.Select(g => g?.GetBoundingBox(true) ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Performs SAP on parts using their bounding boxes.
        /// </summary>
        public static SapResult ExecuteOnParts(IReadOnlyList<Part> parts, SapOptions options = null)
        {
            var boundingBoxes = parts.Select(p => p?.BoundingBox ?? BoundingBox.Empty).ToList();
            return Execute(boundingBoxes, options);
        }

        /// <summary>
        /// Gets candidate pairs that might be in collision using Sweep and Prune.
        /// </summary>
        public static List<(int i, int j)> GetCandidatePairs(IReadOnlyList<Part> parts, DetectionOptions options)
        {
            var result = ExecuteOnParts(parts, new SapOptions { Axis = 0 }); // Default to X-axis
            return result.CandidatePairs;
        }
    }
}




