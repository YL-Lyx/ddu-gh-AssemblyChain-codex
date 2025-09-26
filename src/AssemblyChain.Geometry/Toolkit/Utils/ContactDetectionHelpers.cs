using System;
using AssemblyChain.Planning.Model;
using AssemblyChain.IO.Contracts;

namespace AssemblyChain.Geometry.Toolkit.Utils
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
