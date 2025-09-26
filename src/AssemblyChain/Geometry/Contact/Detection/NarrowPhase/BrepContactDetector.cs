using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Planning.Model;
using AssemblyChain.IO.Contracts;
using AssemblyChain.Geometry.Toolkit.Brep;

namespace AssemblyChain.Geometry.Contact.Detection.NarrowPhase
{
    /// <summary>
    /// 专门处理Brep-Brep接触检测的模块
    /// </summary>
    public static class BrepContactDetector
    {
        /// <summary>
        /// 检测两个Brep部件之间的接触
        /// </summary>
        public static List<ContactData> DetectBrepContacts(
            Part partA, Part partB, DetectionOptions options)
        {
            var contacts = new List<ContactData>();

            // 验证输入
            if (!partA.HasValidGeometry || !partB.HasValidGeometry)
                return contacts;

            // 获取Brep几何
            var brepA = partA.OriginalGeometry as Brep;
            var brepB = partB.OriginalGeometry as Brep;

            if (brepA == null || brepB == null)
                return contacts;

            System.Diagnostics.Debug.WriteLine($"[BrepContactDetector] Processing Brep-Brep: {partA.Name} vs {partB.Name}");
            System.Diagnostics.Debug.WriteLine($"  BrepA: {brepA.Faces.Count} faces, BrepB: {brepB.Faces.Count} faces");

            try
            {
                // 使用PlanarOps进行共面接触检测
                var result = PlanarOps.DetectCoplanarContacts(brepA, brepB, partA, partB, options);

                contacts.AddRange(result.Contacts);

                System.Diagnostics.Debug.WriteLine($"[BrepContactDetector] Found {result.Contacts.Count} contacts");
                System.Diagnostics.Debug.WriteLine($"  Statistics: {result.TotalFacePairs} face pairs, {result.CoplanarPairs} coplanar, {result.OverlappingPairs} overlapping, {result.ValidOverlaps} valid");
                System.Diagnostics.Debug.WriteLine($"  Execution time: {result.ExecutionTime.TotalMilliseconds:F2}ms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BrepContactDetector] Error in Brep-Brep detection: {ex.Message}");
            }

            return contacts;
        }
    }
}
