using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;

namespace AssemblyChain.IO;

/// <summary>
/// Handles reading and writing of assembly descriptions to the canonical JSON schema used by the sample fixtures.
/// </summary>
public static class AssemblySerializer
{
    public static Assembly LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    public static Assembly Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;
        var parts = new List<Part>();
        foreach (var element in root.GetProperty("parts").EnumerateArray())
        {
            parts.Add(ParsePart(element));
        }

        var joints = new List<Joint>();
        if (root.TryGetProperty("joints", out var jointsElement))
        {
            foreach (var element in jointsElement.EnumerateArray())
            {
                joints.Add(new Joint(
                    element.GetProperty("id").GetString()!,
                    element.GetProperty("partA").GetString()!,
                    element.GetProperty("partB").GetString()!,
                    element.GetProperty("type").GetString() ?? "unknown"));
            }
        }

        var metadata = new Dictionary<string, string>();
        if (root.TryGetProperty("metadata", out var metadataElement))
        {
            foreach (var property in metadataElement.EnumerateObject())
            {
                metadata[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return new Assembly(
            root.GetProperty("id").GetString() ?? "assembly",
            parts,
            joints,
            metadata);
    }

    public static void SaveToFile(string path, Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var stream = File.Create(path);
        Save(stream, assembly);
    }

    public static void Save(Stream stream, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(assembly);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("id", assembly.Id);
        writer.WritePropertyName("parts");
        writer.WriteStartArray();
        foreach (var part in assembly.Parts)
        {
            WritePart(writer, part);
        }

        writer.WriteEndArray();
        writer.WritePropertyName("joints");
        writer.WriteStartArray();
        foreach (var joint in assembly.Joints)
        {
            writer.WriteStartObject();
            writer.WriteString("id", joint.Id);
            writer.WriteString("partA", joint.PartA);
            writer.WriteString("partB", joint.PartB);
            writer.WriteString("type", joint.Type);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        if (assembly.Metadata is { Count: > 0 })
        {
            writer.WritePropertyName("metadata");
            writer.WriteStartObject();
            foreach (var pair in assembly.Metadata)
            {
                writer.WriteString(pair.Key, pair.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
        writer.Flush();
    }

    private static Part ParsePart(JsonElement element)
    {
        var id = element.GetProperty("id").GetString() ?? throw new InvalidDataException("Part id missing");
        var name = element.GetProperty("name").GetString() ?? id;
        var mass = element.TryGetProperty("mass", out var massElement) ? massElement.GetDouble() : 1.0;
        var comElement = element.GetProperty("centerOfMass");
        var centerOfMass = new Point3d(
            comElement.GetProperty("x").GetDouble(),
            comElement.GetProperty("y").GetDouble(),
            comElement.GetProperty("z").GetDouble());

        var primitives = new List<GeometryPrimitive>();
        if (element.TryGetProperty("geometry", out var geometryElement))
        {
            foreach (var primitiveElement in geometryElement.EnumerateArray())
            {
                primitives.Add(ParsePrimitive(primitiveElement));
            }
        }

        return new Part(id, name, mass, centerOfMass, primitives);
    }

    private static GeometryPrimitive ParsePrimitive(JsonElement element)
    {
        var id = element.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
        var type = Enum.Parse<GeometryPrimitiveType>(element.GetProperty("type").GetString()!, ignoreCase: true);
        var vertices = new List<Point3d>();
        foreach (var vertexElement in element.GetProperty("vertices").EnumerateArray())
        {
            vertices.Add(new Point3d(
                vertexElement.GetProperty("x").GetDouble(),
                vertexElement.GetProperty("y").GetDouble(),
                vertexElement.GetProperty("z").GetDouble()));
        }

        return new GeometryPrimitive(id, type, vertices);
    }

    private static void WritePart(Utf8JsonWriter writer, Part part)
    {
        writer.WriteStartObject();
        writer.WriteString("id", part.Id);
        writer.WriteString("name", part.Name);
        writer.WriteNumber("mass", part.Mass);
        writer.WritePropertyName("centerOfMass");
        writer.WriteStartObject();
        writer.WriteNumber("x", part.CenterOfMass.X);
        writer.WriteNumber("y", part.CenterOfMass.Y);
        writer.WriteNumber("z", part.CenterOfMass.Z);
        writer.WriteEndObject();

        if (part.Geometry?.Count > 0)
        {
            writer.WritePropertyName("geometry");
            writer.WriteStartArray();
            foreach (var primitive in part.Geometry)
            {
                writer.WriteStartObject();
                writer.WriteString("id", primitive.Id);
                writer.WriteString("type", primitive.Type.ToString().ToLowerInvariant());
                writer.WritePropertyName("vertices");
                writer.WriteStartArray();
                foreach (var vertex in primitive.Vertices)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("x", vertex.X);
                    writer.WriteNumber("y", vertex.Y);
                    writer.WriteNumber("z", vertex.Z);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Serializer for plan.json files.
/// </summary>
public static class PlanSerializer
{
    public static AssemblyPlan LoadFromFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    public static AssemblyPlan Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;
        var name = root.GetProperty("name").GetString() ?? "plan";
        var steps = new List<PlanStep>();
        foreach (var stepElement in root.GetProperty("steps").EnumerateArray())
        {
            steps.Add(new PlanStep(
                stepElement.GetProperty("index").GetInt32(),
                stepElement.GetProperty("action").GetString() ?? "",
                stepElement.GetProperty("partId").GetString() ?? "",
                ParsePose(stepElement)));
        }

        var diagnostics = new List<string>();
        if (root.TryGetProperty("diagnostics", out var diagnosticsElement))
        {
            diagnostics.AddRange(diagnosticsElement.EnumerateArray().Select(e => e.GetString() ?? string.Empty));
        }

        var isValid = root.TryGetProperty("isValid", out var validElement) && validElement.GetBoolean();
        return new AssemblyPlan(name, steps, isValid, diagnostics);
    }

    public static void SaveToFile(string path, AssemblyPlan plan)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var stream = File.Create(path);
        Save(stream, plan);
    }

    public static void Save(Stream stream, AssemblyPlan plan)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(plan);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("name", plan.Name);
        writer.WriteBoolean("isValid", plan.IsValid);
        writer.WritePropertyName("steps");
        writer.WriteStartArray();
        foreach (var step in plan.Steps)
        {
            writer.WriteStartObject();
            writer.WriteNumber("index", step.Index);
            writer.WriteString("action", step.Action);
            writer.WriteString("partId", step.PartId);
            if (step.Pose is { } pose)
            {
                writer.WritePropertyName("pose");
                writer.WriteStartObject();
                WriteVector(writer, "origin", pose.Origin);
                WriteVector(writer, "xAxis", pose.XAxis);
                WriteVector(writer, "yAxis", pose.YAxis);
                WriteVector(writer, "zAxis", pose.ZAxis);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        if (plan.Diagnostics.Count > 0)
        {
            writer.WritePropertyName("diagnostics");
            writer.WriteStartArray();
            foreach (var message in plan.Diagnostics)
            {
                writer.WriteStringValue(message);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
        writer.Flush();
    }

    private static Pose? ParsePose(JsonElement element)
    {
        if (!element.TryGetProperty("pose", out var poseElement))
        {
            return null;
        }

        return new Pose(
            ReadPoint(poseElement.GetProperty("origin")),
            ReadVector(poseElement.GetProperty("xAxis")),
            ReadVector(poseElement.GetProperty("yAxis")),
            ReadVector(poseElement.GetProperty("zAxis")));
    }

    private static Point3d ReadPoint(JsonElement element)
        => new(
            element.GetProperty("x").GetDouble(),
            element.GetProperty("y").GetDouble(),
            element.GetProperty("z").GetDouble());

    private static Vector3d ReadVector(JsonElement element)
        => new(
            element.GetProperty("x").GetDouble(),
            element.GetProperty("y").GetDouble(),
            element.GetProperty("z").GetDouble());

    private static void WriteVector(Utf8JsonWriter writer, string name, Point3d point)
    {
        writer.WritePropertyName(name);
        writer.WriteStartObject();
        writer.WriteNumber("x", point.X);
        writer.WriteNumber("y", point.Y);
        writer.WriteNumber("z", point.Z);
        writer.WriteEndObject();
    }

    private static void WriteVector(Utf8JsonWriter writer, string name, Vector3d vector)
    {
        writer.WritePropertyName(name);
        writer.WriteStartObject();
        writer.WriteNumber("x", vector.X);
        writer.WriteNumber("y", vector.Y);
        writer.WriteNumber("z", vector.Z);
        writer.WriteEndObject();
    }
}
