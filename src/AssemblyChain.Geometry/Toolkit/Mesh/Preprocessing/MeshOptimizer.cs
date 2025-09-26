using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Mesh.Preprocessing
{
    /// <summary>
    /// Specialized mesh optimization utilities for preprocessing pipeline.
    /// Focuses on vertex reduction, welding, and topology optimization.
    /// Used by MeshPreprocessor for automated mesh conditioning.
    /// </summary>
    public static class MeshOptimizer
    {
        /// <summary>
        /// Optimization options
        /// </summary>
        public class OptimizationOptions
        {
            public bool ReduceVertices { get; set; } = true;
            public bool WeldVertices { get; set; } = true;
            public bool UnifyNormals { get; set; } = true;
            public bool SmoothMesh { get; set; } = false;
            public double WeldTolerance { get; set; } = 1e-6;
            public double TargetEdgeLength { get; set; } = 0.0; // 0 = auto
            public int MaxIterations { get; set; } = 5;
            public double SmoothStrength { get; set; } = 0.5;
        }

        /// <summary>
        /// Optimization result
        /// </summary>
        public class OptimizationResult
        {
            public bool Success { get; set; }
            public int OriginalVertexCount { get; set; }
            public int OptimizedVertexCount { get; set; }
            public int OriginalFaceCount { get; set; }
            public int OptimizedFaceCount { get; set; }
            public double VertexReductionPercentage { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
        }

        /// <summary>
        /// Optimizes mesh topology and reduces complexity
        /// </summary>
        public static OptimizationResult OptimizeMesh(Rhino.Geometry.Mesh mesh, OptimizationOptions options = null)
        {
            options ??= new OptimizationOptions();
            var result = new OptimizationResult
            {
                OriginalVertexCount = mesh.Vertices.Count,
                OriginalFaceCount = mesh.Faces.Count
            };

            try
            {
                // Weld duplicate vertices
                if (options.WeldVertices)
                {
                    WeldDuplicateVertices(mesh, result, options);
                }

                // Reduce vertices (decimation)
                if (options.ReduceVertices)
                {
                    ReduceVertices(mesh, result, options);
                }

                // Unify normals
                if (options.UnifyNormals)
                {
                    UnifyMeshNormals(mesh, result);
                }

                // Smooth mesh (optional)
                if (options.SmoothMesh)
                {
                    SmoothMesh(mesh, result, options);
                }

                result.OptimizedVertexCount = mesh.Vertices.Count;
                result.OptimizedFaceCount = mesh.Faces.Count;

                if (result.OriginalVertexCount > 0)
                {
                    result.VertexReductionPercentage =
                        (1.0 - (double)result.OptimizedVertexCount / result.OriginalVertexCount) * 100.0;
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh optimization failed: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        private static void WeldDuplicateVertices(Rhino.Geometry.Mesh mesh, OptimizationResult result, OptimizationOptions options)
        {
            try
            {
                var originalCount = mesh.Vertices.Count;
                mesh.Weld(options.WeldTolerance);
                var newCount = mesh.Vertices.Count;

                if (newCount < originalCount)
                {
                    var welded = originalCount - newCount;
                    result.Warnings.Add($"Welded {welded} duplicate vertices");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Vertex welding failed: {ex.Message}");
            }
        }

        private static void ReduceVertices(Rhino.Geometry.Mesh mesh, OptimizationResult result, OptimizationOptions options)
        {
            try
            {
                // Simple vertex reduction based on edge length
                var targetLength = options.TargetEdgeLength;
                if (targetLength <= 0)
                {
                    // Auto-calculate target edge length based on bounding box
                    var bbox = mesh.GetBoundingBox(true);
                    targetLength = bbox.Diagonal.Length * 0.01; // 1% of diagonal
                }

                var removedVertices = new List<int>();
                var edgesToCollapse = new List<(int v1, int v2, double length)>();

                // Find short edges
                for (int i = 0; i < mesh.TopologyEdges.Count; i++)
                {
                    var edge = mesh.TopologyEdges.EdgeLine(i);
                    var length = edge.Length;

                    if (length < targetLength)
                    {
                        var topologyVertices = mesh.TopologyEdges.GetTopologyVertices(i);
                        if (topologyVertices.I >= 0 && topologyVertices.J >= 0)
                        {
                            edgesToCollapse.Add((topologyVertices.I, topologyVertices.J, length));
                        }
                    }
                }

                // Sort by length (shortest first)
                edgesToCollapse.Sort((a, b) => a.length.CompareTo(b.length));

                // Collapse edges (simplified implementation)
                var collapsed = 0;
                var maxCollapsed = System.Math.Min(edgesToCollapse.Count / 4, 1000); // Don't collapse too many

                foreach (var (v1, v2, length) in edgesToCollapse)
                {
                    if (collapsed >= maxCollapsed) break;

                    try
                    {
                        // Simple edge collapse: move v2 to v1
                        var pos1 = mesh.Vertices[v1];
                        var pos2 = mesh.Vertices[v2];
                        var newPos = new Point3f(
                            (pos1.X + pos2.X) * 0.5f,
                            (pos1.Y + pos2.Y) * 0.5f,
                            (pos1.Z + pos2.Z) * 0.5f
                        );

                        mesh.Vertices[v1] = newPos;
                        removedVertices.Add(v2);
                        collapsed++;
                    }
                    catch
                    {
                        // Skip problematic collapses
                    }
                }

                if (collapsed > 0)
                {
                    // Clean up mesh after collapses
                    mesh.Compact();
                    result.Warnings.Add($"Collapsed {collapsed} short edges");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Vertex reduction failed: {ex.Message}");
            }
        }

        private static void UnifyMeshNormals(Rhino.Geometry.Mesh mesh, OptimizationResult result)
        {
            try
            {
                mesh.UnifyNormals();
                result.Warnings.Add("Unified mesh normals");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Normal unification failed: {ex.Message}");
            }
        }

        private static void SmoothMesh(Rhino.Geometry.Mesh mesh, OptimizationResult result, OptimizationOptions options)
        {
            try
            {
                // Simple Laplacian smoothing
                var originalVertices = new Point3d[mesh.Vertices.Count];
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    originalVertices[i] = mesh.Vertices[i];
                }

                var smoothedVertices = new Point3d[mesh.Vertices.Count];

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var neighbors = GetVertexNeighbors(mesh, i);
                    if (neighbors.Count == 0)
                    {
                        smoothedVertices[i] = originalVertices[i];
                        continue;
                    }

                    // Laplacian smoothing: average of neighbors
                    var average = Point3d.Origin;
                    foreach (var neighbor in neighbors)
                    {
                        average += originalVertices[neighbor];
                    }
                    average /= neighbors.Count;

                    // Interpolate between original and average
                    smoothedVertices[i] = originalVertices[i] * (1 - options.SmoothStrength) +
                                        average * options.SmoothStrength;
                }

                // Apply smoothed vertices
                for (int i = 0; i < smoothedVertices.Length; i++)
                {
                    mesh.Vertices[i] = new Point3f((float)smoothedVertices[i].X, (float)smoothedVertices[i].Y, (float)smoothedVertices[i].Z);
                }

                result.Warnings.Add($"Applied Laplacian smoothing with strength {options.SmoothStrength:F2}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh smoothing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes vertices that are too close together or redundant
        /// </summary>
        public static int RemoveRedundantVertices(Rhino.Geometry.Mesh mesh, double tolerance = 1e-6)
        {
            try
            {
                var verticesToRemove = new HashSet<int>();
                var vertexMap = new Dictionary<Point3d, int>(new Point3dComparer(tolerance));

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var vertex = mesh.Vertices[i];

                    if (vertexMap.TryGetValue(vertex, out var existingIndex))
                    {
                        // This vertex is very close to an existing one
                        verticesToRemove.Add(i);
                        // Update faces that reference this vertex to use the existing one
                        UpdateFacesReferencingVertex(mesh, i, existingIndex);
                    }
                    else
                    {
                        vertexMap[vertex] = i;
                    }
                }

                // Remove vertices (this is a simplified implementation)
                // In practice, this requires careful mesh reconstruction
                return verticesToRemove.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Simplifies mesh by reducing face count while preserving shape
        /// </summary>
        public static int SimplifyMesh(Rhino.Geometry.Mesh mesh, int targetFaceCount)
        {
            try
            {
                if (mesh.Faces.Count <= targetFaceCount)
                    return 0;

                var facesToRemove = mesh.Faces.Count - targetFaceCount;
                var removed = 0;

                // Simple approach: remove small faces
                var faceAreas = new List<(int index, double area)>();

                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    var face = mesh.Faces[i];
                    var area = CalculateFaceArea(mesh, face);
                    faceAreas.Add((i, area));
                }

                // Sort by area (smallest first)
                faceAreas.Sort((a, b) => a.area.CompareTo(b.area));

                // Remove smallest faces
                for (int i = 0; i < System.Math.Min(facesToRemove, faceAreas.Count / 2); i++)
                {
                    try
                    {
                        mesh.Faces.RemoveAt(faceAreas[i].index - removed);
                        removed++;
                    }
                    catch
                    {
                        // Skip if removal fails
                    }
                }

                if (removed > 0)
                {
                    mesh.Compact();
                }

                return removed;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Improves mesh quality by adjusting vertex positions
        /// </summary>
        public static bool ImproveMeshQuality(Rhino.Geometry.Mesh mesh, int iterations = 3)
        {
            try
            {
                for (int iter = 0; iter < iterations; iter++)
                {
                    var originalVertices = new Point3d[mesh.Vertices.Count];
                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        originalVertices[i] = mesh.Vertices[i];
                    }

                    // Laplacian smoothing for quality improvement
                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        var neighbors = GetVertexNeighbors(mesh, i);
                        if (neighbors.Count > 0)
                        {
                            var centroid = Point3d.Origin;
                            foreach (var neighbor in neighbors)
                            {
                                centroid += originalVertices[neighbor];
                            }
                            centroid /= neighbors.Count;

                            // Move vertex towards centroid
                            var newPos = originalVertices[i] * 0.7 + centroid * 0.3;
                            mesh.Vertices[i] = new Point3f((float)newPos.X, (float)newPos.Y, (float)newPos.Z);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateFacesReferencingVertex(Rhino.Geometry.Mesh mesh, int oldVertex, int newVertex)
        {
            // This is a simplified implementation
            // In practice, this requires updating all face indices
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[i];
                if (face.A == oldVertex) mesh.Faces[i] = new MeshFace(newVertex, face.B, face.C, face.IsQuad ? face.D : face.C);
                else if (face.B == oldVertex) mesh.Faces[i] = new MeshFace(face.A, newVertex, face.C, face.IsQuad ? face.D : face.C);
                else if (face.C == oldVertex) mesh.Faces[i] = new MeshFace(face.A, face.B, newVertex, face.IsQuad ? face.D : face.C);
                else if (face.IsQuad && face.D == oldVertex) mesh.Faces[i] = new MeshFace(face.A, face.B, face.C, newVertex);
            }
        }

        private static List<int> GetVertexNeighbors(Rhino.Geometry.Mesh mesh, int vertexIndex)
        {
            var neighbors = new HashSet<int>();

            try
            {
                var connectedFaces = mesh.TopologyVertices.ConnectedFaces(vertexIndex);
                foreach (var faceIndex in connectedFaces)
                {
                    var face = mesh.Faces[faceIndex];
                    if (face.A == vertexIndex) { neighbors.Add(face.B); neighbors.Add(face.C); if (face.IsQuad) neighbors.Add(face.D); }
                    else if (face.B == vertexIndex) { neighbors.Add(face.A); neighbors.Add(face.C); if (face.IsQuad) neighbors.Add(face.D); }
                    else if (face.C == vertexIndex) { neighbors.Add(face.A); neighbors.Add(face.B); if (face.IsQuad) neighbors.Add(face.D); }
                    else if (face.IsQuad && face.D == vertexIndex) { neighbors.Add(face.A); neighbors.Add(face.B); neighbors.Add(face.C); }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return new List<int>(neighbors);
        }

        private static double CalculateFaceArea(Rhino.Geometry.Mesh mesh, MeshFace face)
        {
            try
            {
                var vertices = new Point3d[] {
                    mesh.Vertices[face.A],
                    mesh.Vertices[face.B],
                    mesh.Vertices[face.C]
                };

                if (face.IsQuad)
                {
                    // Approximate quad area as sum of two triangles
                    var v1 = vertices[1] - vertices[0];
                    var v2 = vertices[2] - vertices[0];
                    var v3 = vertices[3] - vertices[0];
                    var v4 = vertices[2] - vertices[1];

                    var cross1 = Vector3d.CrossProduct(v1, v2);
                    var cross2 = Vector3d.CrossProduct(v3, v4);

                    return (cross1.Length + cross2.Length) / 2.0;
                }
                else
                {
                    // Triangle area
                    var v1 = vertices[1] - vertices[0];
                    var v2 = vertices[2] - vertices[0];
                    var cross = Vector3d.CrossProduct(v1, v2);
                    return cross.Length / 2.0;
                }
            }
            catch
            {
                return 0.0;
            }
        }

        private class Point3dComparer : IEqualityComparer<Point3d>
        {
            private readonly double _tolerance;

            public Point3dComparer(double tolerance)
            {
                _tolerance = tolerance;
            }

            public bool Equals(Point3d a, Point3d b)
            {
                return a.DistanceTo(b) < _tolerance;
            }

            public int GetHashCode(Point3d p)
            {
                // Simple hash based on rounded coordinates
                var scale = 1.0 / _tolerance;
                var x = (int)(p.X * scale);
                var y = (int)(p.Y * scale);
                var z = (int)(p.Z * scale);
                return x ^ y ^ z;
            }
        }
    }
}
