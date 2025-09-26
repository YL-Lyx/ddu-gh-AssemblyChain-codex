#nullable enable
using System;
using System.Collections.Generic;
using AssemblyChain.Core;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhCreateAssembly : GH_Component
    {
        public AcGhCreateAssembly()
            : base("Create Assembly", "CA", "Create an AssemblyChain assembly from parts", "AssemblyChain", "2|Part")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Optional name for the assembly.", GH_ParamAccess.item, string.Empty);
            pManager.AddGenericParameter("Part", "Part", "Parts to include in the assembly", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            AddOutputParameter(pManager, OutputParameterSpec.AssemblyItem());
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            try
            {
                var context = BuildContext(dataAccess);
                if (context == null)
                {
                    return;
                }

                var extractionResult = ExtractParts(context);
                if (extractionResult.Parts.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No valid parts found.");
                    return;
                }

                var assembly = BuildAssembly(context.AssemblyName, extractionResult.Parts);
                PublishAssembly(dataAccess, context.AssemblyName, extractionResult, assembly);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Assembly creation failed: {ex.Message}");
            }
        }

        private AssemblyCreationContext BuildContext(IGH_DataAccess dataAccess)
        {
            string baseName = string.Empty;
            dataAccess.GetData(0, ref baseName);
            var partGoos = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(1, partGoos) || partGoos.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No parts provided.");
                return null;
            }

            return new AssemblyCreationContext(baseName, partGoos);
        }

        private AssemblyPartExtractionResult ExtractParts(AssemblyCreationContext context)
        {
            var result = new AssemblyPartExtractionResult();

            foreach (var goo in context.PartGoos)
            {
                try
                {
                    var part = ConvertToPart(goo);
                    if (part != null)
                    {
                        result.RegisterSuccess(part);
                    }
                    else
                    {
                        result.RegisterFailure();
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            $"Unsupported input type: {goo?.GetType().Name ?? "null"}");
                    }
                }
                catch (Exception ex)
                {
                    result.RegisterFailure();
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        $"Failed to process part: {ex.Message}");
                }
            }

            return result;
        }

        private Part? ConvertToPart(IGH_Goo goo)
        {
            if (goo is AcGhPartWrapGoo partGoo)
            {
                if (partGoo.Value == null || partGoo.CompletePart == null)
                {
                    throw new InvalidOperationException("Invalid part data in unified Goo");
                }

                return partGoo.CompletePart;
            }

            if (goo is GH_ObjectWrapper wrapper && wrapper.Value is Part part)
            {
                return part;
            }

            return null;
        }

        private Assembly BuildAssembly(string assemblyName, IReadOnlyCollection<Part> parts)
        {
            var assembly = new Assembly(0, assemblyName);
            foreach (var part in parts)
            {
                assembly.AddPart(part);
            }

            return assembly;
        }

        private void PublishAssembly(IGH_DataAccess dataAccess, string assemblyName, AssemblyPartExtractionResult extractionResult, Assembly assembly)
        {
            dataAccess.SetData(0, new AcGhAssemblyWrapGoo(assembly));

            var report = $"Created assembly '{assemblyName}' with {extractionResult.Parts.Count} parts";
            if (extractionResult.FailureCount > 0)
            {
                report += $", {extractionResult.FailureCount} failed";
            }

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, report);
        }

        private static void AddOutputParameter(GH_OutputParamManager manager, OutputParameterSpec spec)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (spec == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }

            manager.AddParameter(spec.CreateParameter(), spec.Name, spec.Nickname, spec.Description, spec.Access);
        }

        private sealed class AssemblyCreationContext
        {
            public AssemblyCreationContext(string baseName, List<IGH_Goo> partGoos)
            {
                BaseName = baseName?.Trim();
                PartGoos = partGoos;
            }

            public string BaseName { get; }

            public List<IGH_Goo> PartGoos { get; }

            public string AssemblyName => string.IsNullOrWhiteSpace(BaseName) ? "Assembly" : BaseName;
        }

        private sealed class AssemblyPartExtractionResult
        {
            public List<Part> Parts { get; } = new();

            public int FailureCount { get; private set; }

            public void RegisterSuccess(Part part)
            {
                Parts.Add(part);
            }

            public void RegisterFailure()
            {
                FailureCount++;
            }
        }

        private sealed class OutputParameterSpec
        {
            private OutputParameterSpec(Func<IGH_Param> factory, string name, string nickname, string description, GH_ParamAccess access)
            {
                ParameterFactory = factory;
                Name = name;
                Nickname = nickname;
                Description = description;
                Access = access;
            }

            private Func<IGH_Param> ParameterFactory { get; }

            public string Name { get; }

            public string Nickname { get; }

            public string Description { get; }

            public GH_ParamAccess Access { get; }

            public IGH_Param CreateParameter() => ParameterFactory();

            public static OutputParameterSpec AssemblyItem()
            {
                return new OutputParameterSpec(
                    () => new AcGhAssemblyWrapParam(),
                    "Assembly",
                    "A",
                    "AssemblyChain unified assembly from parts",
                    GH_ParamAccess.item);
            }
        }


        public override Guid ComponentGuid => new Guid("543b11a7-30c7-4da3-902e-783d794a1929");
    }
}