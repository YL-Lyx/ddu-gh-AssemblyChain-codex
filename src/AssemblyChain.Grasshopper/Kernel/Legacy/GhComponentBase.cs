#if !GRASSHOPPER
namespace AssemblyChain.Gh.Kernel.Legacy;

/// <summary>
/// Minimal stand-in for the Grasshopper GH_Component base class so the project can
/// compile on environments without the Rhino SDK. When the GRASSHOPPER symbol is
/// defined, the real API is used instead.
/// </summary>
public abstract class GhComponentBase
{
    protected GhComponentBase(string name, string nickname, string description, string category, string subcategory)
    {
        Name = name;
        NickName = nickname;
        Description = description;
        Category = category;
        SubCategory = subcategory;
    }

    public string Name { get; }

    public string NickName { get; }

    public string Description { get; }

    public string Category { get; }

    public string SubCategory { get; }

    public abstract void SolveInstance(IGhDataAccess dataAccess);
}
#endif
