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
            pManager.AddParameter(new AcGhAssemblyWrapParam(), "Assembly", "A", "AssemblyChain unified assembly from parts", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            try
            {
                // Get assembly name
                string baseName = string.Empty;
                dataAccess.GetData(0, ref baseName);
                if (string.IsNullOrWhiteSpace(baseName))
                {
                    baseName = "Assembly";
                }

                // Get part inputs
                var partGoos = new List<IGH_Goo>();
                if (!dataAccess.GetDataList(1, partGoos) || partGoos.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No parts provided.");
                    return;
                }

                // Collect all valid parts
                var validParts = new List<Part>();
                int successCount = 0, failureCount = 0;

                foreach (var goo in partGoos)
                {
                    try
                    {
                        if (goo is AcGhPartWrapGoo partGoo && partGoo.Value != null)
                        {
                            // Unified Goo contains either PartGeometry or complete Part
                            var part = partGoo.CompletePart;
                            if (part != null)
                            {
                                validParts.Add(part);
                                successCount++;
                            }
                            else
                            {
                                failureCount++;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid part data in unified Goo");
                            }
                        }
                        else if (goo is GH_ObjectWrapper wrapper && wrapper.Value is Part part)
                        {
                            validParts.Add(part);
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Unsupported input type: {goo?.GetType().Name ?? "null"}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            $"Failed to process part: {ex.Message}");
                    }
                }

                if (validParts.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No valid parts found.");
                    return;
                }

                // Create single assembly with all parts
                var assembly = new Assembly(0, baseName);

                // Add parts to assembly
                foreach (var part in validParts)
                {
                    assembly.AddPart(part);
                }

                // Set output
                var assemblyGoo = new AcGhAssemblyWrapGoo(assembly);
                dataAccess.SetData(0, assemblyGoo);

                // Report results
                var report = $"Created assembly '{baseName}' with {successCount} parts";
                if (failureCount > 0) report += $", {failureCount} failed";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, report);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Assembly creation failed: {ex.Message}");
            }
        }


        public override Guid ComponentGuid => new Guid("543b11a7-30c7-4da3-902e-783d794a1929");
    }
}