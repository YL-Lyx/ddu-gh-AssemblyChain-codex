using System;
using System.Collections.Generic;
using System.IO;
using AssemblyChain.Analysis.Learning;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Geometry.Contact;
using AssemblyChain.IO.Contracts;
using AssemblyChain.IO.Data;
using AssemblyChain.Planning.Facade;
using AssemblyChain.Planning.Model;
using AssemblyChain.Planning.Solver;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Workflow
{
    public class WorkflowIntegrationTests
    {
        [Fact]
        public void Facade_Composes_Modules_Into_Executable_Workflow()
        {
            // Arrange
            var assembly = BuildAssembly();
            var assemblyModel = AssemblyModelFactory.Create(assembly);

            var zone = new ContactZone(new PlaneSurface(Plane.WorldXY, new Interval(0, 1), new Interval(0, 1)), area: 1.0);
            var plane = new ContactPlane(Plane.WorldXY, Vector3d.ZAxis, Point3d.Origin);
            var contacts = ContactModelFactory.FromContacts(new[]
            {
                new ContactData("P0000", "P0001", ContactType.Face, zone, plane, frictionCoefficient: 0.4)
            });

            var constraintModel = ConstraintModelFactory.CreateEmpty(assemblyModel);
            var solverOptions = new SolverOptions(SolverType.CSP);

            var fakeSolver = new FakeSolver();
            var facade = new AssemblyChainFacade(
                solverFactory: _ => fakeSolver,
                inferenceService: new OnnxInferenceService());

            var request = new AssemblyPlanRequest(
                assemblyModel,
                contacts,
                constraintModel,
                detection: null,
                solver: solverOptions);

            var tempRoot = Path.Combine(Path.GetTempPath(), $"ac_workflow_{Guid.NewGuid():N}");
            var processPath = Path.Combine(tempRoot, "process", "workflow.json");
            var datasetDirectory = Path.Combine(tempRoot, "dataset");

            try
            {
                // Act
                var planResult = facade.RunPlan(request);
                var initialSolver = planResult.SolverResult;

                var directResult = facade.BuildAndSolve(assemblyModel, solverOptions, contacts, constraintModel);

                var processOptions = new ProcessExportOptions
                {
                    OutputPath = processPath,
                    IncludeMetadata = true,
                    Author = "workflow-test"
                };
                var processSchema = facade.ExportProcess(initialSolver, processOptions);

                var datasetOptions = new DatasetExportOptions
                {
                    OutputDirectory = datasetDirectory,
                    IncludeGeometry = true,
                    Tags = new[] { "integration" }
                };
                var datasetResult = facade.ExportDataset(assemblyModel, contacts, initialSolver, datasetOptions);

                var inferenceRequest = new OnnxInferenceRequest(assemblyModel, new Dictionary<string, double>
                {
                    ["graspability"] = 0.85
                });
                var inferenceResult = facade.RunInference(inferenceRequest);

                // Assert
                Assert.Equal(2, fakeSolver.InvocationCount);
                Assert.Same(contacts, planResult.Contacts);
                Assert.Equal(assemblyModel.PartCount, initialSolver.StepCount);
                Assert.Equal(initialSolver.StepCount, directResult.StepCount);
                Assert.Equal(initialSolver.StepCount, processSchema.Steps.Count);
                Assert.NotNull(processSchema.Metadata);
                Assert.True(File.Exists(processPath));
                Assert.Equal(1, datasetResult.RecordCount);
                Assert.True(Directory.Exists(datasetResult.Directory));
                var datasetFile = Path.Combine(datasetResult.Directory, $"{assemblyModel.Name.Replace(' ', '_')}_dataset.json");
                Assert.True(File.Exists(datasetFile));
                Assert.True(inferenceResult.Scores.ContainsKey("stability"));
                Assert.True(inferenceResult.Scores.ContainsKey("graspability"));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
            }
        }

        private static Assembly BuildAssembly()
        {
            var assembly = new Assembly(id: 1, name: "WorkflowAssembly");
            assembly.AddPart(CreatePart(0, "PartA", offsetX: 0.0));
            assembly.AddPart(CreatePart(1, "PartB", offsetX: 1.5));
            return assembly;
        }

        private static Part CreatePart(int id, string name, double offsetX)
        {
            var mesh = new Mesh();
            mesh.Vertices.Add(offsetX, 0, 0);
            mesh.Vertices.Add(offsetX + 1, 0, 0);
            mesh.Vertices.Add(offsetX, 1, 0);
            mesh.Vertices.Add(offsetX, 0, 1);
            mesh.Faces.AddFace(0, 1, 2);
            mesh.Faces.AddFace(0, 2, 3);
            mesh.Normals.ComputeNormals();
            mesh.Compact();

            var geometry = new PartGeometry(id, mesh);
            return new Part(id, name, geometry);
        }

        private sealed class FakeSolver : ISolver
        {
            public int InvocationCount { get; private set; }

            public DgSolverModel Solve(AssemblyModel assembly, ContactModel contacts, ConstraintModel constraints, SolverOptions options = default)
            {
                InvocationCount++;

                var steps = new List<Step>();
                var vectors = new List<Vector3d>();

                for (int i = 0; i < assembly.PartCount; i++)
                {
                    var part = assembly.Parts[i];
                    var direction = new Vector3d(0, 0, 1 + i);
                    steps.Add(new Step(i, part, direction) { Insert = true, Batch = 0 });
                    vectors.Add(direction);
                }

                var metadata = new Dictionary<string, object>
                {
                    ["solverType"] = options.SolverType.ToString(),
                    ["invocation"] = InvocationCount
                };

                return SolverModelFactory.Create(
                    steps,
                    vectors,
                    groups: new[] { (IReadOnlyList<int>)new List<int> { 0, 1 } },
                    isFeasible: true,
                    isOptimal: true,
                    log: "fake-solver",
                    solveTimeSeconds: 0.01 * InvocationCount,
                    solverType: options.SolverType.ToString(),
                    metadata: metadata);
            }
        }
    }
}
