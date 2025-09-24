using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhDetectionOptionsParam : GH_PersistentParam<AcGhDetectionOptionsGoo>
    {
        public AcGhDetectionOptionsParam()
            : base(new GH_InstanceDescription("DetectionOptions", "Opt", "Contact detection options", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("f5a93f35-7fcb-4ff2-9c5e-1d517a339d80");

        protected override GH_GetterResult Prompt_Singular(ref AcGhDetectionOptionsGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhDetectionOptionsGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}
