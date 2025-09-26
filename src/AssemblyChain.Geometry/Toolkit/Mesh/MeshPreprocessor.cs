using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Mesh
{
    /// <summary>
    /// Advanced mesh preprocessor - refactored modular design
    /// Orchestrates mesh validation, repair, optimization and standardization
    /// </summary>
    public static partial class MeshPreprocessor
    {
        /// <summary>
        /// Preprocesses mesh using modular components
        /// </summary>
        public static PreprocessingResult PreprocessMesh(Rhino.Geometry.Mesh inputMesh, PreprocessingOptions options = null)
        {
            options ??= PreprocessingOptions.CreateBalanced();
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
                // Deep copy mesh
                var mesh = inputMesh.DuplicateMesh();
                if (mesh == null)
                {
                    result.Errors.Add("Failed to duplicate input mesh");
                    result.IsValid = false;
                    return result;
                }

                // 1. Validate mesh
                if (options.ValidateMesh)
                {
                    result.ValidationResult = Preprocessing.MeshValidator.ValidateMesh(mesh, options.ValidationOptions);
                    if (!result.ValidationResult.IsValid)
                    {
                        result.Errors.AddRange(result.ValidationResult.Errors);
                        result.Warnings.AddRange(result.ValidationResult.Warnings);
                    }
                    else
                    {
                        result.Warnings.AddRange(result.ValidationResult.Warnings);
                    }
                }

                // 2. Repair mesh
                if (options.RepairMesh)
                {
                    result.RepairResult = Preprocessing.MeshRepair.RepairMesh(mesh, options.RepairOptions);
                    if (!result.RepairResult.Success)
                    {
                        result.Errors.AddRange(result.RepairResult.Errors);
                    }
                    result.Warnings.AddRange(result.RepairResult.Warnings);
                }

                // 3. Triangulate quads if requested
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

                // 4. Optimize mesh
                if (options.OptimizeMesh)
                {
                    result.OptimizationResult = Preprocessing.MeshOptimizer.OptimizeMesh(mesh, options.OptimizationOptions);
                    if (!result.OptimizationResult.Success)
                    {
                        result.Errors.AddRange(result.OptimizationResult.Errors);
                    }
                    result.Warnings.AddRange(result.OptimizationResult.Warnings);
                }

                // 5. Compute normals if requested
                if (options.ComputeNormals)
                {
                    try
                    {
                        mesh.Normals.ComputeNormals();
                        mesh.FaceNormals.ComputeFaceNormals();
                        result.Warnings.Add("Computed mesh normals");
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to compute normals: {ex.Message}");
                    }
                }

                // 6. Final validation
                if (options.ValidateMesh)
                {
                    var finalValidation = Preprocessing.MeshValidator.FinalValidation(mesh);
                    if (!finalValidation.IsValid)
                    {
                        result.Errors.AddRange(finalValidation.Errors);
                        result.Warnings.AddRange(finalValidation.Warnings);
                    }
                }

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
        /// Creates and preprocesses mesh from geometry
        /// </summary>
        public static PreprocessingResult CreatePreprocessedMesh(GeometryBase geometry, PreprocessingOptions options = null)
        {
            options ??= PreprocessingOptions.CreateBalanced();
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

                // Preprocess the mesh
                return PreprocessMesh(mesh, options);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh creation failed: {ex.Message}");
                result.IsValid = false;
                return result;
            }
        }

        #region Mesh Creation Methods

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

        /// <summary>
        /// Generates a summary report of the preprocessing operation
        /// </summary>
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

            // Add sub-operation summaries
            if (result.ValidationResult != null)
            {
                report.AppendLine($"\nValidation: {result.ValidationResult.IsValid} ({result.ValidationResult.TotalFaceCount} faces checked)");
            }

            if (result.RepairResult != null)
            {
                report.AppendLine($"Repair: {result.RepairResult.Success} ({result.RepairResult.HolesFilled} holes filled, {result.RepairResult.DuplicateFacesRemoved} duplicates removed)");
            }

            if (result.OptimizationResult != null)
            {
                report.AppendLine($"Optimization: {result.OptimizationResult.Success} ({result.OptimizationResult.VertexReductionPercentage:F1}% vertex reduction)");
            }

            result.Report = report.ToString();
        }
    }
}