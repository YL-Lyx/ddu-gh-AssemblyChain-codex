using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Learning;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Rhino.Geometry;

namespace AssemblyChain.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }

    [MemoryDiagnoser]
    public class OnnxInferenceBenchmark
    {
        private OnnxInferenceService _service = null!;
        private OnnxInferenceRequest _request = null!;

        [GlobalSetup]
        public void Setup()
        {
            _service = new OnnxInferenceService();

            var mesh = Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(1, 1, 1)), 1, 1, 1);
            var partGeometry = new PartGeometry(0, mesh, "BenchPart", mesh, "Mesh");
            var part = new Part(0, "BenchPart", partGeometry);
            var assembly = new Assembly(id: 1, name: "Benchmark");
            assembly.AddPart(part);

            AssemblyModel model = AssemblyModelFactory.Create(assembly);
            _request = new OnnxInferenceRequest(model);
        }

        [Benchmark]
        public OnnxInferenceResult RunInference()
        {
            return _service.Run(_request);
        }
    }
}
