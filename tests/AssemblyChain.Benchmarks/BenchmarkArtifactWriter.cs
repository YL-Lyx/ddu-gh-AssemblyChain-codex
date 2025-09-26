using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace AssemblyChain.Benchmarks
{
    internal static class BenchmarkArtifactWriter
    {
        public static void WriteSummary(IReadOnlyList<Summary> summaries, string artifactsPath)
        {
            if (summaries == null || summaries.Count == 0)
            {
                return;
            }

            Directory.CreateDirectory(artifactsPath);
            var readmePath = Path.Combine(artifactsPath, "README.md");
            var lines = new List<string>
            {
                "# Benchmark Summary",
                $"Generated: {DateTime.UtcNow:O}",
                string.Empty
            };

            foreach (var summary in summaries)
            {
                lines.Add($"## {summary.Title}");
                lines.Add(string.Empty);
                lines.Add("| Benchmark | Parameters | Ops/s | P95 (ms) | Allocated (KB) |");
                lines.Add("|-----------|------------|-------|----------|----------------|");

                foreach (var report in summary.Reports)
                {
                    var stats = report.ResultStatistics;
                    if (stats == null)
                    {
                        continue;
                    }

                    var name = report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo;
                    var parameters = string.Join(", ", report.BenchmarkCase.Parameters.Items
                        .Select(p => $"{p.Name}={p.Value}"));
                    var ops = stats.OperationsPerSecond;
                    var p95 = ToMilliseconds(stats.Percentile95);
                    var allocatedBytes = report.GcStats.BytesAllocatedPerOperation;
                    var allocatedKb = double.IsNaN(allocatedBytes) ? 0 : allocatedBytes / 1024.0;

                    lines.Add($"| {name} | {parameters} | {ops:F2} | {p95:F3} | {allocatedKb:F2} |");
                }

                lines.Add(string.Empty);

                if (summary.Reports.Any(r => r.BenchmarkCase.Descriptor.Type == typeof(ContactNarrowPhaseBench)))
                {
                    lines.Add("### Prefilter Impact");
                    foreach (var note in ComputeContactImprovements(summary))
                    {
                        lines.Add($"- {note}");
                    }
                    lines.Add(string.Empty);
                }
            }

            File.WriteAllLines(readmePath, lines);
        }

        private static IEnumerable<string> ComputeContactImprovements(Summary summary)
        {
            var reports = summary.Reports
                .Where(r => r.BenchmarkCase.Descriptor.Type == typeof(ContactNarrowPhaseBench))
                .ToList();

            if (reports.Count == 0)
            {
                yield break;
            }

            var groups = reports.GroupBy(r => r.BenchmarkCase.Parameters["Complexity"]);
            foreach (var group in groups)
            {
                var withIndex = group.FirstOrDefault(r => Convert.ToBoolean(r.BenchmarkCase.Parameters["UseSpatialIndexing"], CultureInfo.InvariantCulture));
                var withoutIndex = group.FirstOrDefault(r => !Convert.ToBoolean(r.BenchmarkCase.Parameters["UseSpatialIndexing"], CultureInfo.InvariantCulture));
                if (withIndex?.ResultStatistics == null || withoutIndex?.ResultStatistics == null)
                {
                    continue;
                }

                var withP95 = ToMilliseconds(withIndex.ResultStatistics.Percentile95);
                var withoutP95 = ToMilliseconds(withoutIndex.ResultStatistics.Percentile95);
                var delta = withoutP95 - withP95;
                var percent = Math.Abs(withoutP95) > double.Epsilon ? (delta / withoutP95) * 100.0 : 0.0;
                var descriptor = delta >= 0 ? "faster" : "slower";

                yield return $"{group.Key}: spatial index {descriptor} by {Math.Abs(percent):F1}% (P95 {withP95:F3} ms vs {withoutP95:F3} ms)";
            }
        }

        private static double ToMilliseconds(double nanoseconds)
        {
            return nanoseconds / 1_000_000.0;
        }
    }
}
