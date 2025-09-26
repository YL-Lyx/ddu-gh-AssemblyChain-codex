using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Contact;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Processing
{
    /// <summary>
    /// Extracts face contact geometry grouped by part.
    /// </summary>
    public static class ContactZoneExtractor
    {
        public sealed class ContactZoneExtractionResult
        {
            public Dictionary<int, List<ContactFaceGeometry>> PartGeometries { get; } = new();
            public Dictionary<int, List<Plane>> PartPlanes { get; } = new();
            public List<ProcessingMessage> Messages { get; } = new();
            public IReadOnlyCollection<int> PartIndices => PartGeometries.Keys.OrderBy(i => i).ToList();
        }

        public sealed class ContactFaceGeometry
        {
            public ContactFaceGeometry(Brep? brep, Mesh? mesh, Plane plane)
            {
                Brep = brep;
                Mesh = mesh;
                Plane = plane;
            }

            public Brep? Brep { get; }
            public Mesh? Mesh { get; }
            public Plane Plane { get; }
        }

        public static ContactZoneExtractionResult ExtractFaceContacts(IEnumerable<ContactData> contacts)
        {
            if (contacts == null) throw new ArgumentNullException(nameof(contacts));

            var result = new ContactZoneExtractionResult();
            var contactList = contacts.ToList();

            var faceContacts = contactList.Where(c => c.Type == ContactType.Face).ToList();
            var edgeContacts = contactList.Count(c => c.Type == ContactType.Edge);
            var pointContacts = contactList.Count(c => c.Type == ContactType.Point);
            var neighborPairs = new HashSet<(int, int)>();

            result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark,
                $"Total contacts: {contactList.Count} (Face: {faceContacts.Count}, Edge: {edgeContacts}, Point: {pointContacts})"));

            if (faceContacts.Count == 0 && contactList.Count > 0)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                    "Only point/edge contacts were found; face contacts are required for zones output."));
                return result;
            }

            foreach (var contact in faceContacts)
            {
                if (!TryParsePartIndex(contact.PartAId, out var partA) ||
                    !TryParsePartIndex(contact.PartBId, out var partB))
                {
                    result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                        $"Failed to parse part indices from {contact.PartAId} and {contact.PartBId}"));
                    continue;
                }

                var pair = partA < partB ? (partA, partB) : (partB, partA);
                neighborPairs.Add(pair);

                AppendGeometry(result, partA, contact);
                AppendGeometry(result, partB, contact);
            }

            result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark,
                $"Neighbor pairs: {neighborPairs.Count}"));
            result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark,
                $"Processed {result.PartGeometries.Count} parts with contact zones"));

            return result;
        }

        private static void AppendGeometry(ContactZoneExtractionResult result, int partIndex, ContactData contact)
        {
            if (!result.PartGeometries.TryGetValue(partIndex, out var geometries))
            {
                geometries = new List<ContactFaceGeometry>();
                result.PartGeometries[partIndex] = geometries;
            }

            if (!result.PartPlanes.TryGetValue(partIndex, out var planes))
            {
                planes = new List<Plane>();
                result.PartPlanes[partIndex] = planes;
            }

            var geometry = contact.Zone.Geometry;
            Brep? brep = geometry as Brep;
            Mesh? mesh = geometry as Mesh;

            if (brep == null && mesh == null)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                    $"Unknown geometry type: {geometry?.GetType().Name ?? "null"}"));
                return;
            }

            geometries.Add(new ContactFaceGeometry(brep, mesh, contact.Plane.Plane));
            planes.Add(contact.Plane.Plane);
        }

        private static bool TryParsePartIndex(string partId, out int index)
        {
            if (!string.IsNullOrEmpty(partId) && partId.StartsWith("P") && int.TryParse(partId.Substring(1), out index))
            {
                return true;
            }

            index = -1;
            return false;
        }
    }
}
