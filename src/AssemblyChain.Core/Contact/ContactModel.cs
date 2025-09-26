using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Contracts;

namespace AssemblyChain.Core.Contact
{
    /// <summary>
    /// Read-only contact model holding detected contact information and derived relationships.
    /// </summary>
    public sealed class ContactModel : IContactModel
    {
        public IReadOnlyList<ContactData> Contacts { get; }
        public IReadOnlyList<ContactRelation> Relations { get; }
        public IReadOnlyDictionary<int, IReadOnlySet<int>> NeighborMap { get; }
        public string Hash { get; }
        public int ContactCount => Contacts.Count;
        public int UniquePairs => NeighborMap.Count;

        internal ContactModel(IReadOnlyList<ContactData> contacts, string hash)
        {
            Contacts = contacts ?? throw new ArgumentNullException(nameof(contacts));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));

            var relations = new List<ContactRelation>();
            var neighborMap = new Dictionary<int, HashSet<int>>();

            foreach (var contact in Contacts)
            {
                if (!TryParsePartIndex(contact.PartAId, out int partAIndex) ||
                    !TryParsePartIndex(contact.PartBId, out int partBIndex))
                {
                    continue;
                }

                var relation = new ContactRelation(
                    partAIndex: partAIndex,
                    partBIndex: partBIndex,
                    normalVector: contact.ConstraintVector,
                    contactArea: contact.Area,
                    frictionCoefficient: contact.FrictionCoefficient);
                relations.Add(relation);

                if (!neighborMap.ContainsKey(partAIndex)) neighborMap[partAIndex] = new HashSet<int>();
                if (!neighborMap.ContainsKey(partBIndex)) neighborMap[partBIndex] = new HashSet<int>();

                neighborMap[partAIndex].Add(partBIndex);
                neighborMap[partBIndex].Add(partAIndex);
            }

            Relations = relations;
            NeighborMap = neighborMap.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlySet<int>)kvp.Value);
        }

        private static bool TryParsePartIndex(string partId, out int index)
        {
            if (!string.IsNullOrEmpty(partId) && partId.StartsWith("P") && int.TryParse(partId[1..], out index))
            {
                return true;
            }

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
}
