using System.IO;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;

namespace AssemblyChain.Benchmarks
{
    public sealed class AssemblyChainBenchmarkConfig : ManualConfig
    {
        public AssemblyChainBenchmarkConfig()
        {
            AddJob(Job.ShortRun.WithWarmupCount(3).WithIterationCount(10));
            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(TargetMethodColumn.Method, StatisticColumn.P95, StatisticColumn.OperationsPerSecond, StatisticColumn.Min, StatisticColumn.Max);
            AddExporter(MarkdownExporter.GitHub);
            AddExporter(JsonExporter.Full);
            ArtifactsPath = Path.Combine("artifacts", "benchmarks");
        }
    }
}
