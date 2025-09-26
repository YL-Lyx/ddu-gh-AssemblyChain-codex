using System;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Contact;

namespace AssemblyChain.Core.Toolkit.Utils
{
    /// <summary>
    /// Contact detection helper functions
    /// </summary>
    public static class ContactDetectionHelpers
    {
        /// <summary>
        /// Checks if a contact is blocking based on contact type and friction coefficient
        /// </summary>
        public static bool IsContactBlocking(ContactType type, double friction)
        {
            return type switch
            {
                ContactType.Face => friction > 0.1,
                ContactType.Edge => friction > 0.5,
                ContactType.Point => friction > 0.8,
                _ => false
            };
        }
    }
}
