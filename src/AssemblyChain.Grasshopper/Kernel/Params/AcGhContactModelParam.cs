using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhContactModelParam : GH_PersistentParam<AcGhContactModelGoo>
    {
        public AcGhContactModelParam()
            : base(new GH_InstanceDescription("ContactModel", "CM", "AssemblyChain contact model containing contact data", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("8f4c7e5d-9a2b-4c8f-9d1e-3f7a2b5c8e9f");

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
