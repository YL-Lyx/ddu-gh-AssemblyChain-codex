using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Read-only contact model.
    /// Contains contact data, neighbor map, and version hash for caching.
    /// </summary>
    public sealed class ContactModel
    {
        public IReadOnlyList<ContactData> Contacts { get; }
        public IReadOnlyDictionary<int, IReadOnlySet<int>> NeighborMap { get; }
        public string Hash { get; }
        public int ContactCount => Contacts.Count;
        public int UniquePairs => NeighborMap.Count;

        internal ContactModel(IReadOnlyList<ContactData> contacts, string hash)
        {
            Contacts = contacts ?? throw new ArgumentNullException(nameof(contacts));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));

            var neighborMap = new Dictionary<int, HashSet<int>>();
            foreach (var contact in Contacts)
            {
                if (!TryParsePartIndex(contact.PartAId, out int partAIndex) ||
                    !TryParsePartIndex(contact.PartBId, out int partBIndex))
                {
                    continue;
                }

                if (!neighborMap.ContainsKey(partAIndex)) neighborMap[partAIndex] = new HashSet<int>();
                if (!neighborMap.ContainsKey(partBIndex)) neighborMap[partBIndex] = new HashSet<int>();

                neighborMap[partAIndex].Add(partBIndex);
                neighborMap[partBIndex].Add(partAIndex);
            }

            NeighborMap = neighborMap.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlySet<int>)kvp.Value
            );
        }

        private static bool TryParsePartIndex(string partId, out int index)
        {
            if (!string.IsNullOrEmpty(partId) && partId.StartsWith("P") && int.TryParse(partId.Substring(1), out index))
                return true;
            index = -1;
            return false;
        }

        public IEnumerable<ContactData> GetContactsForPart(int partIndex)
        {
            string partId = $"P{partIndex:D4}";
            return Contacts.Where(c => c.PartAId == partId || c.PartBId == partId);
        }

        public IEnumerable<ContactData> GetContactsBetweenParts(int partAIndex, int partBIndex)
        {
            string partAId = $"P{partAIndex:D4}";
            string partBId = $"P{partBIndex:D4}";
            return Contacts.Where(c =>
                (c.PartAId == partAId && c.PartBId == partBId) ||
                (c.PartAId == partBId && c.PartBId == partAId));
        }
    }

    // Minimal placeholder for ContactData; replace with actual type if available
    public sealed class ContactData
    {
        public string PartAId { get; init; }
        public string PartBId { get; init; }
        public Vector3d ConstraintVector { get; init; }
        public double Area { get; init; }
        public double FrictionCoefficient { get; init; }
    }
}



