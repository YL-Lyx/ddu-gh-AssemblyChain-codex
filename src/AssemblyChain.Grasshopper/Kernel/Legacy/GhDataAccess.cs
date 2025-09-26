using System.Collections.Generic;

namespace AssemblyChain.Gh.Kernel.Legacy;

/// <summary>
/// Simple in-memory implementation of <see cref="IGhDataAccess"/> used by unit tests
/// and command-line tooling. The implementation is always compiled so tests remain
/// available even when the real Grasshopper API is targeted.
/// </summary>
public sealed class GhDataAccess : IGhDataAccess
{
    private readonly Dictionary<int, object?> _inputs = new();
    private readonly Dictionary<int, object?> _outputs = new();

    public void SetInput<T>(int index, T value)
    {
        _inputs[index] = value;
    }

    public T? GetInput<T>(int index)
    {
        if (_inputs.TryGetValue(index, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }

    public void SetOutput<T>(int index, T value)
    {
        _outputs[index] = value;
    }

    public T? GetOutput<T>(int index)
    {
        if (_outputs.TryGetValue(index, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }
}
