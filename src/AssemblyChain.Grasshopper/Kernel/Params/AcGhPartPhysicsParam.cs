using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhPartPhysicsParam : GH_PersistentParam<AcGhPartPhysicsGoo>
    {
        public AcGhPartPhysicsParam()
            : base(new GH_InstanceDescription("PartPhysics", "PP", "AssemblyChain part physics (geometry + physics)", "AssemblyChain", "0|Params"))
        {
        }

        public override Guid ComponentGuid => new Guid("b8c9d0e1-f2a3-4567-89ab-cdef01234567");

        protected override GH_GetterResult Prompt_Singular(ref AcGhPartPhysicsGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<AcGhPartPhysicsGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}

