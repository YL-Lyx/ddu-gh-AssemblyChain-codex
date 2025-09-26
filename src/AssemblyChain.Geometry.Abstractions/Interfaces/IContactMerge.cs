using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// Describes a service that merges duplicate or overlapping contacts.
/// </summary>
public interface IContactMerge
{
    IEnumerable<IContact> Merge(IEnumerable<IContact> contacts, IToleranceProfile tolerance);
}
