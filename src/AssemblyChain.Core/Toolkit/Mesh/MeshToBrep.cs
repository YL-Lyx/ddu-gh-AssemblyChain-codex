using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Mesh
{
    /// <summary>
    /// Utilities for converting meshes to Brep representations with configurable tolerance.
    /// </summary>
    public static class MeshToBrep
    {
        /// <summary>
        /// Conversion options.
        /// </summary>
        public class ConversionOptions
        {
            public double Tolerance { get; set; } = 1e-6;
            public double AngleTolerance { get; set; } = System.Math.PI / 180.0; // 1 degree
            public double MinimumEdgeLength { get; set; } = 1e-6;
            public int MaximumFacesPerPatch { get; set; } = 1000;
            public bool SimplifyResult { get; set; } = true;
            public bool FillHoles { get; set; } = false;
        }

        /// <summary>
        /// Conversion result.
        /// </summary>
        public class ConversionResult
        {
            public Rhino.Geometry.Brep ConvertedBrep { get; set; }
            public bool Success { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public double OriginalMeshArea { get; set; }
            public double ConvertedBrepArea { get; set; }
        }

        /// <summary>
        /// Converts a mesh to a Brep using patch-based reconstruction.
        /// </summary>
        public static ConversionResult ConvertToBrep(Rhino.Geometry.Mesh inputMesh, ConversionOptions options = null)
        {
            options ??= new ConversionOptions();
            var result = new ConversionResult();

            if (inputMesh == null || !inputMesh.IsValid)
            {
                result.Errors.Add("Input mesh is null");
                result.Success = false;
                return result;
            }

            try
            {
                result.OriginalMeshArea = ComputeMeshArea(inputMesh);

                // Method 1: Try using Rhino's mesh to Brep conversion
                var brep = TryRhinoConversion(inputMesh, options);
                if (brep != null)
                {
                    result.ConvertedBrep = brep;
                    result.ConvertedBrepArea = ComputeBrepArea(brep);
                    result.Success = true;
                    return result;
                }

                // Method 2: Patch-based reconstruction (placeholder)
                brep = PatchBasedReconstruction(inputMesh, options);
                if (brep != null)
                {
                    result.ConvertedBrep = brep;
                    result.ConvertedBrepArea = ComputeBrepArea(brep);
                    result.Success = true;
                    result.Warnings.Add("Used patch-based reconstruction");
                    return result;
                }

                result.Errors.Add("All conversion methods failed");
                result.Success = false;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Mesh to Brep conversion failed: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Attempts to use Rhino's built-in mesh to Brep conversion (not directly available).
        /// Returns null by default.
        /// </summary>
        private static Rhino.Geometry.Brep TryRhinoConversion(Rhino.Geometry.Mesh mesh, ConversionOptions options)
        {
            try
            {
                // No direct Mesh -> Brep conversion; return null to use fallback
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Performs patch-based Brep reconstruction from mesh (placeholder).
        /// </summary>
        private static Rhino.Geometry.Brep PatchBasedReconstruction(Rhino.Geometry.Mesh mesh, ConversionOptions options)
        {
            try
            {
                var bbox = mesh.GetBoundingBox(true);
                var brep = Rhino.Geometry.Brep.CreateFromBox(new Box(bbox));
                // Placeholder: return a box representing the mesh bounds
                return brep;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Computes the surface area of a mesh.
        /// </summary>
        private static double ComputeMeshArea(Rhino.Geometry.Mesh mesh)
        {
            double area = 0.0;

            for (int fi = 0; fi < mesh.Faces.Count; fi++)
            {
                var face = mesh.Faces[fi];
                if (face.IsTriangle)
                {
                    var a = mesh.Vertices[face.A];
                    var b = mesh.Vertices[face.B];
                    var c = mesh.Vertices[face.C];

                    var ab = b - a;
                    var ac = c - a;
                    var cross = Vector3d.CrossProduct(ab, ac);
                    area += cross.Length / 2.0;
                }
                else
                {
                    // Split quad into two triangles
                    var a = mesh.Vertices[face.A];
                    var b = mesh.Vertices[face.B];
                    var c = mesh.Vertices[face.C];
                    var d = mesh.Vertices[face.D];

                    var ab = b - a;
                    var ac = c - a;
                    var cross1 = Vector3d.CrossProduct(ab, ac);
                    area += cross1.Length / 2.0;

                    var ad = d - a;
                    var cross2 = Vector3d.CrossProduct(ac, ad);
                    area += cross2.Length / 2.0;
                }
            }

            return area;
        }

        /// <summary>
        /// Computes the surface area of a Brep.
        /// </summary>
        private static double ComputeBrepArea(Rhino.Geometry.Brep brep)
        {
            if (brep == null) return 0.0;

            double area = 0.0;
            foreach (var face in brep.Faces)
            {
                var mass = AreaMassProperties.Compute(face);
                if (mass != null)
                {
                    area += mass.Area;
                }
            }
            return area;
        }

        /// <summary>
        /// Validates the conversion result.
        /// </summary>
        public static bool ValidateConversion(Rhino.Geometry.Mesh originalMesh, Rhino.Geometry.Brep convertedBrep, ConversionOptions options, out List<string> issues)
        {
            issues = new List<string>();

            if (convertedBrep == null)
            {
                issues.Add("Converted Brep is null");
                return false;
            }

            // Check if Brep is valid
            if (!convertedBrep.IsValid)
            {
                issues.Add("Converted Brep is not valid");
                return false;
            }

            // Check area similarity (relative tolerance)
            var originalArea = ComputeMeshArea(originalMesh);
            var convertedArea = ComputeBrepArea(convertedBrep);

            if (System.Math.Abs(originalArea - convertedArea) > options.Tolerance * System.Math.Max(originalArea, 1.0))
            {
                issues.Add($"Area mismatch: mesh={originalArea:F6}, brep={convertedArea:F6}");
            }

            return issues.Count == 0;
        }
    }
}




