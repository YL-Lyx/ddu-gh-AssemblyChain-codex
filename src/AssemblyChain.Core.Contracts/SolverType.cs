namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Solver types supported by the planning pipeline.
    /// </summary>
    public enum SolverType
    {
        Heuristic,
        CSP,
        SAT,
        MILP,
        Auto
    }
}
