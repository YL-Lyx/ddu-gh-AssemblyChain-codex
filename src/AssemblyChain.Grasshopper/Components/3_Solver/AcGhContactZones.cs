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

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            try
            {
                var context = BuildExtractionContext(dataAccess);
                if (context == null)
                {
                    return;
                }

                EnsureBranches(context);
                AppendFaceContacts(context);
                PublishResults(dataAccess, context);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error extracting contact zones: {ex.Message}");
            }
        }

        private ContactZoneExtractionContext BuildExtractionContext(IGH_DataAccess dataAccess)
        {
            if (!TryGetContactModel(dataAccess, out var contactModel))
            {
                return null;
            }

            var faceContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Face).ToList();
            var edgeContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Edge).ToList();
            var pointContacts = contactModel.Contacts.Where(c => c.Type == ContactType.Point).ToList();

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                $"Total contacts: {contactModel.ContactCount} (Face: {faceContacts.Count}, Edge: {edgeContacts.Count}, Point: {pointContacts.Count})");
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                $"Neighbor pairs: {contactModel.UniquePairs}");

            ReportIndividualContacts(contactModel);

            if (contactModel.ContactCount > 0 && faceContacts.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only point/edge contacts were found; face contacts are required for zones output.");
            }

            var partIndices = CollectPartIndices(contactModel);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Found part indices: {string.Join(", ", partIndices)}");

            return new ContactZoneExtractionContext(contactModel, faceContacts, partIndices);
        }

        private bool TryGetContactModel(IGH_DataAccess dataAccess, out ContactModel contactModel)
        {
            contactModel = null;
            var contactModelGoo = new AcGhContactModelGoo();
            if (!dataAccess.GetData(0, ref contactModelGoo) || contactModelGoo?.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid contact model input.");
                return false;
            }

            contactModel = contactModelGoo.Value;
            return true;
        }

        private void ReportIndividualContacts(ContactModel contactModel)
        {
            foreach (var contact in contactModel.Contacts)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Contact {contact.Id}: {contact.PartAId}-{contact.PartBId}, Type={contact.Type}, Area={contact.Area:F6}, HasGeometry={contact.Zone.Geometry != null}");
            }
        }

        private static List<int> CollectPartIndices(ContactModel contactModel)
        {
            var indices = new HashSet<int>();
            foreach (var contact in contactModel.Contacts)
            {
                if (TryParsePartIndex(contact.PartAId, out int partAIndex))
                {
                    indices.Add(partAIndex);
                }
                if (TryParsePartIndex(contact.PartBId, out int partBIndex))
                {
                    indices.Add(partBIndex);
                }
            }

            return indices.OrderBy(i => i).ToList();
        }

        private void EnsureBranches(ContactZoneExtractionContext context)
        {
            foreach (int partIndex in context.PartIndices)
            {
                var path = new GH_Path(partIndex);
                context.ZonesTree.EnsurePath(path);
                context.PlanesTree.EnsurePath(path);
            }
        }

        private void AppendFaceContacts(ContactZoneExtractionContext context)
        {
            foreach (var contact in context.FaceContacts)
            {
                if (!TryParsePartIndex(contact.PartAId, out int partAIndex) ||
                    !TryParsePartIndex(contact.PartBId, out int partBIndex))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to parse part indices from {contact.PartAId} and {contact.PartBId}");
                    continue;
                }

                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Processing face contact: {contact.PartAId}-{contact.PartBId}, Area={contact.Area:F6}, GeometryType={contact.Zone.Geometry?.GetType().Name ?? "null"}");

                AppendFaceContactToPath(contact, new GH_Path(partAIndex), context.ZonesTree, context.PlanesTree);
                AppendFaceContactToPath(contact, new GH_Path(partBIndex), context.ZonesTree, context.PlanesTree);
            }
        }

        private void PublishResults(IGH_DataAccess dataAccess, ContactZoneExtractionContext context)
        {
            dataAccess.SetDataTree(0, context.ZonesTree);
            dataAccess.SetDataTree(1, context.PlanesTree);

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                $"Processed {context.PartIndices.Count} parts with contact zones");
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

        private sealed class ContactZoneExtractionContext
        {
            public ContactZoneExtractionContext(ContactModel contactModel, List<ContactData> faceContacts, List<int> partIndices)
            {
                ContactModel = contactModel;
                FaceContacts = faceContacts;
                PartIndices = partIndices;
            }

            public ContactModel ContactModel { get; }

            public List<ContactData> FaceContacts { get; }

            public List<int> PartIndices { get; }

            public GH_Structure<IGH_GeometricGoo> ZonesTree { get; } = new();

            public GH_Structure<GH_Plane> PlanesTree { get; } = new();
        }

        public override Guid ComponentGuid => new Guid("c7d8e9f0-a1b2-c3d4-e5f6-a7b8c9d0e1f2");
    }
}
