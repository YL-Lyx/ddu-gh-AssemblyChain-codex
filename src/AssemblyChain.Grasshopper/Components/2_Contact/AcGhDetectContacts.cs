#nullable enable
using System;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Model;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhDetectContacts : GH_Component
    {
        public AcGhDetectContacts()
            : base("Detect Contacts", "Detect", "Detect contacts for an assembly model", "AssemblyChain", "2|Contact")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcGhAssemblyModelParam(), "Assembly Model", "AM", "Assembly model snapshot", GH_ParamAccess.item);
            pManager.AddParameter(new AcGhDetectionOptionsParam(), "Options", "Opt", "Contact detection options", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhContactModelParam(), "Contact Model", "CM", "Detected contact model", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            var assemblyModelGoo = new AcGhAssemblyModelGoo();
            if (!dataAccess.GetData(0, ref assemblyModelGoo) || assemblyModelGoo.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Valid assembly model required for contact detection.");
                return;
            }

            var assemblyModel = assemblyModelGoo.Value;
            if (assemblyModel.PartCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Assembly model contains no parts.");
            }

            DetectionOptions options = new DetectionOptions();
            var optionsGoo = new AcGhDetectionOptionsGoo();
            if (dataAccess.GetData(1, ref optionsGoo) && optionsGoo != null)
            {
                options = optionsGoo.Value;
            }

            try
            {
                ContactModel contactModel = MeshDetector.DetectContacts(assemblyModel, options);
                if (contactModel == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Contact detection returned null model.");
                    return;
                }

                dataAccess.SetData(0, new AcGhContactModelGoo(contactModel));
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Detected {contactModel.ContactCount} contacts across {contactModel.UniquePairs} part pairs.");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Contact detection failed: {ex.Message}");
            }
        }

        public override Guid ComponentGuid => new Guid("0c9470e9-4b13-4e64-b010-2401c2836260");
    }
}
