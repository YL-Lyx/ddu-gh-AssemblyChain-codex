namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Parameter wrapper for contact models.
    /// </summary>
    public class AcGhContactModelParam : AcGhParamBase<AcGhContactModelGoo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcGhContactModelParam"/> class.
        /// </summary>
        public AcGhContactModelParam()
            : base("ContactModel", "CM", "AssemblyChain contact model containing contact data", "AssemblyChain", "0|Params")
        {
        }

        /// <inheritdoc />
        public override System.Guid ComponentGuid => GuidFromSeed("8f4c7e5d-9a2b-4c8f-9d1e-3f7a2b5c8e9f");
    }
}
