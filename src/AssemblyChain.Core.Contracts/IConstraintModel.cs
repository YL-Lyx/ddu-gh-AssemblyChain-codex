namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Read-only view for constraint models used by solvers.
    /// </summary>
    public interface IConstraintModel
    {
        string Hash { get; }
    }
}
