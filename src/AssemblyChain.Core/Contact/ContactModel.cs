using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.Core.Contracts;

namespace AssemblyChain.Core.Contact
{
    /// <summary>
    /// Read-only contact model.
    /// Contains contact data, neighbor map, and version hash for caching.
    /// </summary>
    public sealed class ContactModel
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

            // Pre-compute contact relations to avoid repeated conversions
            var relations = new List<ContactRelation>();
            var neighborMap = new Dictionary<int, HashSet<int>>();

            foreach (var contact in Contacts)
            {
                if (!TryParsePartIndex(contact.PartAId, out int partAIndex) ||
                    !TryParsePartIndex(contact.PartBId, out int partBIndex))
                {
                    continue;
                }

                // Build contact relations (pre-computed for efficiency)
                var relation = new ContactRelation(
                    partAIndex: partAIndex,
                    partBIndex: partBIndex,
                    normalVector: contact.ConstraintVector,
                    contactArea: contact.Area,
                    frictionCoefficient: contact.FrictionCoefficient
                );
                relations.Add(relation);

                // Build neighbor map
                if (!neighborMap.ContainsKey(partAIndex)) neighborMap[partAIndex] = new HashSet<int>();
                if (!neighborMap.ContainsKey(partBIndex)) neighborMap[partBIndex] = new HashSet<int>();

                neighborMap[partAIndex].Add(partBIndex);
                neighborMap[partBIndex].Add(partAIndex);
            }

            Relations = relations;
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

    /// <summary>
    /// Unified contact data containing all contact information
    /// </summary>
    public record ContactData(string PartAId, string PartBId, ContactType Type, ContactZone Zone, ContactPlane Plane,
        double FrictionCoefficient = 0.5, double RestitutionCoefficient = 0.1, bool IsBlocking = false)
    {
        /// <summary>
        /// Unique identifier for this contact
        /// </summary>
        public string Id { get; init; } = $"Contact_{PartAId}_{PartBId}_{Guid.NewGuid():N}";

        /// <summary>
        /// Contact pair identifier (A-B format)
        /// </summary>
        public string PairId => $"{PartAId}-{PartBId}";

        /// <summary>
        /// Motion constraint vector (normal direction)
        /// </summary>
        public Vector3d ConstraintVector => Plane.Normal;

        /// <summary>
        /// Contact area
        /// </summary>
        public double Area => Zone.Area;

        /// <summary>
        /// String representation for display
        /// </summary>
        public override string ToString()
        {
            return $"{PairId}: {Type} (μ={FrictionCoefficient:F2}, e={RestitutionCoefficient:F2})";
        }
    }

    /// <summary>
    /// Represents a contact between two parts (legacy compatibility)
    /// </summary>
    public record ContactPair(string PartAId, string PartBId, ContactType Type, ContactZone Zone, ContactPlane Plane)
    {
        /// <summary>
        /// Unique identifier for this contact pair
        /// </summary>
        public string Id { get; init; } = $"Contact_{PartAId}_{PartBId}_{Guid.NewGuid():N}";

        /// <summary>
        /// String representation for display
        /// </summary>
        public override string ToString()
        {
            return $"{PartAId}-{PartBId}: {Type}";
        }
    }

    /// <summary>
    /// Enumerates the supported contact primitive classification.
    /// </summary>
    public enum ContactType
    {
        Unknown = 0,
        Point,
        Edge,
        Face
    }

    /// <summary>
    /// Contact plane with normal and position information
    /// </summary>
    public record ContactPlane(Plane Plane, Vector3d Normal, Point3d Center)
    {
        /// <summary>
        /// String representation for display
        /// </summary>
        public override string ToString()
        {
            return $"Plane(Center: {Center}, Normal: {Normal})";
        }
    }

    /// <summary>
    /// Motion constraint information for a contact
    /// </summary>
    public record MotionConstraint(Vector3d ConstraintVector, double FrictionCoefficient, bool IsBlocking)
    {
        /// <summary>
        /// String representation for display
        /// </summary>
        public override string ToString()
        {
            return $"Constraint(Normal: {ConstraintVector:F3}, μ: {FrictionCoefficient:F2}, Blocking: {IsBlocking})";
        }
    }

    /// <summary>
    /// Contact analysis result
    /// </summary>
    public record ContactAnalysisResult(
        List<ContactPair> Pairs,
        List<MotionConstraint> Constraints,
        Dictionary<string, List<string>> Neighbors
    );

    /// <summary>
    /// Represents a contact relationship between two parts.
    /// Defines the geometric and physical constraints of their interaction.
    /// Used for graph algorithms and constraint analysis.
    /// </summary>
    public readonly struct ContactRelation
    {
        /// <summary>
        /// Index of the first part in the contact relationship.
        /// </summary>
        public int PartAIndex { get; }

        /// <summary>
        /// Index of the second part in the contact relationship.
        /// </summary>
        public int PartBIndex { get; }

        /// <summary>
        /// Normal vector from PartA to PartB at the contact point.
        /// </summary>
        public Vector3d NormalVector { get; }

        /// <summary>
        /// Contact area between the two parts.
        /// </summary>
        public double ContactArea { get; }

        /// <summary>
        /// Friction coefficient for this contact relationship.
        /// </summary>
        public double FrictionCoefficient { get; }

        public ContactRelation(int partAIndex, int partBIndex, Vector3d normalVector, double contactArea, double frictionCoefficient)
        {
            PartAIndex = partAIndex;
            PartBIndex = partBIndex;
            NormalVector = normalVector;
            ContactArea = contactArea;
            FrictionCoefficient = frictionCoefficient;
        }
    }
}



