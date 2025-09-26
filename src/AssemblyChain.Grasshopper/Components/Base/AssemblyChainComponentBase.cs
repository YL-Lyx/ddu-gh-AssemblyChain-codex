#if GRASSHOPPER
using Grasshopper.Kernel;
using IGhComponentBase = Grasshopper.Kernel.GH_Component;
using IGhDataAccessInternal = Grasshopper.Kernel.IGH_DataAccess;
#else
using AssemblyChain.Gh.Kernel.Legacy;
using IGhComponentBase = AssemblyChain.Gh.Kernel.Legacy.GhComponentBase;
using IGhDataAccessInternal = AssemblyChain.Gh.Kernel.Legacy.IGhDataAccess;
#endif

using AssemblyChain.Gh.Kernel.Legacy;

namespace AssemblyChain.Gh.Components.Legacy;

public abstract class AssemblyChainComponentBase : IGhComponentBase
{
    protected AssemblyChainComponentBase(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory)
    {
    }

#if GRASSHOPPER
    protected override void SolveInstance(IGH_DataAccess da)
    {
        var adapter = new GhDataAccessAdapter(da);
        Solve(adapter);
    }
#else
    public override void SolveInstance(IGhDataAccess dataAccess) => Solve(dataAccess);
#endif

    protected abstract void Solve(IGhDataAccess dataAccess);
}

#if GRASSHOPPER
internal sealed class GhDataAccessAdapter : IGhDataAccess
{
    private readonly IGhDataAccessInternal _inner;

    public GhDataAccessAdapter(IGH_DataAccess inner)
    {
        _inner = inner;
    }

    public void SetInput<T>(int index, T value) => throw new System.NotSupportedException();

    public T? GetInput<T>(int index)
    {
        if (_inner.GetData(index, out T value))
        {
            return value;
        }

        return default;
    }

    public void SetOutput<T>(int index, T value) => _inner.SetData(index, value);

    public T? GetOutput<T>(int index) => throw new System.NotSupportedException();
}
#endif
