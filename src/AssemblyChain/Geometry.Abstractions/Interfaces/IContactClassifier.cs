using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Provides classification for contacts, grouping them into semantic categories.
/// </summary>
public interface IContactClassifier
{
    IReadOnlyDictionary<string, IReadOnlyCollection<IContact>> Classify(IEnumerable<IContact> contacts);
}
