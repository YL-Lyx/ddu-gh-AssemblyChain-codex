using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Toolkit.Processing
{
    /// <summary>
    /// Processes mesh and brep input into <see cref="Part"/> or <see cref="PartGeometry"/> instances.
    /// Extracted from the Grasshopper component to enable unit testing and reduce UI complexity.
    /// </summary>
    public static class PartCreationProcessor
    {
        public sealed class PartCreationOptions
        {
            public string BaseName { get; set; } = string.Empty;
            public bool IncludePhysics { get; set; }
            public PhysicsProperties? Physics { get; set; }
        }

        public sealed class PartCreationResult
        {
            public List<object> CreatedItems { get; } = new();
            public List<ProcessingMessage> Messages { get; } = new();
            public int SuccessCount { get; set; }
            public int FailureCount { get; set; }
        }

        public static PartCreationResult FromMeshes(IEnumerable<Mesh?> meshes, PartCreationOptions options)
        {
            if (meshes == null) throw new ArgumentNullException(nameof(meshes));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var result = new PartCreationResult();
            var meshList = meshes.ToList();

            if (meshList.Count == 0)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Error, "No mesh geometry provided."));
                return result;
            }

            for (int i = 0; i < meshList.Count; i++)
            {
                ProcessMesh(meshList[i], i, options, result);
            }

            FinalizeResult(options, result);
            return result;
        }

        public static PartCreationResult FromBreps(IEnumerable<Brep?> breps, PartCreationOptions options)
        {
            if (breps == null) throw new ArgumentNullException(nameof(breps));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var result = new PartCreationResult();
            var brepList = breps.ToList();

            if (brepList.Count == 0)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Error, "No brep geometry provided."));
                return result;
            }

            for (int i = 0; i < brepList.Count; i++)
            {
                ProcessBrep(brepList[i], i, options, result);
            }

            FinalizeResult(options, result);
            return result;
        }

        private static void ProcessMesh(Mesh? mesh, int index, PartCreationOptions options, PartCreationResult result)
        {
            if (mesh == null)
            {
                RegisterInvalidGeometry(result, index, "mesh");
                return;
            }

            var workingMesh = mesh.DuplicateMesh();
            if (!workingMesh.IsValid)
            {
                RegisterInvalidGeometry(result, index, "mesh");
                return;
            }

            workingMesh.Normals.ComputeNormals();
            var partName = CreatePartName(options.BaseName, index);
            var partGeometry = new PartGeometry(index, workingMesh, partName, mesh, "Mesh");

            AddPartOrGeometry(partGeometry, options, result, partName);
        }

        private static void ProcessBrep(Brep? brep, int index, PartCreationOptions options, PartCreationResult result)
        {
            if (brep == null || !brep.IsValid)
            {
                RegisterInvalidGeometry(result, index, "brep");
                return;
            }

            var meshes = Mesh.CreateFromBrep(brep, MeshingParameters.Default);
            if (meshes == null || meshes.Length == 0)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                    $"Meshing failed at index {index}, skipping."));
                result.FailureCount++;
                return;
            }

            var mergedMesh = MergeMeshes(meshes);
            if (!mergedMesh.IsValid)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                    $"Generated mesh invalid at index {index}, skipping."));
                result.FailureCount++;
                return;
            }

            mergedMesh.Normals.ComputeNormals();
            var partName = CreatePartName(options.BaseName, index);
            var partGeometry = new PartGeometry(index, mergedMesh, partName, brep, "Brep");

            AddPartOrGeometry(partGeometry, options, result, partName);
        }

        private static Mesh MergeMeshes(IEnumerable<Mesh?> meshes)
        {
            var merged = new Mesh();
            foreach (var mesh in meshes)
            {
                if (mesh != null)
                {
                    merged.Append(mesh);
                }
            }
            return merged;
        }

        private static void AddPartOrGeometry(PartGeometry partGeometry, PartCreationOptions options,
            PartCreationResult result, string partName)
        {
            if (options.IncludePhysics && options.Physics != null)
            {
                var part = new Part(partGeometry.PartId, partName, partGeometry, options.Physics);
                result.CreatedItems.Add(part);
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark,
                    $"Created Part '{partName}' with physics: mass={options.Physics.Mass:F3}kg, friction={options.Physics.Friction:F2}"));
            }
            else
            {
                result.CreatedItems.Add(partGeometry);
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark,
                    $"Created Part '{partName}' with {partGeometry.Geometry.Vertices.Count} vertices, {partGeometry.Geometry.Faces.Count} faces"));
            }

            result.SuccessCount++;
        }

        private static void RegisterInvalidGeometry(PartCreationResult result, int index, string geometryType)
        {
            result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                $"Invalid {geometryType} at index {index}, skipping."));
            result.FailureCount++;
        }

        private static void FinalizeResult(PartCreationOptions options, PartCreationResult result)
        {
            var report = $"Created {result.SuccessCount} Part objects";
            if (result.FailureCount > 0) report += $", {result.FailureCount} failed";
            if (options.IncludePhysics && options.Physics != null) report += " (with physics)";
            result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark, report));
        }

        private static string CreatePartName(string baseName, int index)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                return $"Part_{index}";
            }

            return $"{baseName}_{index}";
        }
    }
}
