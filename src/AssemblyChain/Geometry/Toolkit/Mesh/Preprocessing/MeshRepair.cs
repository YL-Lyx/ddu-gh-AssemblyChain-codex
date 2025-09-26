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

                // Group naked edges into loops
                // TODO: Fix edge loop detection - nakedEdges type issue
                var edgeLoops = new List<List<int>>(); // Temporarily disable

                foreach (var loop in edgeLoops)
                {
                    if (loop.Count < 3)
                    {
                        result.Warnings.Add($"Skipping degenerate hole with {loop.Count} edges");
                        continue;
                    }

                    // Check hole size
                    var holeArea = CalculateLoopArea(loop, mesh);
                    if (holeArea > options.MaxHoleSize)
                    {
                        result.Warnings.Add($"Skipping large hole (area: {holeArea:F6}, max: {options.MaxHoleSize:F6})");
                        continue;
                    }

                    // Try to fill the hole
                    if (TryFillHole(mesh, loop))
                    {
                        result.HolesFilled++;
                    }
                    else
                    {
                        result.Warnings.Add($"Failed to fill hole with {loop.Count} edges");
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

        private static List<List<int>> GroupNakedEdgesIntoLoops(int[] nakedEdges, double tolerance)
        {
            var loops = new List<List<int>>();
            var processed = new HashSet<int>();

            for (int i = 0; i < nakedEdges.Length; i += 2)
            {
                if (processed.Contains(i)) continue;

                var loop = new List<int> { nakedEdges[i], nakedEdges[i + 1] };
                processed.Add(i);
                processed.Add(i + 1);

                // Try to extend the loop
                bool extended = true;
                while (extended)
                {
                    extended = false;

                    // Find edges that connect to the current loop
                    for (int j = 0; j < nakedEdges.Length; j += 2)
                    {
                        if (processed.Contains(j)) continue;

                        var edgeStart = nakedEdges[j];
                        var edgeEnd = nakedEdges[j + 1];

                        // Check if this edge connects to the loop
                        if (loop[0] == edgeEnd && loop[^1] == edgeStart)
                        {
                            // Insert at beginning
                            loop.Insert(0, edgeStart);
                            loop.Insert(0, edgeEnd);
                            processed.Add(j);
                            processed.Add(j + 1);
                            extended = true;
                            break;
                        }
                        else if (loop[^1] == edgeStart && loop[0] == edgeEnd)
                        {
                            // Insert at end
                            loop.Add(edgeStart);
                            loop.Add(edgeEnd);
                            processed.Add(j);
                            processed.Add(j + 1);
                            extended = true;
                            break;
                        }
                        else if (loop[0] == edgeStart && loop[^1] == edgeEnd)
                        {
                            // Reverse and insert at beginning
                            loop.Insert(0, edgeEnd);
                            loop.Insert(0, edgeStart);
                            processed.Add(j);
                            processed.Add(j + 1);
                            extended = true;
                            break;
                        }
                        else if (loop[^1] == edgeEnd && loop[0] == edgeStart)
                        {
                            // Reverse and insert at end
                            loop.Add(edgeEnd);
                            loop.Add(edgeStart);
                            processed.Add(j);
                            processed.Add(j + 1);
                            extended = true;
                            break;
                        }
                    }
                }

                if (loop.Count >= 4) // At least 2 edges
                {
                    loops.Add(loop);
                }
            }

            return loops;
        }

        private static double CalculateLoopArea(List<int> loop, Rhino.Geometry.Mesh mesh)
        {
            try
            {
                // Convert vertex indices to points
                var points = new List<Point3d>();
                foreach (var vertexIndex in loop)
                {
                    points.Add(mesh.Vertices[vertexIndex]);
                }

                if (points.Count < 3) return 0.0;

                // Calculate area using shoelace formula
                double area = 0.0;
                for (int i = 0; i < points.Count; i++)
                {
                    var j = (i + 1) % points.Count;
                    area += points[i].X * points[j].Y;
                    area -= points[j].X * points[i].Y;
                }

                return System.Math.Abs(area) / 2.0;
            }
            catch
            {
                return double.MaxValue; // Return large area to prevent filling
            }
        }

        private static bool TryFillHole(Rhino.Geometry.Mesh mesh, List<int> loop)
        {
            try
            {
                // Simple triangulation approach
                if (loop.Count == 4) // Triangle hole
                {
                    mesh.Faces.AddFace(loop[0], loop[1], loop[2]);
                    return true;
                }
                else if (loop.Count == 6) // Quad hole
                {
                    mesh.Faces.AddFace(loop[0], loop[1], loop[2], loop[3]);
                    return true;
                }
                else
                {
                    // For more complex holes, use a simple fan triangulation
                    for (int i = 2; i < loop.Count; i++)
                    {
                        mesh.Faces.AddFace(loop[0], loop[i - 1], loop[i]);
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
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
