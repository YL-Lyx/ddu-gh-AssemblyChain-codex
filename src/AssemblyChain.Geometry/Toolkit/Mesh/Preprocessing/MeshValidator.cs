using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Mesh.Preprocessing
{
    /// <summary>
    /// Specialized mesh validation utilities
    /// </summary>
    public static class MeshValidator
    {
        /// <summary>
        /// Validation options
        /// </summary>
        public class ValidationOptions
        {
            public bool CheckDegenerateFaces { get; set; } = true;
            public bool CheckNormals { get; set; } = true;
            public bool CheckBoundingBox { get; set; } = true;
            public bool CheckTopology { get; set; } = true;
            public double Tolerance { get; set; } = 1e-6;
            public int MaxDegenerateFacePercentage { get; set; } = 80;
        }

        /// <summary>
        /// Validation result
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; } = true;
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public int DegenerateFaceCount { get; set; }
            public int TotalFaceCount { get; set; }
            public bool HasValidNormals { get; set; }
            public bool HasValidBoundingBox { get; set; }
        }

        /// <summary>
        /// Validates mesh quality and topology
        /// </summary>
        public static ValidationResult ValidateMesh(Rhino.Geometry.Mesh mesh, ValidationOptions options = null)
        {
            options ??= new ValidationOptions();
            var result = new ValidationResult
            {
                TotalFaceCount = mesh?.Faces.Count ?? 0
            };

            if (mesh == null)
            {
                result.Errors.Add("Mesh is null");
                result.IsValid = false;
                return result;
            }

            if (!mesh.IsValid)
            {
                result.Errors.Add("Mesh is not valid according to Rhino");
                result.IsValid = false;
                return result;
            }

            if (mesh.Vertices.Count < 3)
            {
                result.Errors.Add($"Mesh has insufficient vertices ({mesh.Vertices.Count})");
                result.IsValid = false;
                return result;
            }

            if (mesh.Faces.Count == 0)
            {
                result.Errors.Add("Mesh has no faces");
                result.IsValid = false;
                return result;
            }

            // Check topology
            if (options.CheckTopology)
            {
                CheckTopology(mesh, result, options);
            }

            // Check degenerate faces
            if (options.CheckDegenerateFaces)
            {
                CheckDegenerateFaces(mesh, result, options);
            }

            // Check normals
            if (options.CheckNormals)
            {
                CheckNormals(mesh, result);
            }

            // Check bounding box
            if (options.CheckBoundingBox)
            {
                CheckBoundingBox(mesh, result, options);
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Performs final validation after preprocessing
        /// </summary>
        public static ValidationResult FinalValidation(Rhino.Geometry.Mesh mesh, ValidationOptions options = null)
        {
            options ??= new ValidationOptions { Tolerance = 1e-3 }; // More lenient final validation
            return ValidateMesh(mesh, options);
        }

        /// <summary>
        /// Legacy validation method for backward compatibility
        /// </summary>
        public static bool ValidateMeshForContactDetection(Rhino.Geometry.Mesh mesh, string meshName, out string errorMessage)
        {
            var result = ValidateMesh(mesh, new ValidationOptions { Tolerance = 1e-6 });
            errorMessage = result.IsValid ? null : string.Join("; ", result.Errors);
            return result.IsValid;
        }

        private static void CheckTopology(Rhino.Geometry.Mesh mesh, ValidationResult result, ValidationOptions options)
        {
            try
            {
                // Check for naked edges (boundary edges)
                var nakedEdges = mesh.GetNakedEdges();
                if (nakedEdges != null && nakedEdges.Length > 0)
                {
                    result.Warnings.Add($"Mesh has {nakedEdges.Length} naked edges (open boundaries)");
                }

                // Check for non-manifold edges (simplified check)
                // Note: Rhino doesn't provide direct non-manifold edge detection
                // This is a placeholder for more advanced mesh validation

                // Check for duplicate faces
                var duplicateFaces = FindDuplicateFaces(mesh);
                if (duplicateFaces.Count > 0)
                {
                    result.Errors.Add($"Mesh has {duplicateFaces.Count} duplicate faces");
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Topology check failed: {ex.Message}");
            }
        }

        private static void CheckDegenerateFaces(Rhino.Geometry.Mesh mesh, ValidationResult result, ValidationOptions options)
        {
            try
            {
                result.DegenerateFaceCount = 0;

                // Sample check for performance (don't check all faces if mesh is very large)
                int checkCount = System.Math.Min(mesh.Faces.Count, System.Math.Max(1000, mesh.Faces.Count / 10));

                for (int i = 0; i < checkCount; i++)
                {
                    if (IsDegenerateFace(mesh, i))
                    {
                        result.DegenerateFaceCount++;
                    }
                }

                // Extrapolate for full mesh
                double estimatedDegenerate = (double)result.DegenerateFaceCount * mesh.Faces.Count / checkCount;
                double degeneratePercentage = estimatedDegenerate * 100.0 / mesh.Faces.Count;

                if (degeneratePercentage > options.MaxDegenerateFacePercentage)
                {
                    result.Errors.Add($"Too many degenerate faces ({degeneratePercentage:F1}%, threshold: {options.MaxDegenerateFacePercentage}%)");
                    result.IsValid = false;
                }
                else if (result.DegenerateFaceCount > 0)
                {
                    result.Warnings.Add($"Found {result.DegenerateFaceCount} degenerate faces in sample of {checkCount} faces");
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Degenerate face check failed: {ex.Message}");
            }
        }

        private static void CheckNormals(Rhino.Geometry.Mesh mesh, ValidationResult result)
        {
            try
            {
                if (mesh.FaceNormals.Count == 0)
                {
                    result.Warnings.Add("Mesh has no face normals computed");
                    result.HasValidNormals = false;
                    return;
                }

                if (mesh.FaceNormals.Count != mesh.Faces.Count)
                {
                    result.Errors.Add($"Face normal count ({mesh.FaceNormals.Count}) doesn't match face count ({mesh.Faces.Count})");
                    result.HasValidNormals = false;
                    result.IsValid = false;
                    return;
                }

                // Check for zero-length normals
                int zeroNormals = 0;
                for (int i = 0; i < mesh.FaceNormals.Count; i++)
                {
                    if (mesh.FaceNormals[i].Length < 1e-10)
                    {
                        zeroNormals++;
                    }
                }

                if (zeroNormals > 0)
                {
                    result.Errors.Add($"Found {zeroNormals} faces with zero-length normals");
                    result.HasValidNormals = false;
                    result.IsValid = false;
                }
                else
                {
                    result.HasValidNormals = true;
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Normal check failed: {ex.Message}");
                result.HasValidNormals = false;
            }
        }

        private static void CheckBoundingBox(Rhino.Geometry.Mesh mesh, ValidationResult result, ValidationOptions options)
        {
            try
            {
                var bbox = mesh.GetBoundingBox(true);
                if (!bbox.IsValid)
                {
                    result.Errors.Add("Mesh bounding box is invalid");
                    result.HasValidBoundingBox = false;
                    result.IsValid = false;
                    return;
                }

                var diagonal = bbox.Diagonal;
                if (diagonal.Length < options.Tolerance)
                {
                    result.Errors.Add($"Mesh bounding box is too small (diagonal: {diagonal.Length:F6})");
                    result.HasValidBoundingBox = false;
                    result.IsValid = false;
                }
                else
                {
                    result.HasValidBoundingBox = true;
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Bounding box check failed: {ex.Message}");
                result.HasValidBoundingBox = false;
            }
        }

        private static bool IsDegenerateFace(Rhino.Geometry.Mesh mesh, int faceIndex)
        {
            try
            {
                var face = mesh.Faces[faceIndex];
                var vertices = new Point3d[] {
                    mesh.Vertices[face.A],
                    mesh.Vertices[face.B],
                    mesh.Vertices[face.C]
                };

                if (face.IsQuad)
                {
                    vertices = new Point3d[] {
                        mesh.Vertices[face.A],
                        mesh.Vertices[face.B],
                        mesh.Vertices[face.C],
                        mesh.Vertices[face.D]
                    };
                }

                // Check for duplicate vertices
                for (int i = 0; i < vertices.Length; i++)
                {
                    for (int j = i + 1; j < vertices.Length; j++)
                    {
                        if (vertices[i].DistanceTo(vertices[j]) < 1e-10)
                            return true;
                    }
                }

                // Check for collinear vertices (zero area)
                if (vertices.Length == 3)
                {
                    var v1 = vertices[1] - vertices[0];
                    var v2 = vertices[2] - vertices[0];
                    var cross = Vector3d.CrossProduct(v1, v2);
                    return cross.Length < 1e-10;
                }

                return false;
            }
            catch
            {
                return true; // If calculation fails, consider it degenerate
            }
        }

        private static List<int> FindDuplicateFaces(Rhino.Geometry.Mesh mesh)
        {
            var duplicateIndices = new List<int>();
            var faceSignatures = new Dictionary<string, int>();

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[i];
                var signature = $"{face.A},{face.B},{face.C},{(face.IsQuad ? face.D.ToString() : "")}";

                if (faceSignatures.ContainsKey(signature))
                {
                    duplicateIndices.Add(i);
                }
                else
                {
                    faceSignatures[signature] = i;
                }
            }

            return duplicateIndices;
        }
    }
}
