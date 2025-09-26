using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Toolkit;

namespace AssemblyChain.Core.Contact.Detection.NarrowPhase
{
    /// <summary>
    /// 专门处理混合几何类型接触检测的模块
    /// </summary>
    public static class MixedGeoContactDetector
    {
        /// <summary>
        /// 检测混合几何类型部件之间的接触
        /// </summary>
        public static List<ContactData> DetectMixedGeoContacts(
            Part partA, Part partB, DetectionOptions options)
        {
            var contacts = new List<ContactData>();

            // 验证输入
            if (!partA.HasValidGeometry || !partB.HasValidGeometry)
                return contacts;

            System.Diagnostics.Debug.WriteLine($"[MixedGeoContactDetector] Processing Mixed Geometry: {partA.Name} vs {partB.Name}");
            System.Diagnostics.Debug.WriteLine($"  PartA: {partA.OriginalGeometryType}, PartB: {partB.OriginalGeometryType}");

            try
            {
                // 获取几何类型信息
                var brepA = partA.OriginalGeometry as Brep;
                var brepB = partB.OriginalGeometry as Brep;
                var meshA = partA.Mesh;
                var meshB = partB.Mesh;

                System.Diagnostics.Debug.WriteLine($"  Available geometry:");
                System.Diagnostics.Debug.WriteLine($"    BrepA: {(brepA != null ? "✓" : "✗")}, MeshA: {(meshA != null ? "✓" : "✗")}");
                System.Diagnostics.Debug.WriteLine($"    BrepB: {(brepB != null ? "✓" : "✗")}, MeshB: {(meshB != null ? "✓" : "✗")}");

                // 策略：优先使用Mesh，如果没有则转换Brep为Mesh
                var processedMeshA = GetProcessedMesh(partA, "A");
                var processedMeshB = GetProcessedMesh(partB, "B");

                if (processedMeshA != null && processedMeshB != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MixedGeoContactDetector] Converted to Mesh-Mesh detection");

                    // 使用Mesh检测器进行最终检测
                    var result = Toolkit.Intersection.MeshMeshIntersect.DetectContactsBasedOnIntersection(
                        processedMeshA, processedMeshB, options,
                        $"P{partA.Id:D4}", $"P{partB.Id:D4}");

                    contacts.AddRange(result.Contacts);

                    System.Diagnostics.Debug.WriteLine($"[MixedGeoContactDetector] Found {result.Contacts.Count} contacts after conversion");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MixedGeoContactDetector] Could not convert to compatible geometry types");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MixedGeoContactDetector] Error in mixed geometry detection: {ex.Message}");
            }

            return contacts;
        }

        /// <summary>
        /// 获取部件的处理后Mesh几何
        /// </summary>
        private static Rhino.Geometry.Mesh GetProcessedMesh(Part part, string label)
        {
            // 1. 优先使用现有的Mesh
            if (part.Mesh != null)
            {
                System.Diagnostics.Debug.WriteLine($"[{label}] Using existing mesh: {part.Mesh.Vertices.Count} vertices");
                return part.Mesh;
            }

            // 2. 如果没有Mesh，尝试从Brep转换
            var brep = part.OriginalGeometry as Brep;
            if (brep != null)
            {
                System.Diagnostics.Debug.WriteLine($"[{label}] Converting Brep to Mesh...");
                try
                {
                    var meshes = Rhino.Geometry.Mesh.CreateFromBrep(brep, MeshingParameters.Default);
                    if (meshes != null && meshes.Length > 0)
                    {
                        // 合并多个Mesh为一个
                        var combinedMesh = meshes[0];
                        for (int i = 1; i < meshes.Length; i++)
                        {
                            combinedMesh.Append(meshes[i]);
                        }

                        System.Diagnostics.Debug.WriteLine($"[{label}] Successfully converted: {combinedMesh.Vertices.Count} vertices, {combinedMesh.Faces.Count} faces");
                        return combinedMesh;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[{label}] Failed to convert Brep to Mesh: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[{label}] No compatible geometry available");
            return null;
        }
    }
}
