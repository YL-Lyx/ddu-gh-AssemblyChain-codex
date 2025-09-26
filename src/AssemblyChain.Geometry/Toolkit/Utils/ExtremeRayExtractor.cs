using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Utils
{
    /// <summary>
    /// Utilities for extracting extreme rays from constraint sets.
    /// </summary>
    public static class ExtremeRayExtractor
    {
        /// <summary>
        /// Extracts extreme rays from a set of constraint normals (simplified heuristic).
        /// </summary>
        public static IReadOnlyList<Vector3d> Extract(IReadOnlyList<Vector3d> constraintNormals, object options = null)
        {
            if (constraintNormals == null || constraintNormals.Count == 0)
                return Array.Empty<Vector3d>();

            var rays = new List<Vector3d>();
            foreach (var normal in constraintNormals)
            {
                var n = normal;
                if (n.Length <= 1e-12) continue;
                n.Unitize();
                bool farFromExisting = true;
                foreach (var existing in rays)
                {
                    var angle = Vector3d.VectorAngle(n, existing);
                    if (angle < (5.0 * System.Math.PI / 180.0)) { farFromExisting = false; break; }
                }
                if (farFromExisting) rays.Add(n);
            }

            // Limit to a reasonable number, sample by diversity
            if (rays.Count > 16)
            {
                rays = SampleRaysEvenly(rays, 16);
            }

            return rays;
        }

        private static List<Vector3d> SampleRaysEvenly(IReadOnlyList<Vector3d> rays, int maxCount)
        {
            var sampled = new List<Vector3d>();
            var remaining = rays.ToList();
            if (remaining.Count == 0 || maxCount <= 0) return sampled;

            // Greedy farthest-point sampling by angular distance
            sampled.Add(remaining[0]);
            remaining.RemoveAt(0);
            while (sampled.Count < maxCount && remaining.Count > 0)
            {
                Vector3d best = remaining[0];
                double bestMin = -1;
                foreach (var candidate in remaining)
                {
                    double minAngle = double.MaxValue;
                    foreach (var ex in sampled)
                    {
                        var angle = Vector3d.VectorAngle(candidate, ex);
                        if (angle < minAngle) minAngle = angle;
                    }
                    if (minAngle > bestMin)
                    {
                        bestMin = minAngle;
                        best = candidate;
                    }
                }
                sampled.Add(best);
                remaining.Remove(best);
            }
            return sampled;
        }
    }
}



