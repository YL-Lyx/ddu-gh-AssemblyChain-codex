namespace AssemblyChain.Gh.Kernel
{
    public class AcGhAssemblyWrapParam : AcGhParamBase<AcGhAssemblyWrapGoo>
    {
        public AcGhAssemblyWrapParam()
            : base("Assembly", "A", "AssemblyChain unified assembly", "AssemblyChain", "0|Params")
        {
        }

        public override Guid ComponentGuid => GuidFromSeed("542a6e96-0353-4704-b4fc-fea3c3668ec2");
    }
}

