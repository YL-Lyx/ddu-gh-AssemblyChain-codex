using System.Collections.Generic;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Toolkit.Processing;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Toolkit.Processing
{
    public class ContactZoneExtractorTests
    {
        [Fact]
        public void ExtractFaceContacts_GroupsByPart()
        {
            var contacts = new List<ContactData>
            {
                CreateFaceContact("P0001", "P0002", Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(0, 1), new Interval(0, 1), new Interval(0, 1)), 1, 1, 1))
            };

            var result = ContactZoneExtractor.ExtractFaceContacts(contacts);

            Assert.NotEmpty(result.PartIndices);
            Assert.Equal(2, result.PartGeometries.Count);
            Assert.Contains(result.Messages, m => m.Level == ProcessingMessageLevel.Remark);
        }

        [Fact]
        public void ExtractFaceContacts_WithoutFaceGeometry_AddsWarning()
        {
            var contacts = new List<ContactData>
            {
                new ContactData("P0001", "P0002", ContactType.Edge, new ContactZone(null, 0.1, 0), new ContactPlane(Plane.WorldXY, Vector3d.ZAxis, Point3d.Origin))
            };

            var result = ContactZoneExtractor.ExtractFaceContacts(contacts);

            Assert.Empty(result.PartGeometries);
            Assert.Contains(result.Messages, m => m.Level == ProcessingMessageLevel.Warning);
        }

        private static ContactData CreateFaceContact(string partA, string partB, Mesh mesh)
        {
            var zone = new ContactZone(mesh, 1.0, 0.0, 0.0);
            var plane = new ContactPlane(Plane.WorldXY, Vector3d.ZAxis, Point3d.Origin);
            return new ContactData(partA, partB, ContactType.Face, zone, plane);
        }
    }
}
