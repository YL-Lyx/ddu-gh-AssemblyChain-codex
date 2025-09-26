using System;
using BenchmarkDotNet.Attributes;
using AssemblyChain.Geometry.Contact.Detection.NarrowPhase;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Rhino.Geometry;

namespace AssemblyChain.Benchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(AssemblyChainBenchmarkConfig))]
    public class ContactNarrowPhaseBench
    {
        private Part _smallA = null!;
        private Part _smallB = null!;
        private Part _mediumA = null!;
        private Part _mediumB = null!;

        [Params(true, false)]
        public bool UseSpatialIndexing { get; set; }

        [Params(MeshComplexity.Small, MeshComplexity.Medium)]
        public MeshComplexity Complexity { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            (_smallA, _smallB) = CreatePair(size: 1.0, subdivisions: 1);
            (_mediumA, _mediumB) = CreatePair(size: 3.0, subdivisions: 6);
        }

        [Benchmark(Description = "Mesh narrow-phase")]
        public int DetectContacts()
        {
            var options = MeshContactDetector.EnhancedDetectionOptions.CreatePreset(
                MeshContactDetector.EnhancedDetectionOptions.QualityPreset.Balanced);
            options.EnableSpatialIndexing = UseSpatialIndexing;

            var (a, b) = Complexity == MeshComplexity.Small ? (_smallA, _smallB) : (_mediumA, _mediumB);
            var contacts = MeshContactDetector.DetectMeshContactsEnhanced(a, b, options);
            return contacts.Count;
        }

        private static (Part, Part) CreatePair(double size, int subdivisions)
        {
            var meshA = CreateBoxMesh(new Point3d(0, 0, 0), size, subdivisions);
            var meshB = CreateBoxMesh(new Point3d(size * 0.25, size * 0.25, size * 0.25), size, subdivisions);

            var partA = new Part(0, "BenchA", new PartGeometry(0, meshA));
            var partB = new Part(1, "BenchB", new PartGeometry(1, meshB));
            return (partA, partB);
        }

        private static Mesh CreateBoxMesh(Point3d origin, double size, int subdivisions)
        {
            var bbox = new BoundingBox(origin, origin + new Vector3d(size, size, size));
            subdivisions = Math.Max(1, subdivisions);
            return Mesh.CreateFromBox(bbox, subdivisions, subdivisions, subdivisions);
        }

        public enum MeshComplexity
        {
            Small,
            Medium
        }
    }
}
