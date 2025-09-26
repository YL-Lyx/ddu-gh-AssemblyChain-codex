using AssemblyChain.Constraints;
using AssemblyChain.Core.Spatial;
using AssemblyChain.Gh.Kernel.Legacy;
using AssemblyChain.Geometry.ContactDetection;

namespace AssemblyChain.Gh.Components.Legacy;

public sealed class ContactDetectorComponent : AssemblyChainComponentBase
{
    public ContactDetectorComponent()
        : base("Detect Contacts", "Contacts", "Detect point/line/face contacts", "AssemblyChain", "Constraints")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        if (assemblyWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhContacts));
            return;
        }

        var detector = new ContactDetector();
        var contacts = detector.DetectContacts(assemblyWrapper.Value);
        dataAccess.SetOutput(0, new GhContacts(contacts));
    }
}

public sealed class DirectionConeComponent : AssemblyChainComponentBase
{
    public DirectionConeComponent()
        : base("Direction Cones", "Cones", "Compute direction cones", "AssemblyChain", "Constraints")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        var contactsWrapper = dataAccess.GetInput<GhContacts>(1);
        if (assemblyWrapper is null || contactsWrapper is null)
        {
            dataAccess.SetOutput(0, default(GhDirectionCones));
            return;
        }

        var cones = new DirectionConeBuilder().BuildCones(assemblyWrapper.Value, contactsWrapper.Value);
        dataAccess.SetOutput(0, new GhDirectionCones(cones));
    }
}

public sealed class ConstraintSetComponent : AssemblyChainComponentBase
{
    public ConstraintSetComponent()
        : base("Constraint Set", "HalfSpaces", "Build half-space intersection for a part", "AssemblyChain", "Constraints")
    {
    }

    protected override void Solve(IGhDataAccess dataAccess)
    {
        var assemblyWrapper = dataAccess.GetInput<GhAssembly>(0);
        var conesWrapper = dataAccess.GetInput<GhDirectionCones>(1);
        var partId = dataAccess.GetInput<string>(2) ?? string.Empty;
        if (assemblyWrapper is null || conesWrapper is null || string.IsNullOrWhiteSpace(partId))
        {
            dataAccess.SetOutput(0, System.Array.Empty<HalfSpace>());
            return;
        }

        var builder = new HalfSpaceIntersectionBuilder();
        var halfSpaces = builder.Build(assemblyWrapper.Value, conesWrapper.Value, partId);
        dataAccess.SetOutput(0, halfSpaces);
    }
}
