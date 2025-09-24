#nullable enable
using System;
using AssemblyChain.Core.Contact;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhDetectOptions : GH_Component
    {
        public AcGhDetectOptions()
            : base("Detect Options", "Opt", "Configure contact detection options", "AssemblyChain", "2|Contact")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Tolerance", "Tol", "Detection tolerance", GH_ParamAccess.item, 1e-6);
            pManager.AddNumberParameter("Min Patch Area", "Area", "Minimum contact patch area", GH_ParamAccess.item, 1e-6);
            pManager.AddTextParameter("Broad Phase", "BP", "Broad-phase strategy identifier", GH_ParamAccess.item, "sap");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhDetectionOptionsParam(), "Options", "Opt", "Contact detection options", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            double tolerance = 1e-6;
            double minArea = 1e-6;
            string? broadPhase = "sap";

            dataAccess.GetData(0, ref tolerance);
            dataAccess.GetData(1, ref minArea);
            dataAccess.GetData(2, ref broadPhase);

            if (double.IsNaN(tolerance) || tolerance <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Tolerance must be positive. Falling back to 1e-6.");
                tolerance = 1e-6;
            }

            if (double.IsNaN(minArea) || minArea < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Minimum patch area must be non-negative. Falling back to 1e-6.");
                minArea = 1e-6;
            }

            if (string.IsNullOrWhiteSpace(broadPhase))
            {
                broadPhase = "sap";
            }

            var options = new DetectionOptions(tolerance, minArea, broadPhase);
            dataAccess.SetData(0, new AcGhDetectionOptionsGoo(options));

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                $"Detection options -> Tol: {tolerance:G3}, MinArea: {minArea:G3}, BroadPhase: {broadPhase}");
        }

        public override Guid ComponentGuid => new Guid("2a1d85c5-05d2-47eb-9beb-19ba75a828db");
    }
}
