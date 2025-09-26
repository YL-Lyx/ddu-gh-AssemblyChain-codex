using AssemblyChain.Core.Contracts;

namespace AssemblyChain.Core.Solver
{
    /// <summary>
    /// Options for sequence planning solvers.
    /// </summary>
    public readonly record struct SolverOptions(
        SolverType SolverType = SolverType.Auto,
        int TimeLimitMs = 2000,
        double MipGap = 0.0,
        bool UsePhysicsValidation = false,
        bool UseLearning = false,
        bool ExportArtifacts = false
    ) : ISolverOptions;
}
