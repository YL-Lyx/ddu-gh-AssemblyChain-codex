using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for PartGeometry - lightweight geometry-only part representation
    /// </summary>
    public class AcGhPartGeometryGoo : GH_Goo<PartGeometry>
    {
        public AcGhPartGeometryGoo()
        {
        }

        public AcGhPartGeometryGoo(PartGeometry value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new AcGhPartGeometryGoo() : new AcGhPartGeometryGoo(Value);
        }

        public override bool IsValid => Value != null && Value.HasValidGeometry;

        public override string TypeName => "PartGeometry";

        public override string TypeDescription => "AssemblyChain part geometry (lightweight)";

        public override string ToString()
        {
            return this.GetType().FullName;
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case PartGeometry partGeometry:
                    Value = partGeometry;
                    return true;
                case AcGhPartGeometryGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null && typeof(T).IsAssignableFrom(typeof(PartGeometry)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}

