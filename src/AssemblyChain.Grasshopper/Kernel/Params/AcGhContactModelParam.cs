using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhContactModelParam : GH_PersistentParam<AcGhContactModelGoo>
    {
        public AcGhContactModelParam()
            : base(new GH_InstanceDescription("ContactModel", "CM", "AssemblyChain contact model", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("c710fc36-6b1a-4233-8694-8e6ac65b7c7f");

        protected override GH_GetterResult Prompt_Singular(ref AcGhContactModelGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhContactModelGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}
