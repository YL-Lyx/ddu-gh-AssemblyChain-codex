using System;
using System.Collections.Generic;
using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Defines physics properties for parts
    /// </summary>
    public class AcGhDefinePhysicalProperty : GH_Component
    {
        public AcGhDefinePhysicalProperty()
            : base("Part_Physical Property", "PPP", "Define physics properties for parts (mass, friction, etc.)", "AssemblyChain", "1|Property")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Mass", "Mass", "Mass of the part (kg)", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Friction", "Friction", "Friction coefficient (0-1)", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Restitution", "Restitution", "Restitution coefficient (0-1)", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("RollingFriction", "RollingFriction", "Rolling friction coefficient (0-1)", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("SpinningFriction", "SpinningFriction", "Spinning friction coefficient (0-1)", GH_ParamAccess.item, 0.01);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhPhysicalPropertyParam(), "Physical", "Physical", "Physics properties for parts", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                // Get inputs
                double mass = 1.0;
                double friction = 0.5;
                double restitution = 0.1;
                double rollingFriction = 0.01;
                double spinningFriction = 0.01;

                // Store original input values for display
                double originalMass = mass;
                double originalFriction = friction;
                double originalRestitution = restitution;
                double originalRollingFriction = rollingFriction;
                double originalSpinningFriction = spinningFriction;

                if (!DA.GetData(0, ref mass)) return;
                if (!DA.GetData(1, ref friction)) return;
                if (!DA.GetData(2, ref restitution)) return;
                if (!DA.GetData(3, ref rollingFriction)) return;
                if (!DA.GetData(4, ref spinningFriction)) return;

                // Store the actual input values for display
                originalMass = mass;
                originalFriction = friction;
                originalRestitution = restitution;
                originalRollingFriction = rollingFriction;
                originalSpinningFriction = spinningFriction;

                // Validate and clamp physics properties
                mass = Math.Max(0.001, mass);
                friction = Math.Clamp(friction, 0.0, 1.0);
                restitution = Math.Clamp(restitution, 0.0, 1.0);
                rollingFriction = Math.Clamp(rollingFriction, 0.0, 1.0);
                spinningFriction = Math.Clamp(spinningFriction, 0.0, 1.0);

                // Create physics properties
                var physicsProperties = new PhysicsProperties(mass, friction, restitution, rollingFriction, spinningFriction);

                // Create Goo wrapper and output
                var goo = new AcGhPhysicalPropertyGoo(physicsProperties);
                DA.SetData(0, goo);

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Created PhysicsProperties: Mass={originalMass:F3}kg, Friction={originalFriction:F2}, Restitution={originalRestitution:F2}, RollingFriction={originalRollingFriction:F3}, SpinningFriction={originalSpinningFriction:F3}");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error creating physics properties: {ex.Message}");
            }
        }

        public override Guid ComponentGuid => new Guid("e0f1a2b3-c4d5-6789-abcd-ef0123456789");
    }
}
