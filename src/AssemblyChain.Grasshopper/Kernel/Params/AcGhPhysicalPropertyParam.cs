namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Parameter wrapper for physics properties.
    /// </summary>
    public class AcGhPhysicalPropertyParam : AcGhParamBase<AcGhPhysicalPropertyGoo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcGhPhysicalPropertyParam"/> class.
        /// </summary>
        public AcGhPhysicalPropertyParam()
            : base("PhysicalProperty", "PP", "AssemblyChain physics properties (mass, friction, etc.)", "AssemblyChain", "0|Params")
        {
        }

        /// <inheritdoc />
        public override System.Guid ComponentGuid => GuidFromSeed("c8d9e0f1-a2b3-4567-89ab-cdef01234567");
    }
}
