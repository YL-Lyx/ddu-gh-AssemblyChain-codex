namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Abstraction over solver configuration settings.
    /// </summary>
    public interface ISolverOptions
    {
        SolverType SolverType { get; }
        int TimeLimitMs { get; }
        double MipGap { get; }
        bool UsePhysicsValidation { get; }
        bool UseLearning { get; }
        bool ExportArtifacts { get; }
    }
}
