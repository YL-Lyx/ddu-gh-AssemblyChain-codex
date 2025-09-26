using System;
using AssemblyChain.Core.Contact;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for ContactModel
    /// </summary>
    public class AcGhContactModelGoo : AcGhGooBase<ContactModel>
    {
        public AcGhContactModelGoo()
        {
        }

        public AcGhContactModelGoo(ContactModel value)
            : base(value)
        {
        }

        protected override AcGhGooBase<ContactModel> CreateInstance(ContactModel value)
        {
            return new AcGhContactModelGoo(value);
        }

        protected override AcGhGooBase<ContactModel> CreateEmpty()
        {
            return new AcGhContactModelGoo();
        }

        public override string TypeName => "ContactModel";

        public override string TypeDescription => "AssemblyChain contact model containing contact data and neighbor relationships";

        public override string ToString()
        {
            if (Value == null)
                return "Null ContactModel";

            return $"ContactModel: {Value.ContactCount} contacts, {Value.UniquePairs} pairs, Hash: {Value.Hash}";
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case ContactModel contactModel:
                    Value = contactModel;
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
