using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Contact.Detection.BroadPhase;
using AssemblyChain.Core.Contact.Detection.NarrowPhase;

namespace AssemblyChain.Core.Contact
{
    /// <summary>
    /// 主接触检测入口 - 协调BroadPhase和NarrowPhase的完整检测流程
    /// </summary>
    public static class ContactDetection
    {
        /// <summary>
        /// 检测装配体中所有部件之间的接触
        /// </summary>
        public static ContactModel DetectContacts(AssemblyModel assembly, DetectionOptions options)
        {
            System.Diagnostics.Debug.WriteLine($"\n========== ASSEMBLY CONTACT DETECTION START ==========");
            System.Diagnostics.Debug.WriteLine($"Assembly: {assembly.Name} ({assembly.PartCount} parts)");
            System.Diagnostics.Debug.WriteLine($"Options: Tolerance={options.Tolerance:F6}, MinPatchArea={options.MinPatchArea:F6}");

            var startTime = DateTime.Now;

            // 1. 创建BroadPhase算法
            var broadPhase = BroadPhaseFactory.Create(options.BroadPhase ?? "sap");

            // 2. 获取候选对（Broad Phase）
            var candidatePairs = broadPhase.GetCandidatePairs(assembly.Parts, options);
            System.Diagnostics.Debug.WriteLine($"Broad phase: {candidatePairs.Count} candidate pairs");

            // 3. 精确检测候选对（Narrow Phase）
            var allContacts = new List<ContactData>();
            var processedPairs = 0;

            foreach (var (i, j) in candidatePairs)
            {
                var partA = assembly.Parts[i];
                var partB = assembly.Parts[j];

                var pairContacts = NarrowPhaseDetection.DetectContactsForPair(
                    partA, partB, options.Tolerance, options.MinPatchArea, 0.0, options);

                if (pairContacts.Any())
                {
                    allContacts.AddRange(pairContacts);
                }

                processedPairs++;
                if (processedPairs % 10 == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Processed {processedPairs}/{candidatePairs.Count} pairs...");
                }
            }

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            System.Diagnostics.Debug.WriteLine($"Detection completed in {duration.TotalMilliseconds:F1}ms");
            System.Diagnostics.Debug.WriteLine($"Total contacts found: {allContacts.Count}");
            System.Diagnostics.Debug.WriteLine($"========== ASSEMBLY CONTACT DETECTION END ==========\n");

            // 生成哈希用于缓存
            var hash = $"assembly_{assembly.Hash}_{options.BroadPhase}_{options.Tolerance}_{options.MinPatchArea}";
            return new ContactModel(allContacts, hash);
        }

        /// <summary>
        /// 检测零件列表中的所有接触
        /// </summary>
        public static List<ContactData> DetectContacts(IReadOnlyList<Part> parts, DetectionOptions options)
        {
            System.Diagnostics.Debug.WriteLine($"\n========== PART LIST CONTACT DETECTION ==========");
            System.Diagnostics.Debug.WriteLine($"Parts: {parts.Count}");

            // 创建BroadPhase算法
            var broadPhase = BroadPhaseFactory.Create(options.BroadPhase ?? "sap");

            // 获取所有可能的候选对（简化版）
            var candidatePairs = new List<(int i, int j)>();
            for (int i = 0; i < parts.Count; i++)
            {
                for (int j = i + 1; j < parts.Count; j++)
                {
                    candidatePairs.Add((i, j));
                }
            }

            System.Diagnostics.Debug.WriteLine($"Candidate pairs: {candidatePairs.Count}");

            // 精确检测
            var allContacts = new List<ContactData>();
            foreach (var (i, j) in candidatePairs)
            {
                var partA = parts[i];
                var partB = parts[j];

                var pairContacts = NarrowPhaseDetection.DetectContactsForPair(
                    partA, partB, options.Tolerance, options.MinPatchArea, 0.0, options);

                if (pairContacts.Any())
                {
                    allContacts.AddRange(pairContacts);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total contacts: {allContacts.Count}");
            System.Diagnostics.Debug.WriteLine($"========== PART LIST DETECTION END ==========\n");

            return allContacts;
        }
    }
}
