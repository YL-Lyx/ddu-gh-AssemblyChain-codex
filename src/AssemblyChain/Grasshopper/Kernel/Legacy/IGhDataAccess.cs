namespace AssemblyChain.Gh.Kernel.Legacy;

public interface IGhDataAccess
{
    void SetInput<T>(int index, T value);

    T? GetInput<T>(int index);

    void SetOutput<T>(int index, T value);

    T? GetOutput<T>(int index);
}
