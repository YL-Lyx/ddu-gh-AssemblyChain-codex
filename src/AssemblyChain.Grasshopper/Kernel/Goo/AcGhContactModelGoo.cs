using AssemblyChain.Core.Model;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for <see cref="ContactModel"/> instances.
    /// </summary>
    public class AcGhContactModelGoo : GH_Goo<ContactModel>
    {
        public AcGhContactModelGoo()
        {
        }

        public AcGhContactModelGoo(ContactModel value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return Value == null ? new AcGhContactModelGoo() : new AcGhContactModelGoo(Value);
        }

        public override bool IsValid => Value != null;

        public override string TypeName => "ContactModel";

        public override string TypeDescription => "AssemblyChain contact model";

        public override string ToString()
        {
            return Value == null
                ? "Null ContactModel"
                : $"Contacts: {Value.ContactCount}, Pairs: {Value.UniquePairs}, Hash: {Value.Hash}";
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case ContactModel model:
                    Value = model;
                    return true;
                case AcGhContactModelGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null && typeof(T).IsAssignableFrom(typeof(ContactModel)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}
