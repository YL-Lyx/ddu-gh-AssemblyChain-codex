using AssemblyChain.Core.Domain.Entities;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for unified Assembly
    /// </summary>
    public class AcGhAssemblyGoo : GH_Goo<Assembly>
    {
        public AcGhAssemblyGoo()
        {
        }

        public AcGhAssemblyGoo(Assembly value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new AcGhAssemblyGoo() : new AcGhAssemblyGoo(Value);
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "Assembly";

        public override string TypeDescription => "AssemblyChain unified assembly";

        public override string ToString()
        {
            return this.GetType().FullName;
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case Assembly assembly:
                    Value = assembly;
                    return true;
                case AcGhAssemblyGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null && typeof(T).IsAssignableFrom(typeof(Assembly)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}

