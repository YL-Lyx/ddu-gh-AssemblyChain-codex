using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Solver;
using AssemblyChain.Core.Solver.Backends;
using Rhino.Geometry;

namespace AssemblyChain.Benchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(AssemblyChainBenchmarkConfig))]
    public class SolverBench
    {
        private AssemblyModel _assembly = null!;
        private ContactModel _contacts = null!;
        private ConstraintModel _constraints = null!;
        private ISolver _cspSolver = null!;
        private ISolver _satSolver = null!;
        private ISolver _milpSolver = null!;
        private SolverOptions _cspOptions;
        private SolverOptions _satOptions;
        private SolverOptions _milpOptions;

        [GlobalSetup]
        public void Setup()
        {
            _assembly = CreateAssembly(6);
            _contacts = CreateContactModel();
            _constraints = CreateConstraintModel(_assembly, BuildEdges(), BuildPartConstraints(), BuildGroupConstraints());

            var backend = new OrToolsBackend();
            _cspSolver = new CspSolver(backend);
            _satSolver = new SatSolver(backend);
            _milpSolver = new MilpSolver(backend);

            _cspOptions = new SolverOptions(SolverType.CSP);
            _satOptions = new SolverOptions(SolverType.SAT);
            _milpOptions = new SolverOptions(SolverType.MILP);
        }

        [Benchmark(Description = "CSP order")] 
        public int SolveCsp()
        {
            return _cspSolver.Solve(_assembly, _contacts, _constraints, _cspOptions).StepCount;
        }

        [Benchmark(Description = "SAT feasibility")] 
        public int SolveSat()
        {
            return _satSolver.Solve(_assembly, _contacts, _constraints, _satOptions).StepCount;
        }

        [Benchmark(Description = "MILP objective")]
        public int SolveMilp()
        {
            return _milpSolver.Solve(_assembly, _contacts, _constraints, _milpOptions).StepCount;
        }

        private static AssemblyModel CreateAssembly(int partCount)
        {
            var assembly = new Assembly(42, "Benchmark Assembly");
            for (int i = 0; i < partCount; i++)
            {
                var mesh = Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(1, 1, 1)), 1, 1, 1);
                var geometry = new PartGeometry(i, mesh);
                var part = new Part(i, $"Part_{i}", geometry);
                if (i % 2 == 0)
                {
                    part.Metadata["penalty"] = 2.0 + i;
                }

                assembly.AddPart(part);
            }

            return AssemblyModelFactory.Create(assembly);
        }

        private static ContactModel CreateContactModel()
        {
            var ctor = typeof(ContactModel).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[]
            {
                typeof(IReadOnlyList<ContactData>),
                typeof(string)
            }, null);

            return (ContactModel)ctor!.Invoke(new object[] { Array.Empty<ContactData>(), "bench_contacts" });
        }

        private static ConstraintModel CreateConstraintModel(
            AssemblyModel assembly,
            IEnumerable<(int from, int to)> edges,
            IReadOnlyDictionary<int, IReadOnlyList<string>> partConstraints,
            IReadOnlyDictionary<string, IReadOnlyList<string>> groupConstraints)
        {
            var graph = BuildGraphModel(assembly, edges);
            var motion = BuildMotionModel(assembly);

            var ctor = typeof(ConstraintModel).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[]
            {
                typeof(GraphModel),
                typeof(MotionModel),
                typeof(IReadOnlyDictionary<int, IReadOnlyList<string>>),
                typeof(IReadOnlyDictionary<string, IReadOnlyList<string>>),
                typeof(string)
            }, null);

            return (ConstraintModel)ctor!.Invoke(new object[]
            {
                graph,
                motion,
                partConstraints,
                groupConstraints,
                $"constraints_{Guid.NewGuid():N}"
            });
        }

        private static GraphModel BuildGraphModel(AssemblyModel assembly, IEnumerable<(int from, int to)> edges)
        {
            var blockingGraph = new BlockingGraph();
            var allEdges = new List<BlockingEdge>();
            var indegrees = new Dictionary<int, int>();

            foreach (var part in assembly.Parts)
            {
                blockingGraph.Nodes.Add(part.IndexId);
                indegrees[part.IndexId] = 0;
            }

            foreach (var (from, to) in edges)
            {
                var edge = new BlockingEdge { FromId = from, ToId = to };
                blockingGraph.Edges.Add(edge);
                allEdges.Add(edge);
                if (!indegrees.ContainsKey(to))
                {
                    indegrees[to] = 0;
                }

                indegrees[to]++;
            }

            var ctor = typeof(GraphModel).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[]
            {
                typeof(BlockingGraph),
                typeof(NonDirectionalBlockingGraph),
                typeof(IReadOnlyDictionary<int, int>),
                typeof(IReadOnlyList<StronglyConnectedComponent>),
                typeof(IReadOnlyList<BlockingEdge>),
                typeof(string)
            }, null);

            return (GraphModel)ctor!.Invoke(new object[]
            {
                blockingGraph,
                new NonDirectionalBlockingGraph(),
                indegrees,
                new List<StronglyConnectedComponent>(),
                allEdges,
                $"graph_{Guid.NewGuid():N}"
            });
        }

        private static MotionModel BuildMotionModel(AssemblyModel assembly)
        {
            var rays = assembly.Parts.ToDictionary(part => part.IndexId, _ => (IReadOnlyList<Vector3d>)new[] { Vector3d.ZAxis });

            var ctor = typeof(MotionModel).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[]
            {
                typeof(IReadOnlyDictionary<int, IReadOnlyList<Vector3d>>),
                typeof(IReadOnlyDictionary<string, IReadOnlyList<Vector3d>>),
                typeof(string)
            }, null);

            return (MotionModel)ctor!.Invoke(new object[]
            {
                rays,
                new Dictionary<string, IReadOnlyList<Vector3d>>(),
                $"motion_{Guid.NewGuid():N}"
            });
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<string>> BuildPartConstraints()
        {
            return new Dictionary<int, IReadOnlyList<string>>
            {
                [3] = new List<string> { "requires:1" }
            };
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildGroupConstraints()
        {
            return new Dictionary<string, IReadOnlyList<string>>
            {
                ["sat"] = new List<string> { "P0 P1", "P2" }
            };
        }

        private static IEnumerable<(int from, int to)> BuildEdges()
        {
            yield return (0, 2);
            yield return (1, 3);
            yield return (2, 4);
            yield return (3, 5);
        }
    }
}
