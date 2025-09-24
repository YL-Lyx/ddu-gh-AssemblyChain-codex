using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhPartGeometryParam : GH_PersistentParam<AcGhPartGeometryGoo>
    {
        public AcGhPartGeometryParam()
            : base(new GH_InstanceDescription("PartGeometry", "PG", "AssemblyChain part geometry (lightweight)", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("a7b8c9d0-e1f2-3456-789a-bcdef0123456");

        protected override GH_GetterResult Prompt_Singular(ref AcGhPartGeometryGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhPartGeometryGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}

