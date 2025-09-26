using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AssemblyChain.Core.Spatial;

namespace AssemblyChain.Core.DomainModel;

/// <summary>
/// Immutable description of a part used by the planning pipeline. The constructor normalises inputs to ensure collections are
/// always available for downstream modules.
/// </summary>
public sealed record Part
{
    public Part(
        string id,
        string name,
        double mass,
        Point3d centerOfMass,
        IReadOnlyList<GeometryPrimitive>? geometry = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (mass <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mass), "Mass must be positive.");
        }

        Id = id;
        Name = name;
        Mass = mass;
        CenterOfMass = centerOfMass;
        Geometry = geometry?.ToImmutableArray() ?? Array.Empty<GeometryPrimitive>();
        BoundingBox = Geometry.Count == 0
            ? new BoundingBox(centerOfMass, centerOfMass)
            : BoundingBox.FromPoints(Geometry.SelectMany(p => p.Vertices));
    }

    public string Id { get; }

    public string Name { get; }

    public double Mass { get; }

    public Point3d CenterOfMass { get; }

    public IReadOnlyList<GeometryPrimitive> Geometry { get; }

    public BoundingBox BoundingBox { get; }
}

/// <summary>
/// Simple immutable representation of a joint connecting two parts.
/// </summary>
public sealed record Joint
{
    public Joint(string id, string partA, string partB, string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(partA);
        ArgumentException.ThrowIfNullOrEmpty(partB);
        Id = id;
        PartA = partA;
        PartB = partB;
        Type = string.IsNullOrWhiteSpace(type) ? "unknown" : type;
    }

    public string Id { get; }

    public string PartA { get; }

    public string PartB { get; }

    public string Type { get; }
}

/// <summary>
/// Container describing an assembly with optional metadata and cached lookups.
/// </summary>
public sealed record Assembly
{
    public Assembly(
        string id,
        IReadOnlyList<Part> parts,
        IReadOnlyList<Joint> joints,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        Parts = parts?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(parts));
        Joints = joints?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(joints));
        Metadata = metadata ?? ImmutableDictionary<string, string>.Empty;
        Id = id;
        _partLookup = new Lazy<IReadOnlyDictionary<string, Part>>(() => Parts.ToImmutableDictionary(p => p.Id));
    }

    public string Id { get; }

    public IReadOnlyList<Part> Parts { get; }

    public IReadOnlyList<Joint> Joints { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    private readonly Lazy<IReadOnlyDictionary<string, Part>> _partLookup;

    public IReadOnlyDictionary<string, Part> PartLookup => _partLookup.Value;

    public IEnumerable<Joint> JointsFor(string partId)
        => Joints.Where(j => j.PartA == partId || j.PartB == partId);
}

/// <summary>
/// Immutable description of a single step in an assembly or disassembly plan.
/// </summary>
public sealed record PlanStep
{
    public PlanStep(int index, string action, string partId, Pose? pose = null)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(partId);
        Index = index;
        Action = action;
        PartId = partId;
        Pose = pose;
    }

    public int Index { get; }

    public string Action { get; }

    public string PartId { get; }

    public Pose? Pose { get; }
}

/// <summary>
/// Immutable description of a plan including metadata for stability and collision validation.
/// </summary>
public sealed record AssemblyPlan
{
    public AssemblyPlan(string name, IReadOnlyList<PlanStep> steps, bool isValid, IReadOnlyList<string>? diagnostics = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        Steps = steps?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(steps));
        IsValid = isValid;
        Diagnostics = diagnostics?.ToImmutableArray() ?? Array.Empty<string>();
    }

    public string Name { get; }

    public IReadOnlyList<PlanStep> Steps { get; }

    public bool IsValid { get; }

    public IReadOnlyList<string> Diagnostics { get; }
}

/// <summary>
/// Lightweight pose representation used by the robotics integration.
/// </summary>
public sealed record Pose(Point3d Origin, Vector3d XAxis, Vector3d YAxis, Vector3d ZAxis);

/// <summary>
/// Supported primitive types for contact detection.
/// </summary>
public enum GeometryPrimitiveType
{
    Point,
    Line,
    Face,
}

/// <summary>
/// Minimal primitive definition with vertices.
/// </summary>
public sealed record GeometryPrimitive
{
    public GeometryPrimitive(string id, GeometryPrimitiveType type, IReadOnlyList<Point3d> vertices)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        Id = id;
        Type = type;
        Vertices = vertices?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(vertices));
        if (Vertices.Count == 0)
        {
            throw new ArgumentException("A primitive requires at least one vertex.", nameof(vertices));
        }

        BoundingBox = BoundingBox.FromPoints(Vertices);
    }

    public string Id { get; }

    public GeometryPrimitiveType Type { get; }

    public IReadOnlyList<Point3d> Vertices { get; }

    public BoundingBox BoundingBox { get; }
}
