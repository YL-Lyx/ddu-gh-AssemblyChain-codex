using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for PhysicsProperties - physical properties only
    /// </summary>
    public class AcGhPhysicalPropertyGoo : GH_Goo<PhysicsProperties>
    {
        public AcGhPhysicalPropertyGoo()
        {
        }

        public AcGhPhysicalPropertyGoo(PhysicsProperties value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new AcGhPhysicalPropertyGoo() : new AcGhPhysicalPropertyGoo(Value);
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "PhysicsProperties";

        public override string TypeDescription => "AssemblyChain physics properties (mass, friction, etc.)";

        public override string ToString()
        {
            if (Value == null) return "Null PhysicsProperties";
            return $"PhysicsProperties: Mass={Value.Mass:F3}kg, Friction={Value.Friction:F2}, Restitution={Value.Restitution:F2}, RollingFriction={Value.RollingFriction:F3}, SpinningFriction={Value.SpinningFriction:F3}";
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case PhysicsProperties physicsProperties:
                    Value = physicsProperties;
                    return true;
                case AcGhPhysicalPropertyGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null && typeof(T).IsAssignableFrom(typeof(PhysicsProperties)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}
