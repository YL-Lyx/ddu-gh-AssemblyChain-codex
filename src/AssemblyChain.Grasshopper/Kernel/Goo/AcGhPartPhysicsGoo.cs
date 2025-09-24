using AssemblyChain.Core.Domain.Entities;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for PartPhysics - complete part with geometry and physics properties
    /// </summary>
    public class AcGhPartPhysicsGoo : GH_Goo<Part>
    {
        public AcGhPartPhysicsGoo()
        {
        }

        public AcGhPartPhysicsGoo(Part value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new AcGhPartPhysicsGoo() : new AcGhPartPhysicsGoo(Value);
        }

        public override bool IsValid => Value != null && Value.HasValidGeometry;

        public override string TypeName => "PartPhysics";

        public override string TypeDescription => "AssemblyChain part physics (geometry + physics)";

        public override string ToString()
        {
            return this.GetType().FullName;
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case Part part:
                    Value = part;
                    return true;
                case AcGhPartPhysicsGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null && typeof(T).IsAssignableFrom(typeof(Part)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}

