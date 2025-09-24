using System;
using System.Collections.Generic;
using AssemblyChain.Core.Model;
using Rhino.Geometry;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Contact
{

/// <summary>
/// Contact detection for mesh geometries.
/// </summary>
public static class MeshDetector
{
    /// <summary>
    /// Detects contacts between mesh parts.
    /// </summary>
    public static ContactModel DetectContacts(
        AssemblyModel assembly,
        DetectionOptions options)
    {
        // Use existing contact detection infrastructure
        // This would integrate with the existing ContactAnalyzer
        var contacts = new List<ContactData>();
        var hash = $"mesh_{assembly.Hash}_{options.Tolerance}_{options.MinPatchArea}";

        return new ContactModel(contacts, hash);
    }

    /// <summary>
    /// Checks if a geometry is a mesh.
    /// </summary>
    public static bool IsMeshGeometry(GeometryBase geometry)
    {
        return geometry is Mesh;
    }

    /// <summary>
    /// Validates mesh quality for contact detection.
    /// </summary>
    public static bool ValidateMesh(Mesh mesh)
    {
        return mesh != null &&
               mesh.IsValid &&
               mesh.Vertices.Count > 0 &&
               mesh.Faces.Count > 0;
    }
}


}
