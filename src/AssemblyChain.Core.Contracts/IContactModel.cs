using System.Collections.Generic;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Read-only view over computed contacts.
    /// </summary>
    public interface IContactModel
    {
        IReadOnlyList<ContactData> Contacts { get; }
        IReadOnlyList<ContactRelation> Relations { get; }
        IReadOnlyDictionary<int, IReadOnlySet<int>> NeighborMap { get; }
        string Hash { get; }
        int ContactCount { get; }
    }
}
