using System.Collections.Generic;

namespace AssemblyChain.Geometry.Toolkit.Mesh
{
    /// <summary>
    /// Partial definition containing strongly-typed option/result objects.
    /// </summary>
    public static partial class MeshPreprocessor
    {
        /// <summary>
        /// Mesh preprocessing options.
        /// </summary>
        public class PreprocessingOptions
        {
            public double Tolerance { get; set; } = 1e-6;

            public bool ValidateMesh { get; set; } = true;

            public Preprocessing.MeshValidator.ValidationOptions ValidationOptions { get; set; } = new();

            public bool RepairMesh { get; set; } = true;

            public Preprocessing.MeshRepair.RepairOptions RepairOptions { get; set; } = new();

            public bool OptimizeMesh { get; set; } = true;

            public Preprocessing.MeshOptimizer.OptimizationOptions OptimizationOptions { get; set; } = new();

            public bool TriangulateQuads { get; set; } = true;

            public int MaxFaceCount { get; set; } = 10000;

            public bool ComputeNormals { get; set; } = true;

            /// <summary>
            /// Creates balanced presets enabling all modules.
            /// </summary>
            public static PreprocessingOptions CreateBalanced()
            {
                return new PreprocessingOptions();
            }

            /// <summary>
            /// Creates a fast preset skipping expensive checks.
            /// </summary>
            public static PreprocessingOptions CreateFast()
            {
                return new PreprocessingOptions
                {
                    ValidateMesh = false,
                    RepairMesh = false,
                    OptimizeMesh = false,
                    TriangulateQuads = false,
                    ComputeNormals = false
                };
            }
        }

        /// <summary>
        /// Preprocessing result with detailed sub-operation feedback.
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

            public List<string> Warnings { get; set; } = new();

            public List<string> Errors { get; set; } = new();

            public Preprocessing.MeshValidator.ValidationResult ValidationResult { get; set; }

            public Preprocessing.MeshRepair.RepairResult RepairResult { get; set; }

            public Preprocessing.MeshOptimizer.OptimizationResult OptimizationResult { get; set; }
        }
    }
}
