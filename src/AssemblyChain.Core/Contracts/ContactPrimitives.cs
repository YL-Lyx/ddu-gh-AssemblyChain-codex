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
        public override string ToString() => $"Plane(Center: {Center}, Normal: {Normal})";
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
        /// <summary>
        /// Unique identifier for this contact.
        /// </summary>
        public string Id { get; init; } = $"Contact_{PartAId}_{PartBId}_{Guid.NewGuid():N}";

        /// <summary>
        /// Contact pair identifier (A-B format).
        /// </summary>
        public string PairId => $"{PartAId}-{PartBId}";

        /// <summary>
        /// Motion constraint vector (normal direction).
        /// </summary>
        public Vector3d ConstraintVector => Plane.Normal;

        /// <summary>
        /// Contact area associated with the zone.
        /// </summary>
        public double Area => Zone.Area;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{PairId}: {Type} (μ={FrictionCoefficient:F2}, e={RestitutionCoefficient:F2})";
        }
    }

    /// <summary>
    /// Represents a contact between two parts (legacy compatibility).
    /// </summary>
    public record ContactPair(string PartAId, string PartBId, ContactType Type, ContactZone Zone, ContactPlane Plane)
    {
        /// <summary>
        /// Unique identifier for this contact pair.
        /// </summary>
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
    /// Aggregated contact analysis result used by downstream modules.
    /// </summary>
    public record ContactAnalysisResult(
        List<ContactPair> Pairs,
        List<MotionConstraint> Constraints,
        Dictionary<string, List<string>> Neighbors);
}
