using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Mesh.Preprocessing
{
    /// <summary>
    /// Specialized mesh repair utilities
    /// </summary>
    public static class MeshRepair
    {
        /// <summary>
        /// Repair options
        /// </summary>
        public class RepairOptions
        {
            public bool FillHoles { get; set; } = true;
            public bool FixNonManifoldEdges { get; set; } = true;
            public bool RemoveDuplicateFaces { get; set; } = true;
            public bool HealNakedEdges { get; set; } = false;
            public double Tolerance { get; set; } = 1e-6;
            public double MaxHoleSize { get; set; } = double.MaxValue;
            public int MaxRepairIterations { get; set; } = 3;
        }

        /// <summary>
        /// Repair result
        /// </summary>
        public class RepairResult
        {
            public bool Success { get; set; }
            public int HolesFilled { get; set; }
            public int NonManifoldEdgesFixed { get; set; }
            public int DuplicateFacesRemoved { get; set; }
            public int NakedEdgesHealed { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
        }

        /// <summary>
        /// Repairs common mesh defects
        /// </summary>
        public static RepairResult RepairMesh(Rhino.Geometry.Mesh mesh, RepairOptions options = null)
        {
            options ??= new RepairOptions();
            var result = new RepairResult();

            try
            {
                // Fill holes
                if (options.FillHoles)
                {
                    FillMeshHoles(mesh, result, options);
                }

                // Fix non-manifold edges
                if (options.FixNonManifoldEdges)
                {
                    FixNonManifoldEdges(mesh, result, options);
                }

                // Remove duplicate faces
                if (options.RemoveDuplicateFaces)
                {
                    RemoveDuplicateFaces(mesh, result, options);
                }

                // Heal naked edges (optional, more aggressive)
                if (options.HealNakedEdges)
                {
                    HealNakedEdges(mesh, result, options);
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh repair failed: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        private static void FillMeshHoles(Rhino.Geometry.Mesh mesh, RepairResult result, RepairOptions options)
        {
            try
            {
                var initialFaceCount = mesh.Faces.Count;
                var nakedEdges = mesh.GetNakedEdges();

                if (nakedEdges == null || nakedEdges.Length == 0)
                {
                    result.Warnings.Add("No naked edges found - mesh appears to be closed");
                    return;
                }

                var edgeLoops = ExtractLoopsFromNakedEdges(nakedEdges, mesh, options.Tolerance);

                foreach (var loop in edgeLoops)
                {
                    if (loop.VertexIndices.Count < 3)
                    {
                        result.Warnings.Add($"Skipping degenerate hole with {loop.VertexIndices.Count} vertices");
                        continue;
                    }

                    var holeArea = CalculateLoopArea(loop.Points);
                    if (holeArea > options.MaxHoleSize)
                    {
                        result.Warnings.Add($"Skipping large hole (area: {holeArea:F6}, max: {options.MaxHoleSize:F6})");
                        continue;
                    }

                    if (TryFillHole(mesh, loop.VertexIndices))
                    {
                        result.HolesFilled++;
                    }
                    else
                    {
                        result.Warnings.Add($"Failed to fill hole with {loop.VertexIndices.Count} vertices");
                    }
                }

                var finalFaceCount = mesh.Faces.Count;
                if (finalFaceCount > initialFaceCount)
                {
                    result.Warnings.Add($"Filled {result.HolesFilled} holes, added {finalFaceCount - initialFaceCount} faces");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Hole filling failed: {ex.Message}");
            }
        }

        private static void FixNonManifoldEdges(Rhino.Geometry.Mesh mesh, RepairResult result, RepairOptions options)
        {
            try
            {
                // Non-manifold edge detection not directly available in Rhino
                int[] nonManifoldEdges = null;

                if (nonManifoldEdges == null || nonManifoldEdges.Length == 0)
                {
                    return; // No non-manifold edges to fix
                }

                // For now, we just report them - fixing non-manifold edges is complex
                // and may require mesh reconstruction
                result.NonManifoldEdgesFixed = 0;
                result.Warnings.Add($"Found {nonManifoldEdges.Length} non-manifold edges (not automatically fixed)");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Non-manifold edge detection failed: {ex.Message}");
            }
        }

        private static void RemoveDuplicateFaces(Rhino.Geometry.Mesh mesh, RepairResult result, RepairOptions options)
        {
            try
            {
                var faceSignatures = new Dictionary<string, int>();
                var facesToRemove = new List<int>();

                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    var face = mesh.Faces[i];
                    var signature = GetFaceSignature(face);

                    if (faceSignatures.ContainsKey(signature))
                    {
                        facesToRemove.Add(i);
                    }
                    else
                    {
                        faceSignatures[signature] = i;
                    }
                }

                // Remove faces in reverse order to maintain indices
                facesToRemove.Sort((a, b) => b.CompareTo(a));
                foreach (var faceIndex in facesToRemove)
                {
                    mesh.Faces.RemoveAt(faceIndex);
                }

                result.DuplicateFacesRemoved = facesToRemove.Count;
                if (facesToRemove.Count > 0)
                {
                    result.Warnings.Add($"Removed {facesToRemove.Count} duplicate faces");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Duplicate face removal failed: {ex.Message}");
            }
        }

        private static void HealNakedEdges(Rhino.Geometry.Mesh mesh, RepairResult result, RepairOptions options)
        {
            // This is a complex operation that may involve mesh stitching
            // For now, just report that it's not implemented
            result.Warnings.Add("Naked edge healing is not implemented in this version");
        }

        private static double CalculateLoopArea(IReadOnlyList<Point3d> points)
        {
            try
            {
                if (points == null || points.Count < 3)
                {
                    return 0.0;
                }

                if (!Plane.FitPlaneToPoints(points, out var plane))
                {
                    return double.MaxValue;
                }

                var projected = new List<Point2d>(points.Count);
                foreach (var point in points)
                {
                    plane.ClosestPoint(point, out double u, out double v);
                    projected.Add(new Point2d(u, v));
                }

                double area = 0.0;
                for (int i = 0; i < projected.Count; i++)
                {
                    var j = (i + 1) % projected.Count;
                    area += projected[i].X * projected[j].Y;
                    area -= projected[j].X * projected[i].Y;
                }

                return System.Math.Abs(area) / 2.0;
            }
            catch
            {
                return double.MaxValue;
            }
        }

        private static bool TryFillHole(Rhino.Geometry.Mesh mesh, List<int> loop)
        {
            try
            {
                if (loop == null || loop.Count < 3)
                {
                    return false;
                }

                if (loop.Count == 3)
                {
                    mesh.Faces.AddFace(loop[0], loop[1], loop[2]);
                }
                else if (loop.Count == 4)
                {
                    mesh.Faces.AddFace(loop[0], loop[1], loop[2], loop[3]);
                }
                else
                {
                    for (int i = 1; i < loop.Count - 1; i++)
                    {
                        mesh.Faces.AddFace(loop[0], loop[i], loop[i + 1]);
                    }
                }

                mesh.Normals.ComputeNormals();
                mesh.Compact();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static List<MeshHoleLoop> ExtractLoopsFromNakedEdges(Polyline[] nakedEdges, Rhino.Geometry.Mesh mesh, double tolerance)
        {
            var loops = new List<MeshHoleLoop>();

            if (nakedEdges == null)
            {
                return loops;
            }

            foreach (var edgeLoop in nakedEdges)
            {
                if (edgeLoop == null || edgeLoop.Count < 4)
                {
                    continue;
                }

                var vertexIndices = new List<int>();
                var points = new List<Point3d>();
                int count = edgeLoop.IsClosed ? edgeLoop.Count - 1 : edgeLoop.Count;

                for (int i = 0; i < count; i++)
                {
                    var point = edgeLoop[i];
                    var vertexIndex = FindOrCreateVertexIndex(mesh, point, tolerance);

                    if (vertexIndex < 0)
                    {
                        continue;
                    }

                    points.Add(point);
                    vertexIndices.Add(vertexIndex);
                }

                if (vertexIndices.Count >= 3)
                {
                    loops.Add(new MeshHoleLoop(vertexIndices, points));
                }
            }

            return loops;
        }

        private static int FindOrCreateVertexIndex(Rhino.Geometry.Mesh mesh, Point3d point, double tolerance)
        {
            var meshPoint = mesh.ClosestMeshPoint(point, tolerance);
            if (meshPoint != null && meshPoint.VertexIndex >= 0)
            {
                return meshPoint.VertexIndex;
            }

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                if (mesh.Vertices[i].ToPoint3d().DistanceTo(point) <= tolerance)
                {
                    return i;
                }
            }

            mesh.Vertices.Add(point);
            return mesh.Vertices.Count - 1;
        }

        private class MeshHoleLoop
        {
            public MeshHoleLoop(List<int> vertexIndices, List<Point3d> points)
            {
                VertexIndices = vertexIndices;
                Points = points;
            }

            public List<int> VertexIndices { get; }
            public List<Point3d> Points { get; }
        }

        private static string GetFaceSignature(MeshFace face)
        {
            if (face.IsQuad)
            {
                var indices = new[] { face.A, face.B, face.C, face.D };
                Array.Sort(indices);
                return $"{indices[0]},{indices[1]},{indices[2]},{indices[3]}";
            }
            else
            {
                var indices = new[] { face.A, face.B, face.C };
                Array.Sort(indices);
                return $"{indices[0]},{indices[1]},{indices[2]}";
            }
        }
    }
}
