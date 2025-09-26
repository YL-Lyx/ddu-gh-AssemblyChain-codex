#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using AssemblyChain.Core.Contact;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhContactZones : GH_Component
    {
        public AcGhContactZones()
            : base("Contact Zones", "CZ", "Extract geometric zones and planes where parts contact from contact model", "AssemblyChain", "3|Solver")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcGhContactModelParam(), "ContactModel", "CM", "Contact model containing contact data", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Zones", "Zones", "Contact geometries grouped per part (DataTree: {i} -> all contacts of part i)", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("Planes", "Planes", "Contact planes grouped per part (DataTree: {i} -> all contacts of part i)", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                // Get contact model input
                AcGhContactModelGoo contactModelGoo = null!;
                if (!DA.GetData(0, ref contactModelGoo) || contactModelGoo?.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid contact model input.");
                    return;
                }

                var contactModel = contactModelGoo.Value;

                // 调试信息：统计各种类型的接触
                var faceContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Face).ToList();
                var edgeContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Edge).ToList();
                var pointContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Point).ToList();

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Total contacts: {contactModel.ContactCount} (Face: {faceContacts.Count}, Edge: {edgeContacts.Count}, Point: {pointContacts.Count})");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Neighbor pairs: {contactModel.UniquePairs}");

                // 详细调试：检查每个接触的数据
                foreach (var contact in contactModel.Contacts)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                        $"Contact {contact.Id}: {contact.PartAId}-{contact.PartBId}, Type={contact.Type}, Area={contact.Area:F6}, HasGeometry={contact.Zone.Geometry != null}");
                }

                if (contactModel.ContactCount > 0 && faceContacts.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only point/edge contacts were found; face contacts are required for zones output.");
                    // Still create empty trees for all parts
                }

                // Create DataTree structures for outputs
                var zonesTree = new GH_Structure<IGH_GeometricGoo>();
                var planesTree = new GH_Structure<GH_Plane>();

                // Get all unique part indices from contacts (not just neighbor map)
                var allPartIndices = new HashSet<int>();
                foreach (var contact in contactModel.Contacts)
                {
                    if (TryParsePartIndex(contact.PartAId, out int partAIndex))
                        allPartIndices.Add(partAIndex);
                    if (TryParsePartIndex(contact.PartBId, out int partBIndex))
                        allPartIndices.Add(partBIndex);
                }
                var sortedPartIndices = allPartIndices.OrderBy(i => i).ToList();

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Found part indices: {string.Join(", ", sortedPartIndices)}");

                // Ensure we have branches for every part, even if empty
                foreach (int partIndex in sortedPartIndices)
                {
                    var path = new GH_Path(partIndex);
                    zonesTree.EnsurePath(path);
                    planesTree.EnsurePath(path);
                }

                // Append each contact into both participants' branches {i} and {j}
                foreach (var contact in faceContacts)
                {
                    // Parse part indices from contact IDs
                    if (!TryParsePartIndex(contact.PartAId, out int partAIndex) ||
                        !TryParsePartIndex(contact.PartBId, out int partBIndex))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to parse part indices from {contact.PartAId} and {contact.PartBId}");
                        continue;
                    }

                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Processing face contact: {contact.PartAId}-{contact.PartBId}, Area={contact.Area:F6}, GeometryType={contact.Zone.Geometry?.GetType().Name ?? "null"}");

                    AppendFaceContactToPath(contact, new GH_Path(partAIndex), zonesTree, planesTree);
                    AppendFaceContactToPath(contact, new GH_Path(partBIndex), zonesTree, planesTree);
                }

                // Set outputs
                DA.SetDataTree(0, zonesTree);
                DA.SetDataTree(1, planesTree);

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Processed {sortedPartIndices.Count} parts with contact zones");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error extracting contact zones: {ex.Message}");
            }
        }

        private static void AppendFaceContactToPath(ContactData contact, GH_Path path,
            GH_Structure<IGH_GeometricGoo> zonesTree, GH_Structure<GH_Plane> planesTree)
        {
            // Only process face contacts with valid geometries
            if (contact.Type != ContactType.Face)
                return;

            System.Diagnostics.Debug.WriteLine($"AppendFaceContactToPath: Geometry type = {contact.Zone.Geometry?.GetType().Name ?? "null"}");

            if (contact.Zone.Geometry is Brep brep)
            {
                System.Diagnostics.Debug.WriteLine($"Appending Brep to path {path}: Valid={brep.IsValid}, Faces={brep.Faces.Count}");
                var geomGoo = new GH_Brep(brep);
                zonesTree.Append(geomGoo, path);
                planesTree.Append(new GH_Plane(contact.Plane.Plane), path);
            }
            else if (contact.Zone.Geometry is Mesh mesh)
            {
                System.Diagnostics.Debug.WriteLine($"Appending Mesh to path {path}: Valid={mesh.IsValid}, Vertices={mesh.Vertices.Count}, Faces={mesh.Faces.Count}");
                var geomGoo = new GH_Mesh(mesh);
                zonesTree.Append(geomGoo, path);
                planesTree.Append(new GH_Plane(contact.Plane.Plane), path);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unknown geometry type: {contact.Zone.Geometry?.GetType().Name ?? "null"}");
            }
        }

        private static bool TryParsePartIndex(string partId, out int index)
        {
            if (!string.IsNullOrEmpty(partId) && partId.StartsWith("P") && int.TryParse(partId.Substring(1), out index))
                return true;
            index = -1;
            return false;
        }

        public override Guid ComponentGuid => new Guid("c7d8e9f0-a1b2-c3d4-e5f6-a7b8c9d0e1f2");
    }
}
