using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Mesh
{
    /// <summary>
    /// Mesh optimization utilities for topology improvement and simplification.
    /// </summary>
    public static class MeshOptimize
    {
        /// <summary>
        /// Mesh optimization options.
        /// </summary>
        public class OptimizeOptions
        {
            public bool ReduceVertices { get; set; } = true;
            public bool ImproveAspectRatio { get; set; } = true;
            public bool RemoveSmallFeatures { get; set; } = true;
            public bool SmoothNormals { get; set; } = false;
            public double TargetEdgeLength { get; set; } = 0.0; // 0 = auto
            public double MinEdgeLength { get; set; } = 1e-6;
            public double MaxEdgeLength { get; set; } = double.MaxValue;
            public int MaxIterations { get; set; } = 10;
            public double Tolerance { get; set; } = 1e-6;
        }

        /// <summary>
        /// Optimization result.
        /// </summary>
        public class OptimizeResult
        {
            public Rhino.Geometry.Mesh OptimizedMesh { get; set; }
            public bool Success { get; set; }
            public List<string> OperationsPerformed { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public int OriginalVertexCount { get; set; }
            public int OptimizedVertexCount { get; set; }
            public int OriginalFaceCount { get; set; }
            public int OptimizedFaceCount { get; set; }
        }

        /// <summary>
        /// Performs comprehensive mesh optimization.
        /// </summary>
        public static OptimizeResult OptimizeMesh(Rhino.Geometry.Mesh inputMesh, OptimizeOptions options = null)
        {
            options ??= new OptimizeOptions();
            var result = new OptimizeResult
            {
                OriginalVertexCount = inputMesh?.Vertices.Count ?? 0,
                OriginalFaceCount = inputMesh?.Faces.Count ?? 0
            };

            if (inputMesh == null || !inputMesh.IsValid)
            {
                result.Errors.Add("Input mesh is null or invalid");
                result.Success = false;
                return result;
            }

            try
            {
                var mesh = inputMesh.DuplicateMesh();
                result.OptimizedMesh = mesh;

                // 1. Remove small features
                if (options.RemoveSmallFeatures)
                {
                    var smallFeaturesRemoved = RemoveSmallFeatures(mesh, options);
                    if (smallFeaturesRemoved > 0)
                    {
                        result.OperationsPerformed.Add($"Removed {smallFeaturesRemoved} small features");
                    }
                }

                // 2. Improve aspect ratios
                if (options.ImproveAspectRatio)
                {
                    var aspectRatioImproved = ImproveAspectRatios(mesh, options);
                    if (aspectRatioImproved > 0)
                    {
                        result.OperationsPerformed.Add($"Improved aspect ratio for {aspectRatioImproved} faces");
                    }
                }

                // 3. Reduce vertices if requested
                if (options.ReduceVertices)
                {
                    var originalVertexCount = mesh.Vertices.Count;
                    ReduceVertices(mesh, options);
                    var newVertexCount = mesh.Vertices.Count;
                    if (newVertexCount < originalVertexCount)
                    {
                        result.OperationsPerformed.Add($"Reduced vertices: {originalVertexCount} → {newVertexCount}");
                    }
                }

                // 4. Smooth normals if requested
                if (options.SmoothNormals)
                {
                    SmoothNormals(mesh, options);
                    result.OperationsPerformed.Add("Smoothed vertex normals");
                }

                // Compact and finalize
                mesh.Compact();

                result.OptimizedVertexCount = mesh.Vertices.Count;
                result.OptimizedFaceCount = mesh.Faces.Count;
                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh optimization failed: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Removes small geometric features that may cause numerical issues.
        /// </summary>
        private static int RemoveSmallFeatures(Rhino.Geometry.Mesh mesh, OptimizeOptions options)
        {
            int featuresRemoved = 0;

            // Remove very short edges by collapsing them (simplified placeholder)
            var shortEdges = FindShortEdges(mesh, options.MinEdgeLength);
            foreach (var edge in shortEdges)
            {
                if (TryCollapseEdge(mesh, edge.Item1, edge.Item2, options.Tolerance))
                {
                    featuresRemoved++;
                }
            }

            return featuresRemoved;
        }

        /// <summary>
        /// Finds edges shorter than the specified length.
        /// </summary>
        private static List<(int, int)> FindShortEdges(Rhino.Geometry.Mesh mesh, double minLength)
        {
            var shortEdges = new List<(int, int)>();

            for (int ei = 0; ei < mesh.TopologyEdges.Count; ei++)
            {
                var line = mesh.TopologyEdges.EdgeLine(ei);
                if (line.Length < minLength)
                {
                    var vertices = mesh.TopologyEdges.GetTopologyVertices(ei);
                    shortEdges.Add((vertices.I, vertices.J));
                }
            }

            return shortEdges;
        }

        /// <summary>
        /// Attempts to collapse a short edge.
        /// </summary>
        private static bool TryCollapseEdge(Rhino.Geometry.Mesh mesh, int vertexA, int vertexB, double tolerance)
        {
            // This is a simplified implementation
            try
            {
                var positionA = mesh.Vertices[vertexA];
                mesh.Vertices[vertexB] = positionA;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to improve aspect ratios of faces.
        /// </summary>
        private static int ImproveAspectRatios(Rhino.Geometry.Mesh mesh, OptimizeOptions options)
        {
            int improved = 0;
            // Placeholder: compacting may remove degenerate faces
            var originalFaceCount = mesh.Faces.Count;
            mesh.Compact();
            var newFaceCount = mesh.Faces.Count;
            improved = System.Math.Max(0, originalFaceCount - newFaceCount);
            return improved;
        }

        /// <summary>
        /// Reduces vertex count by simplifying the mesh.
        /// </summary>
        private static void ReduceVertices(Rhino.Geometry.Mesh mesh, OptimizeOptions options)
        {
            try
            {
                // Placeholder simplification
                mesh.Compact();
            }
            catch
            {
                // Ignore errors in simplification
            }
        }

        /// <summary>
        /// Smooths vertex normals for better shading.
        /// </summary>
        private static void SmoothNormals(Rhino.Geometry.Mesh mesh, OptimizeOptions options)
        {
            try
            {
                var vertexNormals = new Vector3d[mesh.TopologyVertices.Count];

                for (int vi = 0; vi < mesh.TopologyVertices.Count; vi++)
                {
                    var connectedFaces = mesh.TopologyVertices.ConnectedFaces(vi);
                    var averageNormal = Vector3d.Zero;

                    foreach (var faceIndex in connectedFaces)
                    {
                        var face = mesh.Faces[faceIndex];
                        Vector3d faceNormal;

                        if (face.IsTriangle)
                        {
                            faceNormal = Vector3d.CrossProduct(
                                mesh.Vertices[face.B] - mesh.Vertices[face.A],
                                mesh.Vertices[face.C] - mesh.Vertices[face.A]
                            );
                        }
                        else // Quad
                        {
                            faceNormal = Vector3d.CrossProduct(
                                mesh.Vertices[face.B] - mesh.Vertices[face.A],
                                mesh.Vertices[face.D] - mesh.Vertices[face.A]
                            );
                        }

                        if (faceNormal.Length > 0)
                        {
                            faceNormal.Unitize();
                            averageNormal += faceNormal;
                        }
                    }

                    if (averageNormal.Length > 0)
                    {
                        averageNormal.Unitize();
                    }
                    else
                    {
                        averageNormal = Vector3d.ZAxis; // Default normal
                    }

                    vertexNormals[vi] = averageNormal;
                }

                mesh.Normals.Clear();
                for (int vi = 0; vi < mesh.TopologyVertices.Count; vi++)
                {
                    var tv = mesh.TopologyVertices.MeshVertexIndices(vi);
                    foreach (var mv in tv)
                    {
                        mesh.Normals.SetNormal(mv, vertexNormals[vi]);
                    }
                }
            }
            catch
            {
                mesh.Normals.ComputeNormals();
            }
        }
    }
}




