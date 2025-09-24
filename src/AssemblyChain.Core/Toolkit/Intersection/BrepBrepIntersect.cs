using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Intersection
{
    /// <summary>
    /// Two-stage Brep-Brep intersection with tolerance handling and result merging.
    /// </summary>
    public static class BrepBrepIntersect
    {
        /// <summary>
        /// Intersection options.
        /// </summary>
        public class IntersectionOptions
        {
            public double Tolerance { get; set; } = 1e-6;
            public bool MergeCoplanarIntersections { get; set; } = true;
            public bool IncludeBoundaryIntersections { get; set; } = true;
            public bool IncludeInteriorIntersections { get; set; } = false;
            public int MaxIntersectionPoints { get; set; } = 1000;
        }

        /// <summary>
        /// Intersection result.
        /// </summary>
        public class IntersectionResult
        {
            public List<Point3d> IntersectionPoints { get; set; } = new List<Point3d>();
            public List<Line> IntersectionLines { get; set; } = new List<Line>();
            public List<Curve> IntersectionCurves { get; set; } = new List<Curve>();
            public bool Success { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public TimeSpan ExecutionTime { get; set; }
        }

        /// <summary>
        /// Computes intersection between two Breps.
        /// </summary>
        public static IntersectionResult ComputeIntersection(
            Rhino.Geometry.Brep brep1,
            Rhino.Geometry.Brep brep2,
            IntersectionOptions options = null)
        {
            options ??= new IntersectionOptions();
            var result = new IntersectionResult();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (brep1 == null || brep2 == null || !brep1.IsValid || !brep2.IsValid)
                {
                    result.Errors.Add("Invalid input Breps");
                    result.Success = false;
                    return result;
                }

                // Stage 1: Surface-surface intersections
                var surfaceIntersections = ComputeSurfaceIntersections(brep1, brep2, options);

                // Stage 2: Merge and clean up results
                if (options.MergeCoplanarIntersections)
                {
                    surfaceIntersections = MergeCoplanarIntersections(surfaceIntersections, options);
                }

                // Convert to final format
                result.IntersectionCurves.AddRange(surfaceIntersections);
                result.Success = result.Errors.Count == 0;

                // Extract points from curves if requested
                if (options.MaxIntersectionPoints > 0)
                {
                    ExtractPointsFromCurves(surfaceIntersections, result, options);
                }

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                result.Errors.Add($"Intersection failed: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Computes surface-surface intersections.
        /// </summary>
        private static List<Curve> ComputeSurfaceIntersections(
            Rhino.Geometry.Brep brep1,
            Rhino.Geometry.Brep brep2,
            IntersectionOptions options)
        {
            var intersections = new List<Curve>();

            try
            {
                Curve[] curves;
                Point3d[] points;
                var ok = Rhino.Geometry.Intersect.Intersection.BrepBrep(brep1, brep2, options.Tolerance, out curves, out points);
                if (ok && curves != null && curves.Length > 0)
                {
                    intersections.AddRange(curves);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue
                System.Diagnostics.Debug.WriteLine($"Surface intersection failed: {ex.Message}");
            }

            return intersections;
        }

        /// <summary>
        /// Merges coplanar intersection curves.
        /// </summary>
        private static List<Curve> MergeCoplanarIntersections(
            List<Curve> intersections,
            IntersectionOptions options)
        {
            if (intersections == null || intersections.Count <= 1) return intersections ?? new List<Curve>();

            var merged = new List<Curve>();
            var processed = new bool[intersections.Count];

            for (int i = 0; i < intersections.Count; i++)
            {
                if (processed[i]) continue;

                var currentGroup = new List<Curve> { intersections[i] };
                processed[i] = true;

                // Find curves that can be merged with this one (simple endpoint proximity heuristic)
                for (int j = i + 1; j < intersections.Count; j++)
                {
                    if (processed[j]) continue;

                    if (CanMergeCurves(intersections[i], intersections[j], options))
                    {
                        currentGroup.Add(intersections[j]);
                        processed[j] = true;
                    }
                }

                // Merge the group into a single curve when possible
                if (currentGroup.Count == 1)
                {
                    merged.Add(currentGroup[0]);
                }
                else
                {
                    var mergedCurve = MergeCurveGroup(currentGroup, options);
                    if (mergedCurve != null)
                    {
                        merged.Add(mergedCurve);
                    }
                    else
                    {
                        // If merging failed, add them separately
                        merged.AddRange(currentGroup);
                    }
                }
            }

            return merged;
        }

        /// <summary>
        /// Checks if two curves can be merged.
        /// </summary>
        private static bool CanMergeCurves(Curve curve1, Curve curve2, IntersectionOptions options)
        {
            try
            {
                // Check if curves are close at endpoints
                var end1 = curve1.PointAtEnd;
                var start2 = curve2.PointAtStart;
                var end2 = curve2.PointAtEnd;
                var start1 = curve1.PointAtStart;

                var dist1 = end1.DistanceTo(start2);
                var dist2 = end1.DistanceTo(end2);

                return dist1 < options.Tolerance || dist2 < options.Tolerance;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Merges a group of curves into a single curve.
        /// </summary>
        private static Curve MergeCurveGroup(List<Curve> curves, IntersectionOptions options)
        {
            try
            {
                // Simple approach: join the curves
                var joined = Curve.JoinCurves(curves, options.Tolerance);
                return joined != null && joined.Length > 0 ? joined[0] : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts points from intersection curves.
        /// </summary>
        private static void ExtractPointsFromCurves(
            List<Curve> curves,
            IntersectionResult result,
            IntersectionOptions options)
        {
            foreach (var curve in curves)
            {
                if (result.IntersectionPoints.Count >= options.MaxIntersectionPoints)
                    break;

                try
                {
                    // Sample points along the curve
                    var points = SamplePointsOnCurve(curve, options);
                    result.IntersectionPoints.AddRange(points);
                }
                catch
                {
                    // Skip problematic curves
                }
            }

            // Limit total points
            if (result.IntersectionPoints.Count > options.MaxIntersectionPoints)
            {
                result.IntersectionPoints = result.IntersectionPoints
                    .Take(options.MaxIntersectionPoints)
                    .ToList();
            }
        }

        /// <summary>
        /// Samples points along a curve.
        /// </summary>
        private static List<Point3d> SamplePointsOnCurve(Curve curve, IntersectionOptions options)
        {
            var points = new List<Point3d>();

            try
            {
                var length = curve.GetLength();
                var numSamples = System.Math.Max(2, (int)(length / System.Math.Max(options.Tolerance, 1e-9)));

                for (int i = 0; i <= numSamples; i++)
                {
                    var t = curve.Domain.ParameterAt((double)i / numSamples);
                    var point = curve.PointAt(t);
                    points.Add(point);
                }
            }
            catch
            {
                // Return endpoints at least
                points.Add(curve.PointAtStart);
                points.Add(curve.PointAtEnd);
            }

            return points;
        }

        /// <summary>
        /// Computes intersection between multiple Brep pairs.
        /// </summary>
        public static List<IntersectionResult> ComputeMultipleIntersections(
            IReadOnlyList<Rhino.Geometry.Brep> breps,
            IntersectionOptions options = null)
        {
            options ??= new IntersectionOptions();
            var results = new List<IntersectionResult>();

            if (breps == null) return results;

            for (int i = 0; i < breps.Count; i++)
            {
                for (int j = i + 1; j < breps.Count; j++)
                {
                    var result = ComputeIntersection(breps[i], breps[j], options);
                    results.Add(result);
                }
            }

            return results;
        }
    }
}




