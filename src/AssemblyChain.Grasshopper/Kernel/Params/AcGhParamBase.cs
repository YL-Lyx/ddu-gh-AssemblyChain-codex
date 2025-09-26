using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Shared persistent parameter base to remove duplicated prompt logic.
    /// </summary>
    /// <typeparam name="TGoo">Associated Goo type.</typeparam>
    public abstract class AcGhParamBase<TGoo> : GH_PersistentParam<TGoo>
        where TGoo : class, IGH_Goo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcGhParamBase{TGoo}"/> class.
        /// </summary>
        /// <param name="name">Display name.</param>
        /// <param name="nickname">Nickname.</param>
        /// <param name="description">Description.</param>
        /// <param name="category">Category.</param>
        /// <param name="subcategory">Subcategory.</param>
        protected AcGhParamBase(string name, string nickname, string description, string category, string subcategory)
            : base(new GH_InstanceDescription(name, nickname, description, category, subcategory))
        {
        }

        /// <inheritdoc />
        protected override GH_GetterResult Prompt_Singular(ref TGoo value)
        {
            return GH_GetterResult.cancel;
        }

        /// <inheritdoc />
        protected override GH_GetterResult Prompt_Plural(ref List<TGoo> values)
        {
            return GH_GetterResult.cancel;
        }

        /// <summary>
        /// Utility helper for deterministic GUID definitions.
        /// </summary>
        /// <param name="seed">String seed.</param>
        /// <returns>Deterministic <see cref="Guid"/>.</returns>
        protected static Guid GuidFromSeed(string seed)
        {
            return new Guid(seed);
        }
    }
}
