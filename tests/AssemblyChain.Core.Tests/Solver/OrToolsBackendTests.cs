using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Facade;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Solver;
using AssemblyChain.Core.Solver.Backends;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Solver
{
    public class OrToolsBackendTests
    {
        [Fact]
        public void CspSolver_ReturnsFeasiblePlan()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var constraintModel = CreateConstraintModel(assembly);

            var solver = new CspSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.CSP));

            Assert.True(result.IsFeasible);
            Assert.Equal(assembly.PartCount, result.StepCount);
            Assert.Equal("Feasible", result.Metadata["outcome"].ToString());
        }

        [Fact]
        public void CspSolver_DetectsInfeasiblePartConstraint()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var partConstraints = new Dictionary<int, IReadOnlyList<string>>
            {
                [1] = new List<string> { "forbid" }
            };
            var constraintModel = CreateConstraintModel(assembly, partConstraints: partConstraints);

            var solver = new CspSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.CSP));

            Assert.False(result.IsFeasible);
            Assert.Contains("forbidden", result.Log, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("Infeasible", result.Metadata["outcome"].ToString());
        }

        [Fact]
        public void CspSolver_DetectsCycleConflict()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var partConstraints = new Dictionary<int, IReadOnlyList<string>>
            {
                [0] = new List<string> { "requires:1" },
                [1] = new List<string> { "requires:0" }
            };
            var constraintModel = CreateConstraintModel(assembly, partConstraints: partConstraints);

            var solver = new CspSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.CSP));

            Assert.False(result.IsFeasible);
            Assert.Equal("Conflict", result.Metadata["outcome"].ToString());
            Assert.Contains("cycle", result.Log, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void MilpSolver_MinimizesPenalties()
        {
            var assembly = CreateAssembly(out var parts);
            parts[0].Metadata["penalty"] = 10.0;
            parts[1].Metadata["penalty"] = 1.0;
            parts[2].Metadata["penalty"] = 2.0;

            var contactModel = CreateContactModel();
            var edges = new[] { (from: parts[1].IndexId, to: parts[0].IndexId) };
            var constraintModel = CreateConstraintModel(assembly, edges: edges);

            var solver = new MilpSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.MILP));

            Assert.True(result.IsFeasible);
            var plannedOrder = result.Steps.Select(s => s.Part.IndexId).ToArray();
            Assert.True(Array.IndexOf(plannedOrder, parts[1].IndexId) < Array.IndexOf(plannedOrder, parts[0].IndexId));
        }

        [Fact]
        public void MilpSolver_ReportsInfeasibleOnForbidden()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var partConstraints = new Dictionary<int, IReadOnlyList<string>>
            {
                [2] = new List<string> { "forbid" }
            };
            var constraintModel = CreateConstraintModel(assembly, partConstraints: partConstraints);

            var solver = new MilpSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.MILP));

            Assert.False(result.IsFeasible);
            Assert.Equal("Infeasible", result.Metadata["outcome"].ToString());
        }

        [Fact]
        public void MilpSolver_ReportsConflictOnCycle()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var edges = new[] { (from: 0, to: 1), (from: 1, to: 0) };
            var constraintModel = CreateConstraintModel(assembly, edges: edges);

            var solver = new MilpSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.MILP));

            Assert.False(result.IsFeasible);
            Assert.Equal("Conflict", result.Metadata["outcome"].ToString());
        }

        [Fact]
        public void SatSolver_FindsSatisfyingAssignment()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var clauses = new Dictionary<string, IReadOnlyList<string>>
            {
                ["sat"] = new List<string> { "P0 P1", "P2" }
            };
            var constraintModel = CreateConstraintModel(assembly, groupConstraints: clauses);

            var solver = new SatSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.SAT));

            Assert.True(result.IsFeasible);
            Assert.Equal(assembly.PartCount, result.StepCount);
        }

        [Fact]
        public void SatSolver_ReportsInfeasibleWhenPartForbidden()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var clauses = new Dictionary<string, IReadOnlyList<string>>
            {
                ["sat"] = new List<string> { "P0" }
            };
            var partConstraints = new Dictionary<int, IReadOnlyList<string>>
            {
                [0] = new List<string> { "forbid" }
            };
            var constraintModel = CreateConstraintModel(assembly, partConstraints: partConstraints, groupConstraints: clauses);

            var solver = new SatSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.SAT));

            Assert.False(result.IsFeasible);
            Assert.Equal("Infeasible", result.Metadata["outcome"].ToString());
        }

        [Fact]
        public void SatSolver_ReportsConflictOnUnsatClauses()
        {
            var assembly = CreateAssembly(out _);
            var contactModel = CreateContactModel();
            var clauses = new Dictionary<string, IReadOnlyList<string>>
            {
                ["sat"] = new List<string> { "P0", "-P0" }
            };
            var constraintModel = CreateConstraintModel(assembly, groupConstraints: clauses);

            var solver = new SatSolver(new OrToolsBackend());
            var result = solver.Solve(assembly, contactModel, constraintModel, new SolverOptions(SolverType.SAT));

            Assert.False(result.IsFeasible);
            Assert.Equal("Conflict", result.Metadata["outcome"].ToString());
        }

        [Theory]
        [InlineData(SolverType.CSP)]
        [InlineData(SolverType.MILP)]
        [InlineData(SolverType.SAT)]
        public void FacadeBuildAndSolve_UsesRequestedSolver(SolverType solverType)
        {
            var assembly = CreateAssembly(out _);
            var facade = new AssemblyChainFacade();
            var result = facade.BuildAndSolve(assembly, new SolverOptions(solverType));

            Assert.Equal(NormalizeType(solverType).ToString(), result.SolverType);
        }

        private static SolverType NormalizeType(SolverType solverType)
        {
            return solverType == SolverType.Auto ? SolverType.CSP : solverType;
        }

        private static AssemblyModel CreateAssembly(out List<Part> createdParts)
        {
            var assembly = new Assembly(1, "TestAssembly");
            createdParts = new List<Part>();
            for (int i = 0; i < 3; i++)
            {
                var mesh = CreateUnitCubeMesh(i);
                var geometry = new PartGeometry(i, mesh);
                var part = new Part(i, $"Part_{i}", geometry);
                createdParts.Add(part);
                assembly.AddPart(part);
            }

            createdParts = assembly.Parts.ToList();
            return AssemblyModelFactory.Create(assembly);
        }

        private static Mesh CreateUnitCubeMesh(double zOffset)
        {
            var mesh = new Mesh();
            mesh.Vertices.Add(0, 0, zOffset);
            mesh.Vertices.Add(1, 0, zOffset);
            mesh.Vertices.Add(1, 1, zOffset);
            mesh.Vertices.Add(0, 1, zOffset);
            mesh.Vertices.Add(0, 0, zOffset + 1);
            mesh.Vertices.Add(1, 0, zOffset + 1);
            mesh.Vertices.Add(1, 1, zOffset + 1);
            mesh.Vertices.Add(0, 1, zOffset + 1);
            mesh.Faces.AddFace(0, 1, 2, 3);
            mesh.Faces.AddFace(4, 5, 6, 7);
            mesh.Faces.AddFace(0, 1, 5, 4);
            mesh.Faces.AddFace(1, 2, 6, 5);
            mesh.Faces.AddFace(2, 3, 7, 6);
            mesh.Faces.AddFace(3, 0, 4, 7);
            mesh.Normals.ComputeNormals();
            return mesh;
        }

        private static ContactModel CreateContactModel()
        {
            var ctor = typeof(ContactModel).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[]
            {
                typeof(IReadOnlyList<ContactData>),
                typeof(string)
            }, null);

            return (ContactModel)ctor!.Invoke(new object[] { Array.Empty<ContactData>(), "contacts_test" });
        }

        private static ConstraintModel CreateConstraintModel(
            AssemblyModel assembly,
            IEnumerable<(int from, int to)>? edges = null,
            IDictionary<int, IReadOnlyList<string>>? partConstraints = null,
            IDictionary<string, IReadOnlyList<string>>? groupConstraints = null,
            IDictionary<int, IReadOnlyList<Vector3d>>? motionRays = null)
        {
            var graph = BuildGraphModel(assembly, edges ?? Array.Empty<(int, int)>());
            var motion = BuildMotionModel(assembly, motionRays);
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
                partConstraints ?? new Dictionary<int, IReadOnlyList<string>>(),
                groupConstraints ?? new Dictionary<string, IReadOnlyList<string>>(),
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

        private static MotionModel BuildMotionModel(
            AssemblyModel assembly,
            IDictionary<int, IReadOnlyList<Vector3d>>? motionRays)
        {
            var rays = new Dictionary<int, IReadOnlyList<Vector3d>>();
            foreach (var part in assembly.Parts)
            {
                if (motionRays != null && motionRays.TryGetValue(part.IndexId, out var customRays))
                {
                    rays[part.IndexId] = customRays;
                }
                else
                {
                    rays[part.IndexId] = new List<Vector3d> { Vector3d.ZAxis };
                }
            }

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
    }
}
