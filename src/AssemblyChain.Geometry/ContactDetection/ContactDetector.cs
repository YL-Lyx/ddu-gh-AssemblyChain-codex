using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;

namespace AssemblyChain.Geometry.ContactDetection;

/// <summary>
/// Computes contacts between geometry primitives using simple bounding volume hierarchy free checks. The detector supports point,
/// line and face primitives and classifies contacts by the smallest dimensional overlap detected.
/// </summary>
public sealed class ContactDetector
{
    private readonly double _tolerance;

    public ContactDetector(double tolerance = 1e-3)
    {
        _tolerance = tolerance;
    }

    public IReadOnlyList<Contact> DetectContacts(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        var contacts = new List<Contact>();
        for (int i = 0; i < assembly.Parts.Count; i++)
        {
            for (int j = i + 1; j < assembly.Parts.Count; j++)
            {
                CollectContacts(assembly.Parts[i], assembly.Parts[j], contacts);
            }
        }

        return contacts;
    }

    private void CollectContacts(Part a, Part b, List<Contact> contacts)
    {
        if (!a.BoundingBox.Overlaps(b.BoundingBox, _tolerance))
        {
            return;
        }

        foreach (var primitiveA in a.Geometry)
        {
            foreach (var primitiveB in b.Geometry)
            {
                var classification = ClassifyContact(primitiveA, primitiveB);
                if (classification is null)
                {
                    continue;
                }

                contacts.Add(new Contact(a.Id, b.Id, classification.Value, primitiveA.Id, primitiveB.Id));
            }
        }
    }

    private ContactType? ClassifyContact(GeometryPrimitive a, GeometryPrimitive b)
    {
        if (!a.BoundingBox.Overlaps(b.BoundingBox, _tolerance))
        {
            return null;
        }

        var minimumDistance = MinimumDistance(a.Vertices, b.Vertices);
        if (minimumDistance > _tolerance)
        {
            return null;
        }

        if (a.Type == GeometryPrimitiveType.Face && b.Type == GeometryPrimitiveType.Face)
        {
            return ContactType.Face;
        }

        if (a.Type == GeometryPrimitiveType.Line && b.Type is GeometryPrimitiveType.Face or GeometryPrimitiveType.Line)
        {
            return ContactType.Line;
        }

        return ContactType.Point;
    }

    private double MinimumDistance(IReadOnlyList<Point3d> a, IReadOnlyList<Point3d> b)
    {
        double min = double.MaxValue;
        foreach (var pa in a)
        {
            foreach (var pb in b)
            {
                var distance = (pa - pb).Length;
                if (distance < min)
                {
                    min = distance;
                }
            }
        }

        return min;
    }
}

/// <summary>
/// Contact classification between two parts.
/// </summary>
/// <param name="PartA">First part identifier.</param>
/// <param name="PartB">Second part identifier.</param>
/// <param name="Type">Contact dimensionality.</param>
/// <param name="PrimitiveA">Primitive on part A involved in the contact.</param>
/// <param name="PrimitiveB">Primitive on part B involved in the contact.</param>
public sealed record Contact(string PartA, string PartB, ContactType Type, string PrimitiveA, string PrimitiveB);

/// <summary>
/// Supported contact classifications.
/// </summary>
public enum ContactType
{
    Point,
    Line,
    Face,
}

