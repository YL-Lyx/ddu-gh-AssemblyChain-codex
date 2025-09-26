using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;
using AssemblyChain.Geometry.ContactDetection;

namespace AssemblyChain.Constraints;

/// <summary>
/// Computes direction cones for a set of contacts. A direction cone is represented as the intersection of half-spaces pointing
/// away from each contact normal. The implementation uses a simple average normal per contact pair.
/// </summary>
public sealed class DirectionConeBuilder
{
    public IReadOnlyList<DirectionCone> BuildCones(Assembly assembly, IEnumerable<Contact> contacts)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(contacts);
        var grouped = contacts.GroupBy(c => (c.PartA, c.PartB));
        var cones = new List<DirectionCone>();
        foreach (var group in grouped)
        {
            var partA = assembly.PartLookup[group.Key.PartA];
            var partB = assembly.PartLookup[group.Key.PartB];
            var normal = (partB.CenterOfMass - partA.CenterOfMass).Normalize();
            if (normal.Length < 1e-6)
            {
                normal = new Vector3d(0, 0, 1);
            }

            cones.Add(new DirectionCone(group.Key.PartA, group.Key.PartB, normal, Math.PI / 4));
        }

        return cones;
    }
}

/// <summary>
/// Represents a cone of allowed motion for separating two parts.
/// </summary>
public sealed record DirectionCone(string PartA, string PartB, Vector3d Axis, double AngleRadians);

/// <summary>
/// Builds a conservative half-space intersection for a part by intersecting half-spaces derived from its incident cones.
/// </summary>
public sealed class HalfSpaceIntersectionBuilder
{
    public IReadOnlyList<HalfSpace> Build(Assembly assembly, IEnumerable<DirectionCone> cones, string partId)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(cones);
        ArgumentException.ThrowIfNullOrEmpty(partId);
        var part = assembly.PartLookup[partId];
        var halfSpaces = new List<HalfSpace>();
        foreach (var cone in cones.Where(c => c.PartA == partId || c.PartB == partId))
        {
            var axis = cone.PartA == partId ? cone.Axis : cone.Axis * -1;
            var offset = Vector3d.Dot(axis.Normalize(), new Vector3d(part.CenterOfMass.X, part.CenterOfMass.Y, part.CenterOfMass.Z));
            halfSpaces.Add(new HalfSpace(axis.Normalize(), offset));
        }

        if (halfSpaces.Count == 0)
        {
            // Default to allow all motion.
            halfSpaces.Add(new HalfSpace(new Vector3d(0, 0, 1), double.MaxValue));
        }

        return halfSpaces;
    }
}
