using System;
using System.Collections.Generic;
using System.Linq;
// Updated to use Domain architecture - types now in AssemblyChain.Core.Domain.*
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Contracts;

namespace AssemblyChain.Core.Contact.Detection.NarrowPhase
{
    /// <summary>
    /// 窄相位檢測協調器 - 負責路由到專門的檢測模塊
    /// </summary>
    public static class NarrowPhaseDetection
    {
        /// <summary>
        /// 檢測一對物體之間的接觸
        /// </summary>
        public static List<ContactData> DetectContactsForPair(
            Part A, Part B, double tol=1e-3, double minArea=0.0, double minEdgeLen=0.0, DetectionOptions options = default)
        {
            var res = new List<ContactData>();
            if (!A.HasValidGeometry || !B.HasValidGeometry) return res;

            // 詳細調試信息：輸入分析
            System.Diagnostics.Debug.WriteLine($"\n=== Contact Detection Start ===");
            System.Diagnostics.Debug.WriteLine($"{A.Name} ({A.OriginalGeometryType}) vs {B.Name} ({B.OriginalGeometryType})");
            System.Diagnostics.Debug.WriteLine($"Part A: ID={A.Id}, Name={A.Name}, OriginalType={A.OriginalGeometryType}");
            System.Diagnostics.Debug.WriteLine($"Part B: ID={B.Id}, Name={B.Name}, OriginalType={B.OriginalGeometryType}");
            System.Diagnostics.Debug.WriteLine($"Tolerance: {tol:F6}, MinArea: {minArea:F6}, MinEdgeLen: {minEdgeLen:F6}");

            // 判斷幾何體類型和可用性
            var brepA = A.OriginalGeometry as Brep;
            var brepB = B.OriginalGeometry as Brep;

            System.Diagnostics.Debug.WriteLine($"Available geometry:");
            System.Diagnostics.Debug.WriteLine($"  - BrepA: {(brepA != null ? "✓" : "✗")} (Faces: {brepA?.Faces.Count ?? 0})");
            System.Diagnostics.Debug.WriteLine($"  - BrepB: {(brepB != null ? "✓" : "✗")} (Faces: {brepB?.Faces.Count ?? 0})");
            System.Diagnostics.Debug.WriteLine($"  - MeshA: {(A.Mesh != null ? "✓" : "✗")} (Vertices: {A.Mesh?.Vertices.Count ?? 0}, Faces: {A.Mesh?.Faces.Count ?? 0})");
            System.Diagnostics.Debug.WriteLine($"  - MeshB: {(B.Mesh != null ? "✓" : "✗")} (Vertices: {B.Mesh?.Vertices.Count ?? 0}, Faces: {B.Mesh?.Faces.Count ?? 0})");

            var startTime = DateTime.Now;

            // 根據幾何類型路由到對應的專門檢測器
            if (brepA != null && brepB != null)
            {
                System.Diagnostics.Debug.WriteLine($"→ Routing to BrepContactDetector");
                res.AddRange(BrepContactDetector.DetectBrepContacts(A, B, options));
            }
            else if (A.Mesh != null && B.Mesh != null)
            {
                System.Diagnostics.Debug.WriteLine($"→ Routing to MeshContactDetector");
                res.AddRange(MeshContactDetector.DetectMeshContacts(A, B, options));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"→ Routing to MixedGeoContactDetector");
                res.AddRange(MixedGeoContactDetector.DetectMixedGeoContacts(A, B, options));
            }

            var endTime = DateTime.Now;
            var duration = (endTime - startTime).TotalMilliseconds;

            System.Diagnostics.Debug.WriteLine($"=== Contact Detection Results ===");
            System.Diagnostics.Debug.WriteLine($"Total contacts: {res.Count}");
            System.Diagnostics.Debug.WriteLine($"Execution time: {duration:F2}ms");
            System.Diagnostics.Debug.WriteLine($"=== Contact Detection End ===\n");

            return res;
        }

        public static ContactData DetectContact(Part partA, Part partB)
            => DetectContactsForPair(partA, partB).FirstOrDefault();


    }
}