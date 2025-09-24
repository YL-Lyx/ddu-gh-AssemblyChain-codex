using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhAssemblyModelParam : GH_PersistentParam<AcGhAssemblyModelGoo>
    {
        public AcGhAssemblyModelParam()
            : base(new GH_InstanceDescription("AssemblyModel", "AM", "AssemblyChain read-only assembly model", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("4c8e31a7-3af3-4f66-97f8-45a3b758d84a");

        protected override GH_GetterResult Prompt_Singular(ref AcGhAssemblyModelGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhAssemblyModelGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}
