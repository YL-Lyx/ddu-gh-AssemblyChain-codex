using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Mesh
{
    /// <summary>
    /// Advanced mesh preprocessor
    /// Provides complete mesh optimization, repair and standardization functionality
    /// </summary>
    public static class MeshPreprocessor
    {
        /// <summary>
        /// Mesh preprocessing options
        /// </summary>
        public class PreprocessingOptions
        {
            public bool RepairMesh { get; set; } = true;
            public bool ComputeNormals { get; set; } = true;
            public bool OptimizeTopology { get; set; } = true;
            public bool RemoveDuplicates { get; set; } = true;
            public bool ValidateMesh { get; set; } = true;
            public double Tolerance { get; set; } = 1e-6;
            public int MaxFaceCount { get; set; } = 10000;
            public double MinEdgeLength { get; set; } = 1e-6;
            public double MaxEdgeLength { get; set; } = double.MaxValue;
            public bool TriangulateQuads { get; set; } = true;
            public bool UnifyNormals { get; set; } = false;
        }

        /// <summary>
        /// Preprocessing result
        /// </summary>
        public class PreprocessingResult
        {
            public Rhino.Geometry.Mesh ProcessedMesh { get; set; }
            public bool IsValid { get; set; }
            public string Report { get; set; }
            public int OriginalFaceCount { get; set; }
            public int ProcessedFaceCount { get; set; }
            public int OriginalVertexCount { get; set; }
            public int ProcessedVertexCount { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
        }

        /// <summary>
        /// Preprocesses mesh
        /// </summary>
        public static PreprocessingResult PreprocessMesh(Rhino.Geometry.Mesh inputMesh, PreprocessingOptions options = null)
        {
            options ??= new PreprocessingOptions();
            var result = new PreprocessingResult
            {
                OriginalFaceCount = inputMesh?.Faces.Count ?? 0,
                OriginalVertexCount = inputMesh?.Vertices.Count ?? 0
            };

            if (inputMesh == null || !inputMesh.IsValid)
            {
                result.Errors.Add("Input mesh is null or invalid");
                result.IsValid = false;
                return result;
            }

            try
            {
                // 深層複製網格
                var mesh = inputMesh.DuplicateMesh();
                if (mesh == null)
                {
                    result.Errors.Add("Failed to duplicate input mesh");
                    result.IsValid = false;
                    return result;
                }

                // 1. 網格驗證
                if (options.ValidateMesh)
                {
                    ValidateMesh(mesh, result, options);
                }

                // 2. 修復網格
                if (options.RepairMesh)
                {
                    RepairMesh(mesh, result, options);
                }

                // 2.5. 四邊面三角化
                if (options.TriangulateQuads)
                {
                    var facesBefore = mesh.Faces.Count;
                    mesh.Faces.ConvertQuadsToTriangles();
                    var facesAfter = mesh.Faces.Count;
                    if (facesAfter > facesBefore)
                    {
                        result.Warnings.Add($"Triangulated quads: {facesBefore} → {facesAfter}");
                    }
                }

                // 3. 移除重複頂點
                if (options.RemoveDuplicates)
                {
                    RemoveDuplicateVertices(mesh, result, options);
                }

                // 4. 優化拓撲
                if (options.OptimizeTopology)
                {
                    OptimizeTopology(mesh, result, options);
                }

                // 5. 計算法向量
                if (options.ComputeNormals)
                {
                    ComputeNormals(mesh, result, options);
                }

                // 5.5. 統一法向
                if (options.UnifyNormals)
                {
                    try
                    {
                        mesh.UnifyNormals();
                        result.Warnings.Add("Unified normals");
                    }
                    catch { }
                }

                // 6. 最終驗證
                FinalValidation(mesh, result, options);

                result.ProcessedMesh = mesh;
                result.ProcessedFaceCount = mesh.Faces.Count;
                result.ProcessedVertexCount = mesh.Vertices.Count;
                result.IsValid = result.Errors.Count == 0;

                GenerateReport(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Preprocessing failed: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// 從幾何體創建預處理的網格
        /// </summary>
        public static PreprocessingResult CreatePreprocessedMesh(GeometryBase geometry, PreprocessingOptions options = null)
        {
            options ??= new PreprocessingOptions();
            var result = new PreprocessingResult();

            try
            {
                Rhino.Geometry.Mesh mesh = null;

                if (geometry is Rhino.Geometry.Mesh inputMesh)
                {
                    mesh = inputMesh.DuplicateMesh();
                }
                else if (geometry is Rhino.Geometry.Brep brep)
                {
                    mesh = CreateMeshFromBrep(brep, options);
                }
                else if (geometry is Surface surface)
                {
                    mesh = CreateMeshFromSurface(surface, options);
                }
                else if (geometry is Extrusion extrusion)
                {
                    mesh = CreateMeshFromExtrusion(extrusion, options);
                }
                else if (geometry is SubD subd)
                {
                    mesh = CreateMeshFromSubD(subd, options);
                }
                else
                {
                    result.Errors.Add($"Unsupported geometry type: {geometry.GetType().Name}");
                    result.IsValid = false;
                    return result;
                }

                if (mesh == null)
                {
                    result.Errors.Add("Failed to create mesh from geometry");
                    result.IsValid = false;
                    return result;
                }

                // 預處理網格
                return PreprocessMesh(mesh, options);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh creation failed: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        #region 網格創建方法

        private static Rhino.Geometry.Mesh CreateMeshFromBrep(Rhino.Geometry.Brep brep, PreprocessingOptions options)
        {
            var meshingParameters = new MeshingParameters
            {
                Tolerance = options.Tolerance,
                RelativeTolerance = 0.01,
                MinimumTolerance = options.Tolerance,
                SimplePlanes = false,
                JaggedSeams = false,
                ClosedObjectPostProcess = true
            };

            var brepMeshes = Rhino.Geometry.Mesh.CreateFromBrep(brep, meshingParameters);
            if (brepMeshes == null || brepMeshes.Length == 0)
                return null;

            var combined = new Rhino.Geometry.Mesh();
            foreach (var brepMesh in brepMeshes)
            {
                if (brepMesh != null)
                    combined.Append(brepMesh);
            }

            return combined;
        }

        private static Rhino.Geometry.Mesh CreateMeshFromSurface(Surface surface, PreprocessingOptions options)
        {
            var brep = surface.ToBrep();
            return brep != null ? CreateMeshFromBrep(brep, options) : null;
        }

        private static Rhino.Geometry.Mesh CreateMeshFromExtrusion(Extrusion extrusion, PreprocessingOptions options)
        {
            var brep = extrusion.ToBrep();
            return brep != null ? CreateMeshFromBrep(brep, options) : null;
        }

        private static Rhino.Geometry.Mesh CreateMeshFromSubD(SubD subd, PreprocessingOptions options)
        {
            var brep = subd.ToBrep();
            return brep != null ? CreateMeshFromBrep(brep, options) : null;
        }

        #endregion

        #region 網格預處理方法

        private static void ValidateMesh(Rhino.Geometry.Mesh mesh, PreprocessingResult result, PreprocessingOptions options)
        {
            if (mesh.Faces.Count == 0)
            {
                result.Errors.Add("Mesh has no faces");
                return;
            }

            if (mesh.Vertices.Count == 0)
            {
                result.Errors.Add("Mesh has no vertices");
                return;
            }

            // 檢查面數限制
            if (mesh.Faces.Count > options.MaxFaceCount)
            {
                result.Warnings.Add($"Mesh face count ({mesh.Faces.Count}) exceeds maximum ({options.MaxFaceCount})");
            }

            // 檢查邊長 - 使用簡化的方法
            var edges = new List<Line>();
            for (int fi = 0; fi < mesh.Faces.Count; fi++)
            {
                var face = mesh.Faces[fi];
                if (face.IsTriangle)
                {
                    var v0 = mesh.Vertices[face.A];
                    var v1 = mesh.Vertices[face.B];
                    var v2 = mesh.Vertices[face.C];

                    edges.Add(new Line(v0, v1));
                    edges.Add(new Line(v1, v2));
                    edges.Add(new Line(v2, v0));
                }
                else
                {
                    var v0 = mesh.Vertices[face.A];
                    var v1 = mesh.Vertices[face.B];
                    var v2 = mesh.Vertices[face.C];
                    var v3 = mesh.Vertices[face.D];

                    edges.Add(new Line(v0, v1));
                    edges.Add(new Line(v1, v2));
                    edges.Add(new Line(v2, v3));
                    edges.Add(new Line(v3, v0));
                }
            }

            foreach (var edge in edges)
            {
                var length = edge.Length;

                if (length < options.MinEdgeLength)
                {
                    result.Warnings.Add($"Edge too short: {length:F6} < {options.MinEdgeLength:F6}");
                }

                if (length > options.MaxEdgeLength)
                {
                    result.Warnings.Add($"Edge too long: {length:F6} > {options.MaxEdgeLength:F6}");
                }
            }
        }

        private static void RepairMesh(Rhino.Geometry.Mesh mesh, PreprocessingResult result, PreprocessingOptions options)
        {
            var originalFaceCount = mesh.Faces.Count;
            var originalVertexCount = mesh.Vertices.Count;

            // 修復退化面
            mesh.Faces.CullDegenerateFaces();

            // 修復法向量
            if (options.ComputeNormals)
            {
                mesh.Normals.Clear();
                mesh.Normals.ComputeNormals();
            }

            var repairedFaceCount = mesh.Faces.Count;
            var repairedVertexCount = mesh.Vertices.Count;

            if (repairedFaceCount != originalFaceCount)
            {
                result.Warnings.Add($"Repaired faces: {originalFaceCount} → {repairedFaceCount}");
            }

            if (repairedVertexCount != originalVertexCount)
            {
                result.Warnings.Add($"Repaired vertices: {originalVertexCount} → {repairedVertexCount}");
            }
        }

        private static void RemoveDuplicateVertices(Rhino.Geometry.Mesh mesh, PreprocessingResult result, PreprocessingOptions options)
        {
            var originalVertexCount = mesh.Vertices.Count;
            mesh.Compact();
            var processedVertexCount = mesh.Vertices.Count;

            if (processedVertexCount != originalVertexCount)
            {
                result.Warnings.Add($"Removed duplicate/unreferenced vertices: {originalVertexCount} → {processedVertexCount}");
            }
        }

        private static void OptimizeTopology(Rhino.Geometry.Mesh mesh, PreprocessingResult result, PreprocessingOptions options)
        {
            var originalFaceCount = mesh.Faces.Count;

            mesh.Compact();
            mesh.Faces.CullDegenerateFaces();

            var optimizedFaceCount = mesh.Faces.Count;

            if (optimizedFaceCount != originalFaceCount)
            {
                result.Warnings.Add($"Optimized topology: {originalFaceCount} → {optimizedFaceCount} faces");
            }
        }

        private static void ComputeNormals(Rhino.Geometry.Mesh mesh, PreprocessingResult result, PreprocessingOptions options)
        {
            try
            {
                mesh.Normals.Clear();
                mesh.Normals.ComputeNormals();
                result.Warnings.Add("Successfully computed normals");
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Normal computation failed: {ex.Message}");
            }
        }

        private static void FinalValidation(Rhino.Geometry.Mesh mesh, PreprocessingResult result, PreprocessingOptions options)
        {
            if (mesh.Faces.Count == 0)
            {
                result.Errors.Add("Mesh has no faces after preprocessing");
                return;
            }

            if (mesh.Vertices.Count == 0)
            {
                result.Errors.Add("Mesh has no vertices after preprocessing");
                return;
            }

            if (!mesh.IsClosed)
            {
                result.Warnings.Add("Mesh is not closed");
            }

            bool isOriented, hasBoundary;
            var isManifold = mesh.IsManifold(true, out isOriented, out hasBoundary);
            if (!isManifold)
            {
                result.Warnings.Add("Mesh is not manifold");
            }
        }

        private static void GenerateReport(PreprocessingResult result)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Mesh Preprocessing Report ===");
            report.AppendLine($"Original: {result.OriginalVertexCount} vertices, {result.OriginalFaceCount} faces");
            report.AppendLine($"Processed: {result.ProcessedVertexCount} vertices, {result.ProcessedFaceCount} faces");
            report.AppendLine($"Valid: {result.IsValid}");

            if (result.Warnings.Count > 0)
            {
                report.AppendLine("\nWarnings:");
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"- {warning}");
                }
            }

            if (result.Errors.Count > 0)
            {
                report.AppendLine("\nErrors:");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"- {error}");
                }
            }

            result.Report = report.ToString();
        }

        #endregion
    }
}




