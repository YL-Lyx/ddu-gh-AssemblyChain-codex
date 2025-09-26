#nullable enable
using System;
using System.Linq;
using AssemblyChain.Core;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Facade;
using AssemblyChain.Core.Model;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhBuildContactModel : GH_Component
    {
        private readonly AssemblyChainFacade _facade = new();

        public AcGhBuildContactModel()
            : base("Build Contact Model", "BCM", "Build contact model from assembly using specified detection options", "AssemblyChain", "3|Solver")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcGhAssemblyWrapParam(), "Assembly", "A", "AssemblyChain assembly to analyze", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "T", "Detection tolerance", GH_ParamAccess.item, 1e-4);
            pManager.AddNumberParameter("MinPatchArea", "MPA", "Minimum patch area to consider", GH_ParamAccess.item, 0.0);
            pManager.AddTextParameter("BroadPhase", "BP", "Broad phase algorithm (SAP/RTree)", GH_ParamAccess.item, "SAP");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhContactModelParam(), "ContactModel", "CM", "Contact model containing all contact data", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            try
            {
                // Get assembly input
                AcGhAssemblyWrapGoo assemblyGoo = null!;
                if (!dataAccess.GetData(0, ref assemblyGoo) || assemblyGoo?.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid assembly input.");
                    return;
                }

                // Get detection parameters
                double tolerance = 1e-4;
                dataAccess.GetData(1, ref tolerance);

                double minPatchArea = 0.0;
                dataAccess.GetData(2, ref minPatchArea);

                string broadPhase = "SAP";
                dataAccess.GetData(3, ref broadPhase);

                // Convert assembly to assembly model
                var assembly = assemblyGoo.Value;
                var assemblyModel = AssemblyModelFactory.Create(assembly);

                // Create detection options
                var detectionOptions = new DetectionOptions(
                    Tolerance: tolerance,
                    MinPatchArea: minPatchArea,
                    BroadPhase: broadPhase
                );

                // Build contact model
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Starting contact detection with {assemblyModel.PartCount} parts, Tolerance={tolerance:F6}, MinPatchArea={minPatchArea:F6}");

                var contactModel = _facade.DetectContacts(assemblyModel, detectionOptions);

                // Report results
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Built contact model: {contactModel.ContactCount} contacts, {contactModel.UniquePairs} pairs");

                // 详细调试信息
                var faceContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Face).Count();
                var edgeContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Edge).Count();
                var pointContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Point).Count();

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Contact breakdown: Face={faceContacts}, Edge={edgeContacts}, Point={pointContacts}");

                foreach (var contact in contactModel.Contacts.Where(c => c.Type == ContactType.Face))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                        $"Face contact: {contact.PartAId}-{contact.PartBId}, Area={contact.Area:F6}, HasGeom={contact.Zone.Geometry != null}");
                }

                // Set output
                var contactModelGoo = new AcGhContactModelGoo(contactModel);
                dataAccess.SetData(0, contactModelGoo);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"Contact model building failed: {ex.Message}");
            }
        }

        public override Guid ComponentGuid => new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    }
}
