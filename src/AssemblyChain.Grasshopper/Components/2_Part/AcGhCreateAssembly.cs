#nullable enable
using System;
using System.Collections.Generic;
using AssemblyChain.Core;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Toolkit.Processing;
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
            pManager.AddParameter(new AcGhAssemblyWrapParam(), "Assembly", "A", "AssemblyChain unified assembly from parts", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            try
            {
                string baseName = GetAssemblyName(dataAccess);

                var partGoos = new List<IGH_Goo>();
                if (!dataAccess.GetDataList(1, partGoos) || partGoos.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No parts provided.");
                    return;
                }

                var extraction = ExtractParts(partGoos);
                EmitMessages(extraction.messages);

                var result = AssemblyBuilder.Build(baseName, extraction.parts);
                EmitMessages(result.Messages);

                if (!result.HasAssembly)
                {
                    return;
                }

                dataAccess.SetData(0, new AcGhAssemblyWrapGoo(result.Assembly));
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Assembly creation failed: {ex.Message}");
            }
        }

        private static string GetAssemblyName(IGH_DataAccess dataAccess)
        {
            string baseName = string.Empty;
            dataAccess.GetData(0, ref baseName);
            return string.IsNullOrWhiteSpace(baseName) ? "Assembly" : baseName.Trim();
        }

        private (List<Part?> parts, List<ProcessingMessage> messages) ExtractParts(IEnumerable<IGH_Goo> goos)
        {
            var parts = new List<Part?>();
            var messages = new List<ProcessingMessage>();

            foreach (var goo in goos)
            {
                try
                {
                    switch (goo)
                    {
                        case AcGhPartWrapGoo partGoo when partGoo.Value != null:
                            parts.Add(partGoo.CompletePart);
                            if (partGoo.CompletePart == null)
                            {
                                messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                                    "Invalid part data in unified Goo"));
                            }
                            break;
                        case GH_ObjectWrapper wrapper when wrapper.Value is Part part:
                            parts.Add(part);
                            break;
                        default:
                            parts.Add(null);
                            messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                                $"Unsupported input type: {goo?.GetType().Name ?? "null"}"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    parts.Add(null);
                    messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                        $"Failed to process part: {ex.Message}"));
                }
            }

            return (parts, messages);
        }

        private void EmitMessages(IEnumerable<ProcessingMessage> messages)
        {
            foreach (var message in messages)
            {
                AddRuntimeMessage(ToRuntimeLevel(message.Level), message.Text);
            }
        }

        private static GH_RuntimeMessageLevel ToRuntimeLevel(ProcessingMessageLevel level)
        {
            return level switch
            {
                ProcessingMessageLevel.Remark => GH_RuntimeMessageLevel.Remark,
                ProcessingMessageLevel.Warning => GH_RuntimeMessageLevel.Warning,
                ProcessingMessageLevel.Error => GH_RuntimeMessageLevel.Error,
                _ => GH_RuntimeMessageLevel.Remark
            };
        }


        public override Guid ComponentGuid => new Guid("543b11a7-30c7-4da3-902e-783d794a1929");
    }
}