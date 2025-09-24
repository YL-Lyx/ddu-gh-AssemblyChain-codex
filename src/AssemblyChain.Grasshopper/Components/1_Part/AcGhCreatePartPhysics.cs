using System;
using System.Collections.Generic;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Domain.Entities;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Creates PartPhysics objects by combining PartGeometry with physics properties
    /// </summary>
    public class AcGhCreatePartPhysics : GH_Component
    {
        public AcGhCreatePartPhysics()
            : base("Create PartPhysics", "CPP", "Create PartPhysics objects by combining PartGeometry with physics properties.", "AssemblyChain", "1|Part")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcGhPartGeometryParam(), "Part", "PG", "PartGeometry objects to add physics to", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mass", "Mass", "Mass of the part (kg)", GH_ParamAccess.list, 1.0);
            pManager.AddNumberParameter("Friction", "Friction", "Friction coefficient (0-1)", GH_ParamAccess.list, 0.5);
            pManager.AddNumberParameter("Restitution", "Restitution", "Restitution coefficient (0-1)", GH_ParamAccess.list, 0.1);
            pManager.AddNumberParameter("RollingFriction", "RollingFriction", "Rolling friction coefficient (0-1)", GH_ParamAccess.list, 0.01);
            pManager.AddNumberParameter("SpinningFriction", "SpinningFriction", "Spinning friction coefficient (0-1)", GH_ParamAccess.list, 0.01);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhPartPhysicsParam(), "Part", "PP", "AssemblyChain part physics", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                // Get inputs
                var partGeometryGoos = new List<AcGhPartGeometryGoo>();
                if (!DA.GetDataList(0, partGeometryGoos) || partGeometryGoos.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No PartGeometry objects provided.");
                    return;
                }

                var masses = new List<double>();
                var frictions = new List<double>();
                var restitutions = new List<double>();
                var rollingFrictions = new List<double>();
                var spinningFrictions = new List<double>();

                DA.GetDataList(1, masses);
                DA.GetDataList(2, frictions);
                DA.GetDataList(3, restitutions);
                DA.GetDataList(4, rollingFrictions);
                DA.GetDataList(5, spinningFrictions);

                var partPhysicsList = new List<Part>();
                int successCount = 0;
                int failureCount = 0;

                int count = partGeometryGoos.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var partGeometry = partGeometryGoos[i]?.Value;
                        if (partGeometry == null)
                        {
                            failureCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid PartGeometry at index {i}, skipping.");
                            continue;
                        }

                        // Get physics properties (use defaults if not provided)
                        var mass = (masses != null && i < masses.Count) ? masses[i] : 1.0;
                        var friction = (frictions != null && i < frictions.Count) ? frictions[i] : 0.5;
                        var rollingFriction = (rollingFrictions != null && i < rollingFrictions.Count) ? rollingFrictions[i] : 0.01;
                        var spinningFriction = (spinningFrictions != null && i < spinningFrictions.Count) ? spinningFrictions[i] : 0.01;
                        var restitution = (restitutions != null && i < restitutions.Count) ? restitutions[i] : 0.1;

                        // Validate and clamp physics properties
                        mass = Math.Max(0.001, mass);
                        friction = Math.Clamp(friction, 0.0, 1.0);
                        rollingFriction = Math.Clamp(rollingFriction, 0.0, 1.0);
                        spinningFriction = Math.Clamp(spinningFriction, 0.0, 1.0);
                        restitution = Math.Clamp(restitution, 0.0, 1.0);

                        var physics = new PhysicsProperties(mass, friction, restitution, rollingFriction, spinningFriction);
                        var partPhysics = new Part(partGeometry.IndexId, $"PartPhysics_{partGeometry.IndexId}", partGeometry, physics);

                        partPhysicsList.Add(partPhysics);
                        successCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                            $"Created PartPhysics '{partPhysics.Name}' with mass {mass:F3}kg, friction {friction:F2}");
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            $"Error creating PartPhysics at index {i}: {ex.Message}");
                    }
                }

                // Report and output
                var report = $"Created {successCount} PartPhysics objects";
                if (failureCount > 0) report += $", {failureCount} failed";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, report);

                var goos = partPhysicsList.ConvertAll(pp => new AcGhPartPhysicsGoo(pp));
                DA.SetDataList(0, goos);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
            }
        }

        public override Guid ComponentGuid => new Guid("d0e1f2a3-b4c5-6789-abcd-ef0123456789");
    }
}




