using System.Collections.Generic;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Contract describing the core contact detection capabilities.
    /// </summary>
    public interface IContactDetector
    {
        /// <summary>Runs full assembly contact detection.</summary>
        IContactModel DetectContacts(IModelQuery assembly, DetectionOptions options);

        /// <summary>Detects contacts for a pre-filtered part list.</summary>
        IReadOnlyList<ContactData> DetectContacts(IReadOnlyList<IPartGeometry> parts, DetectionOptions options);

        /// <summary>Detects contacts for a specific pair of parts.</summary>
        IReadOnlyList<ContactData> DetectContactsForPair(IPartGeometry partA, IPartGeometry partB, DetectionOptions options);
    }
}
