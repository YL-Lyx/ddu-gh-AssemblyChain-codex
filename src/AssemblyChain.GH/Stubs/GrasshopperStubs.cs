#if !GRASSHOPPER
namespace AssemblyChain.GH.Stubs;

/// <summary>
/// Minimal stand-ins for Grasshopper base types so the project can build without the Rhino SDK.
/// Define the <c>GRASSHOPPER</c> symbol to compile against the real API.
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

    public virtual void SolveInstance() => throw new System.NotImplementedException("Grasshopper runtime not available in stub build.");
}
#endif
