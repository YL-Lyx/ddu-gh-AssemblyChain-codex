using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhAssemblyWrapParam : GH_PersistentParam<AcGhAssemblyWrapGoo>
    {
        public AcGhAssemblyWrapParam()
            : base(new GH_InstanceDescription("Assembly", "A", "AssemblyChain unified assembly", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("542a6e96-0353-4704-b4fc-fea3c3668ec2");

        protected override GH_GetterResult Prompt_Singular(ref AcGhAssemblyWrapGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhAssemblyWrapGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}

