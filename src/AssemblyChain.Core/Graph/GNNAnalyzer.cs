using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Graph
{
    /// <summary>
    /// GNN-like (Graph Neural Network) analyzer for disassembly planning
    /// Implements message passing on factor graphs to compute part scores and affinities
    /// </summary>
    public sealed class GNNAnalyzer
    {
        private readonly int _maxIterations = 10;
        private readonly double _convergenceThreshold = 1e-3;
        private readonly double _dampingFactor = 0.8; // Message damping
        private readonly int _maxNeighborsPerNode = 16; // Sparse: limit neighbors per node
        private readonly bool _useSparseMode = true; // Enable sparse computation

        /// <summary>
        /// Run GNN-style message passing to compute part scores and pair affinities
        /// </summary>
        public GNNAnalysisResult Analyze(IReadOnlyList<Part> parts,
                                       IEnumerable<DirectionalBlockingGraph> dbgs,
                                       IEnumerable<int> activePartIds)
        {
            var activeParts = activePartIds.ToList();
            var result = new GNNAnalysisResult(activeParts);

            // Initialize node features
            var nodeFeatures = InitializeNodeFeatures(parts, dbgs, activeParts);

            // Message passing iterations
            for (int iter = 0; iter < _maxIterations; iter++)
            {
                var newFeatures = UpdateNodeFeatures(nodeFeatures, dbgs, activeParts);

                // Check convergence
                if (HasConverged(nodeFeatures, newFeatures))
                {
                    break;
                }

                nodeFeatures = newFeatures;
            }

            // Compute final scores
            result.SingleScores = ComputeSingleScores(nodeFeatures, activeParts);
            result.PairAffinities = ComputePairAffinities(nodeFeatures, dbgs, activeParts);

            return result;
        }

        /// <summary>
        /// Incrementally update analysis after part removal (sparse optimization)
        /// </summary>
        public GNNAnalysisResult UpdateAnalysis(GNNAnalysisResult previousResult,
                                              IReadOnlyList<Part> parts,
                                              IEnumerable<DirectionalBlockingGraph> dbgs,
                                              IEnumerable<int> removedPartIds)
        {
            var activeParts = previousResult.ActivePartIds
                .Where(id => !removedPartIds.Contains(id))
                .ToList();

            var result = new GNNAnalysisResult(activeParts);

            // Incremental update: only recompute affected nodes
            var affectedNodes = new HashSet<int>(removedPartIds);

            // Find nodes that were connected to removed nodes
            foreach (var removedId in removedPartIds)
            {
                // Add all previous neighbors of removed nodes to affected set
                foreach (var kvp in previousResult.PairAffinities)
                {
                    if (kvp.Key.Item1 == removedId)
                        affectedNodes.Add(kvp.Key.Item2);
                    else if (kvp.Key.Item2 == removedId)
                        affectedNodes.Add(kvp.Key.Item1);
                }
            }

            // Keep scores for unaffected nodes
            foreach (var partId in activeParts)
            {
                if (!affectedNodes.Contains(partId) && previousResult.SingleScores.TryGetValue(partId, out var score))
                {
                    result.SingleScores[partId] = score;
                }
            }

            // Recompute features only for affected nodes
            var nodeFeatures = InitializeNodeFeatures(parts, dbgs, activeParts);

            // Quick message passing only for affected nodes (1-2 iterations)
            for (int iter = 0; iter < 2; iter++)
            {
                var newFeatures = UpdateNodeFeaturesIncrementally(nodeFeatures, dbgs, activeParts, affectedNodes);
                nodeFeatures = newFeatures;
            }

            // Compute scores for affected nodes
            foreach (var partId in affectedNodes.Where(id => activeParts.Contains(id)))
            {
                if (nodeFeatures.TryGetValue(partId, out var features))
                {
                    result.SingleScores[partId] = ComputeSingleScore(features);
                }
            }

            // Recompute pair affinities (sparse)
            result.PairAffinities = ComputePairAffinities(nodeFeatures, dbgs, activeParts);

            return result;
        }

        /// <summary>
        /// Initialize node features based on geometric and blocking properties
        /// </summary>
        private Dictionary<int, NodeFeatures> InitializeNodeFeatures(
            IReadOnlyList<Part> parts,
            IEnumerable<DirectionalBlockingGraph> dbgs,
            List<int> activeParts)
        {
            var features = new Dictionary<int, NodeFeatures>();

            foreach (var partId in activeParts)
            {
                var part = parts[partId];
                var nodeFeatures = new NodeFeatures();

                // Geometric features
                if (part.Mesh != null)
                {
                    var bbox = part.Mesh.GetBoundingBox(true);
                    nodeFeatures.Volume = bbox.Volume;
                    nodeFeatures.SurfaceArea = CalculateSurfaceArea(part.Mesh);
                    nodeFeatures.BoundingBoxSize = bbox.Diagonal.Length;
                }

                // Blocking features from DBGs
                var totalBlockingScore = 0.0;
                var directionBlockingScores = new List<double>();

                foreach (var dbg in dbgs)
                {
                    var score = dbg.GetBlockingScore(partId);
                    totalBlockingScore += score;
                    directionBlockingScores.Add(score);
                }

                nodeFeatures.TotalBlockingScore = totalBlockingScore;
                nodeFeatures.DirectionBlockingScores = directionBlockingScores;

                // Exposure features (how accessible the part is)
                nodeFeatures.ExposureScore = CalculateExposureScore(partId, parts, activeParts);

                features[partId] = nodeFeatures;
            }

            return features;
        }

        /// <summary>
        /// Update node features through message passing
        /// </summary>
        private Dictionary<int, NodeFeatures> UpdateNodeFeatures(
            Dictionary<int, NodeFeatures> currentFeatures,
            IEnumerable<DirectionalBlockingGraph> dbgs,
            List<int> activeParts)
        {
            return UpdateNodeFeaturesIncrementally(currentFeatures, dbgs, activeParts, new HashSet<int>(activeParts));
        }

        /// <summary>
        /// Incrementally update node features for affected nodes only
        /// </summary>
        private Dictionary<int, NodeFeatures> UpdateNodeFeaturesIncrementally(
            Dictionary<int, NodeFeatures> currentFeatures,
            IEnumerable<DirectionalBlockingGraph> dbgs,
            List<int> activeParts,
            HashSet<int> affectedNodes)
        {
            var newFeatures = new Dictionary<int, NodeFeatures>();

            // Copy unaffected features
            foreach (var partId in activeParts)
            {
                if (!affectedNodes.Contains(partId))
                {
                    newFeatures[partId] = currentFeatures[partId].Clone();
                }
            }

            // Update affected nodes
            foreach (var partId in affectedNodes.Where(id => activeParts.Contains(id)))
            {
                var currentFeature = currentFeatures[partId];
                var newFeature = currentFeature.Clone();

                // Aggregate messages from neighbors in each DBG
                var neighborMessages = new List<double>();

                foreach (var dbg in dbgs)
                {
                    // Messages from parts this part blocks
                    foreach (var blockedId in dbg.GetBlockedParts(partId))
                    {
                        if (activeParts.Contains(blockedId))
                        {
                            var neighborFeature = currentFeatures[blockedId];
                            var message = CalculateMessage(currentFeature, neighborFeature, "blocking");
                            neighborMessages.Add(message);
                        }
                    }

                    // Messages from parts that block this part
                    foreach (var blockerId in dbg.GetBlockers(partId))
                    {
                        if (activeParts.Contains(blockerId))
                        {
                            var neighborFeature = currentFeatures[blockerId];
                            var message = CalculateMessage(currentFeature, neighborFeature, "blocked");
                            neighborMessages.Add(message);
                        }
                    }
                }

                // Update features based on aggregated messages
                if (neighborMessages.Count > 0)
                {
                    var avgMessage = neighborMessages.Average();
                    newFeature.MessageAggregate = _dampingFactor * currentFeature.MessageAggregate +
                                                (1 - _dampingFactor) * avgMessage;
                }

                newFeatures[partId] = newFeature;
            }

            return newFeatures;
        }

        /// <summary>
        /// Calculate message between two nodes
        /// </summary>
        private double CalculateMessage(NodeFeatures from, NodeFeatures to, string relationType)
        {
            // Simple message calculation based on feature differences
            var volumeDiff = Math.Abs(from.Volume - to.Volume) / Math.Max(Math.Max(from.Volume, to.Volume), 1.0);
            var sizeDiff = Math.Abs(from.BoundingBoxSize - to.BoundingBoxSize) /
                          Math.Max(Math.Max(from.BoundingBoxSize, to.BoundingBoxSize), 1.0);

            var similarity = 1.0 - (volumeDiff + sizeDiff) / 2.0;

            // Blocking relationship influences message
            var blockingInfluence = relationType == "blocking" ? 0.8 : 0.6;

            return similarity * blockingInfluence;
        }

        /// <summary>
        /// Check if features have converged
        /// </summary>
        private bool HasConverged(Dictionary<int, NodeFeatures> oldFeatures,
                                Dictionary<int, NodeFeatures> newFeatures)
        {
            foreach (var partId in newFeatures.Keys)
            {
                var oldFeature = oldFeatures[partId];
                var newFeature = newFeatures[partId];

                if (Math.Abs(oldFeature.MessageAggregate - newFeature.MessageAggregate) > _convergenceThreshold)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compute single part removal scores
        /// </summary>
        private Dictionary<int, double> ComputeSingleScores(
            Dictionary<int, NodeFeatures> features, List<int> activeParts)
        {
            var scores = new Dictionary<int, double>();

            foreach (var partId in activeParts)
            {
                var feature = features[partId];
                scores[partId] = ComputeSingleScore(feature);
            }

            return scores;
        }

        /// <summary>
        /// Compute single score for one part
        /// </summary>
        private double ComputeSingleScore(NodeFeatures feature)
        {
            // Score based on exposure, blocking, and message passing results
            var exposureScore = feature.ExposureScore;
            var blockingPenalty = Math.Min(feature.TotalBlockingScore * 0.1, 1.0);
            var messageScore = feature.MessageAggregate;

            // Higher score = more likely to be removable alone
            var score = exposureScore * (1.0 - blockingPenalty) + messageScore * 0.3;
            return Math.Max(0, Math.Min(1, score));
        }

        /// <summary>
        /// Compute pair-wise affinities between parts (sparse version)
        /// </summary>
        private Dictionary<(int, int), double> ComputePairAffinities(
            Dictionary<int, NodeFeatures> features,
            IEnumerable<DirectionalBlockingGraph> dbgs,
            List<int> activeParts)
        {
            var affinities = new Dictionary<(int, int), double>();

            if (_useSparseMode)
            {
                // Sparse mode: only compute affinities for promising pairs
                var allPairs = new List<((int, int) parts, double affinity)>();

                // First pass: compute all pairwise affinities
                for (int i = 0; i < activeParts.Count; i++)
                {
                    for (int j = i + 1; j < activeParts.Count; j++)
                    {
                        var partA = activeParts[i];
                        var partB = activeParts[j];
                        var affinity = CalculatePairAffinity(partA, partB, features, dbgs);
                        allPairs.Add(((partA, partB), affinity));
                    }
                }

                // Second pass: keep only top-K affinities per node
                var nodeAffinities = new Dictionary<int, List<(int other, double affinity)>>();

                foreach (var (parts, affinity) in allPairs)
                {
                    var (partA, partB) = parts;

                    // Add to partA's neighbors
                    if (!nodeAffinities.ContainsKey(partA))
                        nodeAffinities[partA] = new List<(int, double)>();
                    nodeAffinities[partA].Add((partB, affinity));

                    // Add to partB's neighbors
                    if (!nodeAffinities.ContainsKey(partB))
                        nodeAffinities[partB] = new List<(int, double)>();
                    nodeAffinities[partB].Add((partA, affinity));
                }

                // Keep only top-K neighbors per node
                foreach (var kvp in nodeAffinities)
                {
                    var topNeighbors = kvp.Value
                        .OrderByDescending(x => x.affinity)
                        .Take(_maxNeighborsPerNode);

                    foreach (var (other, affinity) in topNeighbors)
                    {
                        var key = kvp.Key < other ? (kvp.Key, other) : (other, kvp.Key);
                        affinities[key] = affinity;
                    }
                }
            }
            else
            {
                // Dense mode: compute all pairwise affinities (original implementation)
                for (int i = 0; i < activeParts.Count; i++)
                {
                    for (int j = i + 1; j < activeParts.Count; j++)
                    {
                        var partA = activeParts[i];
                        var partB = activeParts[j];

                        var affinity = CalculatePairAffinity(partA, partB, features, dbgs);
                        affinities[(partA, partB)] = affinity;
                        affinities[(partB, partA)] = affinity; // Symmetric
                    }
                }
            }

            return affinities;
        }

        /// <summary>
        /// Calculate affinity between two parts
        /// </summary>
        private double CalculatePairAffinity(int partA, int partB,
                                          Dictionary<int, NodeFeatures> features,
                                          IEnumerable<DirectionalBlockingGraph> dbgs)
        {
            var featureA = features[partA];
            var featureB = features[partB];

            // Feature similarity
            var volumeSimilarity = 1.0 - Math.Abs(featureA.Volume - featureB.Volume) /
                                         Math.Max(Math.Max(featureA.Volume, featureB.Volume), 1.0);
            var sizeSimilarity = 1.0 - Math.Abs(featureA.BoundingBoxSize - featureB.BoundingBoxSize) /
                                       Math.Max(Math.Max(featureA.BoundingBoxSize, featureB.BoundingBoxSize), 1.0);

            // Mutual blocking relationships
            var mutualBlocking = 0.0;
            foreach (var dbg in dbgs)
            {
                var aBlocksB = dbg.GetBlockedParts(partA).Contains(partB);
                var bBlocksA = dbg.GetBlockedParts(partB).Contains(partA);

                if (aBlocksB && bBlocksA)
                {
                    mutualBlocking += 1.0; // Strong mutual blocking
                }
                else if (aBlocksB || bBlocksA)
                {
                    mutualBlocking += 0.5; // One-way blocking
                }
            }

            // Message passing correlation
            var messageCorrelation = 1.0 - Math.Abs(featureA.MessageAggregate - featureB.MessageAggregate);

            // Combine factors
            var affinity = (volumeSimilarity + sizeSimilarity) / 2.0 * 0.4 +
                          mutualBlocking / dbgs.Count() * 0.4 +
                          messageCorrelation * 0.2;

            return Math.Max(0, Math.Min(1, affinity));
        }

        /// <summary>
        /// Calculate surface area of a mesh (simplified)
        /// </summary>
        private double CalculateSurfaceArea(Rhino.Geometry.Mesh mesh)
        {
            if (mesh == null) return 0;

            double totalArea = 0;
            foreach (var face in mesh.Faces)
            {
                var p0 = mesh.Vertices[face.A];
                var p1 = mesh.Vertices[face.B];
                var p2 = mesh.Vertices[face.C];

                var v1 = p1 - p0;
                var v2 = p2 - p0;
                var cross = Vector3d.CrossProduct(v1, v2);
                totalArea += cross.Length / 2.0;

                // Handle quad faces
                if (face.IsQuad)
                {
                    var p3 = mesh.Vertices[face.D];
                    v1 = p3 - p0;
                    v2 = p2 - p0;
                    cross = Vector3d.CrossProduct(v1, v2);
                    totalArea += cross.Length / 2.0;
                }
            }

            return totalArea;
        }

        /// <summary>
        /// Calculate exposure score (how accessible a part is)
        /// </summary>
        private double CalculateExposureScore(int partId, IReadOnlyList<Part> parts, List<int> activeParts)
        {
            var part = parts[partId];
            if (part.Mesh == null) return 0;

            var bbox = part.Mesh.GetBoundingBox(true);
            var center = bbox.Center;

            // Count how many other parts are "in front" of this part from different directions
            var directions = new[] {
                Vector3d.XAxis, -Vector3d.XAxis,
                Vector3d.YAxis, -Vector3d.YAxis,
                Vector3d.ZAxis, -Vector3d.ZAxis
            };

            var totalObstruction = 0.0;

            foreach (var dir in directions)
            {
                var obstructions = 0;
                var testPoint = center + dir * bbox.Diagonal.Length;

                foreach (var otherId in activeParts)
                {
                    if (otherId == partId) continue;

                    var otherPart = parts[otherId];
                    if (otherPart.Mesh == null) continue;

                    var otherBbox = otherPart.Mesh.GetBoundingBox(true);
                    if (otherBbox.Contains(testPoint))
                    {
                        obstructions++;
                    }
                }

                totalObstruction += Math.Min(obstructions, 3); // Cap at 3
            }

            // Exposure score: higher = more exposed (less obstructed)
            return Math.Max(0, 1.0 - totalObstruction / (directions.Length * 3.0));
        }
    }

    /// <summary>
    /// Node features for GNN message passing
    /// </summary>
    public sealed class NodeFeatures
    {
        public double Volume { get; set; }
        public double SurfaceArea { get; set; }
        public double BoundingBoxSize { get; set; }
        public double TotalBlockingScore { get; set; }
        public List<double> DirectionBlockingScores { get; set; } = new List<double>();
        public double ExposureScore { get; set; }
        public double MessageAggregate { get; set; }

        public NodeFeatures Clone()
        {
            return new NodeFeatures
            {
                Volume = this.Volume,
                SurfaceArea = this.SurfaceArea,
                BoundingBoxSize = this.BoundingBoxSize,
                TotalBlockingScore = this.TotalBlockingScore,
                DirectionBlockingScores = new List<double>(this.DirectionBlockingScores),
                ExposureScore = this.ExposureScore,
                MessageAggregate = this.MessageAggregate
            };
        }
    }

    /// <summary>
    /// Results from GNN analysis
    /// </summary>
    public sealed class GNNAnalysisResult
    {
        public List<int> ActivePartIds { get; }
        public Dictionary<int, double> SingleScores { get; set; } = new Dictionary<int, double>();
        public Dictionary<(int, int), double> PairAffinities { get; set; } = new Dictionary<(int, int), double>();

        public GNNAnalysisResult(List<int> activePartIds)
        {
            ActivePartIds = activePartIds;
        }

        /// <summary>
        /// Get top-k single part candidates by score
        /// </summary>
        public List<(int partId, double score)> GetTopSingleCandidates(int k)
        {
            return SingleScores.OrderByDescending(kvp => kvp.Value)
                              .Take(k)
                              .Select(kvp => (kvp.Key, kvp.Value))
                              .ToList();
        }

        /// <summary>
        /// Get top-k pair candidates by affinity
        /// </summary>
        public List<((int, int) parts, double affinity)> GetTopPairCandidates(int k)
        {
            return PairAffinities.OrderByDescending(kvp => kvp.Value)
                                .Take(k)
                                .Select(kvp => (kvp.Key, kvp.Value))
                                .ToList();
        }
    }
}