using System.Collections.Generic;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Read-only view of solver outputs.
    /// </summary>
    public interface ISolverModel
    {
        string GetSummary();
        IReadOnlyDictionary<string, object> Metadata { get; }
    }
}
