namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Parameter wrapper for parts.
    /// </summary>
    public class AcGhPartWrapParam : AcGhParamBase<AcGhPartWrapGoo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcGhPartWrapParam"/> class.
        /// </summary>
        public AcGhPartWrapParam()
            : base("Part", "Part", "AssemblyChain part (geometry with optional physics)", "AssemblyChain", "0|Params")
        {
        }

        /// <inheritdoc />
        public override System.Guid ComponentGuid => GuidFromSeed("d9e0f1a2-b3c4-5678-9abc-def012345678");
    }
}
