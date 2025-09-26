using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhPhysicalPropertyParam : GH_PersistentParam<AcGhPhysicalPropertyGoo>
    {
        public AcGhPhysicalPropertyParam()
            : base(new GH_InstanceDescription("PhysicalProperty", "PP", "AssemblyChain physics properties (mass, friction, etc.)", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("c8d9e0f1-a2b3-4567-89ab-cdef01234567");

        protected override GH_GetterResult Prompt_Singular(ref AcGhPhysicalPropertyGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhPhysicalPropertyGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}
