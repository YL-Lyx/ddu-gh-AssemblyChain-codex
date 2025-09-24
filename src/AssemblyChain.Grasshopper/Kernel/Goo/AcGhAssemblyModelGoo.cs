using AssemblyChain.Core.Model;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for read-only <see cref="AssemblyModel"/> snapshots.
    /// </summary>
    public class AcGhAssemblyModelGoo : GH_Goo<AssemblyModel>
    {
        public AcGhAssemblyModelGoo()
        {
        }

        public AcGhAssemblyModelGoo(AssemblyModel value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new AcGhAssemblyModelGoo() : new AcGhAssemblyModelGoo(Value);
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "AssemblyModel";

        public override string TypeDescription => "AssemblyChain read-only assembly snapshot";

        public override string ToString()
        {
            return Value == null
                ? "Null AssemblyModel"
                : $"AssemblyModel (parts: {Value.PartCount}, hash: {Value.Hash})";
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case AssemblyModel model:
                    Value = model;
                    return true;
                case AcGhAssemblyModelGoo goo:
                    Value = goo.Value;
                    return true;
                case AcGhAssemblyGoo assemblyGoo:
                    if (AcGhAssemblyModelConversion.TryGetSnapshot(assemblyGoo, out var snapshot, out _))
                    {
                        Value = snapshot;
                        return true;
                    }
                    break;
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null && typeof(T).IsAssignableFrom(typeof(AssemblyModel)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}
