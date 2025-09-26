using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Planning.Facade;
using AssemblyChain.Planning.Model;
using AssemblyChain.Robotics;
using AssemblyChain.Planning.Solver;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace Case03Sample
{
    internal static class Program
    {
        private static readonly string SampleRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        private static void Main()
        {
            Console.WriteLine("[Case03] Building demo assembly");
            var assembly = CreateAssembly();

            var facade = new AssemblyChainFacade();
            var result = facade.BuildAndSolve(assembly, new SolverOptions(SolverType.CSP));
            Console.WriteLine($"[Case03] Solver outcome: {(result.IsFeasible ? "Feasible" : "Infeasible")}, steps = {result.StepCount}");

            var outputDir = Path.Combine(SampleRoot, "output");
            Directory.CreateDirectory(outputDir);
            var processPath = Path.Combine(outputDir, "process.json");

            var exportOptions = new ProcessExportOptions
            {
                OutputPath = processPath,
                Author = "Case03 Sample",
                Description = "Three-block disassembly via AssemblyChain facade",
                IncludeMetadata = true
            };

            var process = facade.ExportProcess(result, exportOptions);
            Console.WriteLine($"[Case03] Process exported to {processPath}");

            var schemaPath = Path.GetFullPath(Path.Combine(SampleRoot, "..", "src", "AssemblyChain.Core", "Robotics", "process.schema.json"));
            ValidateProcess(processPath, schemaPath);
            Console.WriteLine("[Case03] Schema validation succeeded");
        }

        private static AssemblyModel CreateAssembly()
        {
            var assembly = new Assembly(id: 100, name: "Case03 Assembly");

            for (int i = 0; i < 3; i++)
            {
                var origin = new Point3d(i * 1.2, 0, 0);
                var mesh = CreateCubeMesh(origin, size: 1.0);
                var geometry = new PartGeometry(i, mesh);
                assembly.AddPart(new Part(i, $"Block_{i}", geometry));
            }
            return AssemblyModelFactory.Create(assembly);
        }

        private static Mesh CreateCubeMesh(Point3d minCorner, double size)
        {
            var maxCorner = minCorner + new Vector3d(size, size, size);
            var box = new BoundingBox(minCorner, maxCorner);
            return Mesh.CreateFromBox(box, 1, 1, 1);
        }

        private static void ValidateProcess(string processPath, string schemaPath)
        {
            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException("Process file not found.", processPath);
            }

            if (!File.Exists(schemaPath))
            {
                throw new FileNotFoundException("Schema file not found.", schemaPath);
            }

            var document = JObject.Parse(File.ReadAllText(processPath));
            var schema = JObject.Parse(File.ReadAllText(schemaPath));

            var requiredProperties = schema["required"]?.Values<string>().ToList() ?? new List<string>();
            foreach (var property in requiredProperties)
            {
                if (document[property] == null)
                {
                    throw new InvalidOperationException($"Process JSON missing required property '{property}'.");
                }
            }

            if (document["steps"] is not JArray steps || steps.Count == 0)
            {
                throw new InvalidOperationException("Process JSON must contain a non-empty 'steps' array.");
            }

            var stepRequirements = schema["properties"]?["steps"]?["items"]?["required"]?.Values<string>().ToList() ?? new List<string>();
            foreach (var (stepToken, index) in steps.Select((token, idx) => (token, idx)))
            {
                if (stepToken is not JObject step)
                {
                    throw new InvalidOperationException($"Step #{index} is not a JSON object.");
                }

                foreach (var field in stepRequirements)
                {
                    if (step[field] == null)
                    {
                        throw new InvalidOperationException($"Step #{index} missing required property '{field}'.");
                    }
                }

                if (step["direction"] is not JArray direction || direction.Count != 3 || direction.Any(component => component.Type != JTokenType.Float && component.Type != JTokenType.Integer))
                {
                    throw new InvalidOperationException($"Step #{index} has an invalid 'direction' vector.");
                }
            }
        }
    }
}
