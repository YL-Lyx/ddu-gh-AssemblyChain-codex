using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace AssemblyChain.Core.Contracts
{
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
    /// Contact plane with normal and position information.
    /// </summary>
    public record ContactPlane(Plane Plane, Vector3d Normal, Point3d Center)
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Plane(Center: {Center}, Normal: {Normal})";
        }
    }

    /// <summary>
    /// Unified contact data containing all contact information.
    /// </summary>
    public record ContactData(
        string PartAId,
        string PartBId,
        ContactType Type,
        ContactZone Zone,
        ContactPlane Plane,
        double FrictionCoefficient = 0.5,
        double RestitutionCoefficient = 0.1,
        bool IsBlocking = false)
    {
        /// <summary>Gets the unique identifier for the contact.</summary>
        public string Id { get; init; } = $"Contact_{PartAId}_{PartBId}_{Guid.NewGuid():N}";

        /// <summary>Gets the combined identifier for the contact pair.</summary>
        public string PairId => $"{PartAId}-{PartBId}";

        /// <summary>Gets the motion constraint vector (contact normal).</summary>
        public Vector3d ConstraintVector => Plane.Normal;

        /// <summary>Gets the resolved contact area.</summary>
        public double Area => Zone.Area;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{PairId}: {Type} (μ={FrictionCoefficient:F2}, e={RestitutionCoefficient:F2})";
        }
    }

    /// <summary>
    /// Represents a contact between two parts (legacy compatibility helper).
    /// </summary>
    public record ContactPair(string PartAId, string PartBId, ContactType Type, ContactZone Zone, ContactPlane Plane)
    {
        /// <summary>Gets the unique identifier for the pair.</summary>
        public string Id { get; init; } = $"Contact_{PartAId}_{PartBId}_{Guid.NewGuid():N}";

        /// <inheritdoc />
        public override string ToString() => $"{PartAId}-{PartBId}: {Type}";
    }

    /// <summary>
    /// Motion constraint information for a contact.
    /// </summary>
    public record MotionConstraint(Vector3d ConstraintVector, double FrictionCoefficient, bool IsBlocking)
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Constraint(Normal: {ConstraintVector:F3}, μ: {FrictionCoefficient:F2}, Blocking: {IsBlocking})";
        }
    }

    /// <summary>
    /// Contact analysis result aggregate.
    /// </summary>
    public record ContactAnalysisResult(
        List<ContactPair> Pairs,
        List<MotionConstraint> Constraints,
        Dictionary<string, List<string>> Neighbors
    );

    /// <summary>
    /// Represents a contact relationship between two parts for graph algorithms.
    /// </summary>
    public readonly struct ContactRelation
    {
        /// <summary>Initializes a new instance of the <see cref="ContactRelation"/> struct.</summary>
        public ContactRelation(int partAIndex, int partBIndex, Vector3d normalVector, double contactArea, double frictionCoefficient)
        {
            PartAIndex = partAIndex;
            PartBIndex = partBIndex;
            NormalVector = normalVector;
            ContactArea = contactArea;
            FrictionCoefficient = frictionCoefficient;
        }

        /// <summary>Gets the index of the first part.</summary>
        public int PartAIndex { get; }

        /// <summary>Gets the index of the second part.</summary>
        public int PartBIndex { get; }

        /// <summary>Gets the representative contact normal.</summary>
        public Vector3d NormalVector { get; }

        /// <summary>Gets the accumulated contact area.</summary>
        public double ContactArea { get; }

        /// <summary>Gets the representative friction coefficient.</summary>
        public double FrictionCoefficient { get; }
    }
}
