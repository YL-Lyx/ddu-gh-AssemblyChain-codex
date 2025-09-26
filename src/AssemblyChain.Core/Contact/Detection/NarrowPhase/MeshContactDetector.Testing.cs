using System;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Contact.Detection.NarrowPhase
{
    /// <summary>
    /// Partial class containing test utilities for mesh contact detection.
    /// </summary>
    public static partial class MeshContactDetector
    {
        /// <summary>
        /// Helper utilities primarily used by tests and samples.
        /// </summary>
        public static class MeshContactTestUtilities
        {
            /// <summary>
            /// Creates a cube mesh centred at the provided point.
            /// </summary>
            public static Mesh CreateTestCube(Point3d center, double size)
            {
                var mesh = new Mesh();
                var half = size * 0.5;

                mesh.Vertices.Add(center.X - half, center.Y - half, center.Z - half);
                mesh.Vertices.Add(center.X + half, center.Y - half, center.Z - half);
                mesh.Vertices.Add(center.X + half, center.Y + half, center.Z - half);
                mesh.Vertices.Add(center.X - half, center.Y + half, center.Z - half);
                mesh.Vertices.Add(center.X - half, center.Y - half, center.Z + half);
                mesh.Vertices.Add(center.X + half, center.Y - half, center.Z + half);
                mesh.Vertices.Add(center.X + half, center.Y + half, center.Z + half);
                mesh.Vertices.Add(center.X - half, center.Y + half, center.Z + half);

                mesh.Faces.AddFace(0, 1, 2, 3);
                mesh.Faces.AddFace(4, 7, 6, 5);
                mesh.Faces.AddFace(0, 4, 5, 1);
                mesh.Faces.AddFace(2, 6, 7, 3);
                mesh.Faces.AddFace(0, 3, 7, 4);
                mesh.Faces.AddFace(1, 5, 6, 2);

                mesh.Normals.ComputeNormals();
                mesh.FaceNormals.ComputeFaceNormals();
                mesh.Compact();

                return mesh;
            }

            /// <summary>
            /// Runs a basic contact test with verbose logging.
            /// </summary>
            public static void RunBasicContactTest(Part partA, Part partB)
            {
                var options = MeshContactDetector.EnhancedDetectionOptions.CreatePreset(
                    MeshContactDetector.EnhancedDetectionOptions.QualityPreset.Balanced);
                options.EnablePerformanceMonitoring = true;

                var contacts = MeshContactDetector.DetectMeshContactsEnhanced(partA, partB, options);

                System.Diagnostics.Debug.WriteLine($"Test completed. Found {contacts.Count} contacts");
            }

            /// <summary>
            /// Runs a minimal performance comparison across presets.
            /// </summary>
            public static void RunPerformanceTest(Part partA, Part partB)
            {
                var presets = new[]
                {
                    MeshContactDetector.EnhancedDetectionOptions.QualityPreset.Fast,
                    MeshContactDetector.EnhancedDetectionOptions.QualityPreset.Balanced,
                    MeshContactDetector.EnhancedDetectionOptions.QualityPreset.Precise
                };

                foreach (var preset in presets)
                {
                    var options = MeshContactDetector.EnhancedDetectionOptions.CreatePreset(preset);
                    options.EnablePerformanceMonitoring = true;

                    var startTime = DateTime.Now;
                    var contacts = MeshContactDetector.DetectMeshContactsEnhanced(partA, partB, options);
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

                    System.Diagnostics.Debug.WriteLine($"Preset {preset}: {contacts.Count} contacts in {elapsed:F2}ms");
                }
            }
        }
    }
}
