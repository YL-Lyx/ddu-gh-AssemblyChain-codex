using System.Collections.Generic;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Contract exposing read-only access to assembly level information.
    /// </summary>
    public interface IModelQuery
    {
        /// <summary>Gets the human friendly assembly name.</summary>
        string Name { get; }

        /// <summary>Gets the hash identifying the assembly snapshot.</summary>
        string Hash { get; }

        /// <summary>Gets the number of parts contained in the assembly.</summary>
        int PartCount { get; }

        /// <summary>Gets the read-only list of parts participating in the assembly.</summary>
        IReadOnlyList<IPartGeometry> Parts { get; }
    }
}
