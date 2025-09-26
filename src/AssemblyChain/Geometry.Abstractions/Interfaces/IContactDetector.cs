using System.Collections.Generic;

namespace AssemblyChain.Geometry.Abstractions.Interfaces;

/// <summary>
/// High level service abstraction that detects contacts between two meshes.
/// </summary>
public interface IContactDetector
{
    IEnumerable<IContact> DetectContacts(IMeshLite meshA, IMeshLite meshB, IToleranceProfile tolerance);
}
