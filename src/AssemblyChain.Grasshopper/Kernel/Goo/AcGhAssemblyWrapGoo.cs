using System;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Planning.Model;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for unified Assembly
    /// </summary>
    public class AcGhAssemblyWrapGoo : AcGhGooBase<Assembly>
    {
        private AssemblyModel _cachedAssemblyModel;
        private string _cachedAssemblyHash = string.Empty;

        public AcGhAssemblyWrapGoo()
        {
        }

        public AcGhAssemblyWrapGoo(Assembly value)
            : base(value)
        {
        }

        protected override AcGhGooBase<Assembly> CreateInstance(Assembly value)
        {
            return new AcGhAssemblyWrapGoo(value);
        }

        protected override AcGhGooBase<Assembly> CreateEmpty()
        {
            return new AcGhAssemblyWrapGoo();
        }

        public override string TypeName => "Assembly";

        public override string TypeDescription => "AssemblyChain unified assembly";

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case Assembly assembly:
                    Value = assembly;
                    return true;
                case AcGhAssemblyWrapGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null)
            {
                if (typeof(T).IsAssignableFrom(typeof(Assembly)))
                {
                    target = (T)(object)Value;
                    return true;
                }

                if (typeof(T).IsAssignableFrom(typeof(AssemblyModel)))
                {
                    var model = EnsureAssemblyModel();
                    if (model != null)
                    {
                        target = (T)(object)model;
                        return true;
                    }
                }
            }

            return base.CastTo(ref target);
        }

        private AssemblyModel EnsureAssemblyModel()
        {
            if (Value == null)
            {
                return null;
            }

            var model = AssemblyModelFactory.Create(Value);
            if (!string.Equals(_cachedAssemblyHash, model.Hash, StringComparison.Ordinal))
            {
                _cachedAssemblyModel = model;
                _cachedAssemblyHash = model.Hash;
            }

            return _cachedAssemblyModel ?? model;
        }
    }
}

