using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhPartWrapParam : GH_PersistentParam<AcGhPartWrapGoo>
    {
        public AcGhPartWrapParam()
            : base(new GH_InstanceDescription("Part", "Part", "AssemblyChain part (geometry with optional physics)", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("d9e0f1a2-b3c4-5678-9abc-def012345678");

        protected override GH_GetterResult Prompt_Singular(ref AcGhPartWrapGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhPartWrapGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}
