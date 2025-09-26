#if !GRASSHOPPER
namespace AssemblyChain.Gh.Kernel.Legacy;

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

    public abstract void SolveInstance(IGhDataAccess dataAccess);
}

/// <summary>
/// Minimal data access helper that mirrors Grasshopper's IGH_DataAccess semantics for tests.
/// </summary>
public sealed class GhDataAccess : IGhDataAccess
{
    private readonly System.Collections.Generic.Dictionary<int, object?> _inputs = new();
    private readonly System.Collections.Generic.Dictionary<int, object?> _outputs = new();

    public void SetInput<T>(int index, T value) => _inputs[index] = value;

    public T? GetInput<T>(int index)
    {
        if (_inputs.TryGetValue(index, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }

    public void SetOutput<T>(int index, T value) => _outputs[index] = value;

    public T? GetOutput<T>(int index)
    {
        if (_outputs.TryGetValue(index, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }
}
#endif
