using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Math
{
    /// <summary>
    /// Clustering algorithms for geometric data analysis and grouping.
    /// </summary>
    public static class Clustering
    {
        public class KMeansOptions
        {
            public int MaxIterations { get; set; } = 100;
            public double Tolerance { get; set; } = 1e-6;
            public int RandomSeed { get; set; } = 42;
            public bool UseKMeansPlusPlus { get; set; } = true;
        }

        public class KMeansResult
        {
            public List<List<Point3d>> Clusters { get; set; } = new List<List<Point3d>>();
            public List<Point3d> Centroids { get; set; } = new List<Point3d>();
            public List<int> Labels { get; set; } = new List<int>();
            public double Inertia { get; set; }
            public int Iterations { get; set; }
            public bool Converged { get; set; }
        }

        public static KMeansResult KMeans(IReadOnlyList<Point3d> points, int k, KMeansOptions options = null)
        {
            options ??= new KMeansOptions();
            var result = new KMeansResult();
            if (points == null || points.Count == 0 || k <= 0) return result;
            var random = new Random(options.RandomSeed);

            var centroids = options.UseKMeansPlusPlus ?
                InitializeKMeansPlusPlus(points, k, random) :
                InitializeRandom(points, k, random);

            var labels = new int[points.Count];
            var oldCentroids = new List<Point3d>(centroids);
            int iteration = 0;

            while (iteration < options.MaxIterations)
            {
                // Assign points
                for (int i = 0; i < points.Count; i++)
                {
                    labels[i] = FindNearestCentroid(points[i], centroids);
                }

                // Update centroids
                centroids = UpdateCentroids(points, labels, k);

                // Convergence check
                double maxMovement = 0.0;
                for (int i = 0; i < centroids.Count; i++)
                {
                    var movement = centroids[i].DistanceTo(oldCentroids[i]);
                    maxMovement = System.Math.Max(maxMovement, movement);
                }
                iteration++;
                if (maxMovement <= options.Tolerance)
                {
                    result.Converged = true;
                    break;
                }
                oldCentroids = centroids.ToList();
            }

            result.Centroids = centroids;
            result.Labels = labels.ToList();
            result.Iterations = iteration;
            result.Inertia = CalculateInertia(points, centroids, labels);

            result.Clusters = new List<List<Point3d>>(k);
            for (int i = 0; i < k; i++) result.Clusters.Add(new List<Point3d>());
            for (int i = 0; i < points.Count; i++) result.Clusters[labels[i]].Add(points[i]);
            return result;
        }

        private static List<Point3d> InitializeKMeansPlusPlus(IReadOnlyList<Point3d> points, int k, Random random)
        {
            var centroids = new List<Point3d>();
            var distances = new double[points.Count];
            centroids.Add(points[random.Next(points.Count)]);

            while (centroids.Count < k)
            {
                // compute distances to nearest centroid
                for (int j = 0; j < points.Count; j++)
                {
                    var dmin = double.MaxValue;
                    foreach (var c in centroids)
                    {
                        var d = points[j].DistanceTo(c);
                        dmin = System.Math.Min(dmin, d);
                    }
                    distances[j] = dmin * dmin;
                }
                var total = distances.Sum();
                var target = random.NextDouble() * System.Math.Max(total, 1e-12);
                double cumulative = 0;
                int chosen = 0;
                for (int j = 0; j < distances.Length; j++)
                {
                    cumulative += distances[j];
                    if (cumulative >= target) { chosen = j; break; }
                }
                centroids.Add(points[chosen]);
            }
            return centroids;
        }

        private static List<Point3d> InitializeRandom(IReadOnlyList<Point3d> points, int k, Random random)
        {
            var centroids = new List<Point3d>();
            var indices = Enumerable.Range(0, points.Count).ToList();
            for (int i = 0; i < k && indices.Count > 0; i++)
            {
                var randomIndex = random.Next(indices.Count);
                centroids.Add(points[indices[randomIndex]]);
                indices.RemoveAt(randomIndex);
            }
            return centroids;
        }

        private static int FindNearestCentroid(Point3d point, IReadOnlyList<Point3d> centroids)
        {
            var minDistance = double.MaxValue;
            var nearestIndex = 0;
            for (int i = 0; i < centroids.Count; i++)
            {
                var distance = point.DistanceTo(centroids[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }
            return nearestIndex;
        }

        private static List<Point3d> UpdateCentroids(IReadOnlyList<Point3d> points, int[] labels, int k)
        {
            var sums = new Vector3d[k];
            var counts = new int[k];
            for (int i = 0; i < points.Count; i++)
            {
                int idx = labels[i];
                sums[idx] += new Vector3d(points[i].X, points[i].Y, points[i].Z);
                counts[idx]++;
            }
            var centroids = new List<Point3d>(k);
            for (int i = 0; i < k; i++)
            {
                if (counts[i] > 0)
                {
                    var v = sums[i] / counts[i];
                    centroids.Add(new Point3d(v.X, v.Y, v.Z));
                }
                else
                {
                    centroids.Add(Point3d.Origin);
                }
            }
            return centroids;
        }

        private static double CalculateInertia(IReadOnlyList<Point3d> points, IReadOnlyList<Point3d> centroids, int[] labels)
        {
            double inertia = 0.0;
            for (int i = 0; i < points.Count; i++)
            {
                var c = centroids[labels[i]];
                var d = points[i].DistanceTo(c);
                inertia += d * d;
            }
            return inertia;
        }

        public static List<List<Point3d>> HierarchicalClustering(IReadOnlyList<Point3d> points, int maxClusters)
        {
            if (points == null || points.Count == 0 || maxClusters <= 0)
                return new List<List<Point3d>>();
            var clusters = points.Select(p => new List<Point3d> { p }).ToList();
            while (clusters.Count > maxClusters)
            {
                double minDistance = double.MaxValue;
                int iMin = -1, jMin = -1;
                for (int i = 0; i < clusters.Count; i++)
                {
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        var distance = ClusterDistance(clusters[i], clusters[j]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            iMin = i;
                            jMin = j;
                        }
                    }
                }
                if (iMin == -1) break;
                clusters[iMin].AddRange(clusters[jMin]);
                clusters.RemoveAt(jMin);
            }
            return clusters;
        }

        private static double ClusterDistance(List<Point3d> cluster1, List<Point3d> cluster2)
        {
            double minDistance = double.MaxValue;
            foreach (var p1 in cluster1)
            {
                foreach (var p2 in cluster2)
                {
                    var d = p1.DistanceTo(p2);
                    if (d < minDistance) minDistance = d;
                }
            }
            return minDistance;
        }

        public static List<List<Point3d>> DBSCAN(IReadOnlyList<Point3d> points, double eps, int minPoints)
        {
            var clusters = new List<List<Point3d>>();
            if (points == null || points.Count == 0) return clusters;
            var visited = new bool[points.Count];
            var assigned = new bool[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                if (visited[i]) continue;
                visited[i] = true;
                var neighbors = FindNeighbors(points, i, eps);
                if (neighbors.Count < minPoints)
                {
                    continue; // noise
                }
                var cluster = new List<Point3d>();
                clusters.Add(cluster);
                var queue = new Queue<int>(neighbors);
                while (queue.Count > 0)
                {
                    var idx = queue.Dequeue();
                    if (!visited[idx])
                    {
                        visited[idx] = true;
                        var further = FindNeighbors(points, idx, eps);
                        if (further.Count >= minPoints)
                        {
                            foreach (var nn in further)
                            {
                                if (!visited[nn]) queue.Enqueue(nn);
                            }
                        }
                    }
                    if (!assigned[idx])
                    {
                        assigned[idx] = true;
                        cluster.Add(points[idx]);
                    }
                }
            }
            return clusters;
        }

        private static List<int> FindNeighbors(IReadOnlyList<Point3d> points, int pointIndex, double eps)
        {
            var neighbors = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                if (points[pointIndex].DistanceTo(points[i]) <= eps)
                    neighbors.Add(i);
            }
            return neighbors;
        }

        public static double SilhouetteScore(IReadOnlyList<Point3d> points, IReadOnlyList<int> labels, int k)
        {
            if (points == null || labels == null || points.Count != labels.Count || k <= 1) return 0.0;
            var silhouetteValues = new List<double>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                int clusterI = labels[i];
                double a = AverageDistanceToCluster(points, i, clusterI, labels);
                double b = double.MaxValue;
                for (int c = 0; c < k; c++)
                {
                    if (c == clusterI) continue;
                    var dist = AverageDistanceToCluster(points, i, c, labels);
                    b = System.Math.Min(b, dist);
                }
                var silhouette = (b - a) / System.Math.Max(a, b);
                silhouetteValues.Add(double.IsNaN(silhouette) ? 0.0 : silhouette);
            }
            return silhouetteValues.Average();
        }

        private static double AverageDistanceToCluster(IReadOnlyList<Point3d> points, int pointIndex, int clusterIndex, IReadOnlyList<int> labels)
        {
            var distances = new List<double>();
            for (int i = 0; i < points.Count; i++)
            {
                if (labels[i] == clusterIndex)
                    distances.Add(points[pointIndex].DistanceTo(points[i]));
            }
            return distances.Count > 0 ? distances.Average() : 0.0;
        }
    }
}



