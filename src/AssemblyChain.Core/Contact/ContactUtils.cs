// 改造目的：提供集中化的接触转换工具，降低窄相与模型耦合。
// 兼容性注意：保留 ContactData 结构，调用方无需调整数据消费逻辑。
using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Contracts;
using Rhino.Geometry;

namespace AssemblyChain.Core.Contact
{
    /// <summary>
    /// Default implementation for <see cref="IContactUtils"/>.
    /// </summary>
    public sealed class ContactUtils : IContactUtils
    {
        /// <inheritdoc />
        public IReadOnlyList<ContactData> CreateContacts(
            string partAId,
            string partBId,
            IEnumerable<ContactZone> zones,
            ContactType type)
        {
            if (string.IsNullOrWhiteSpace(partAId))
            {
                throw new ArgumentException("Part identifier cannot be empty.", nameof(partAId));
            }

            if (string.IsNullOrWhiteSpace(partBId))
            {
                throw new ArgumentException("Part identifier cannot be empty.", nameof(partBId));
            }

            zones ??= Enumerable.Empty<ContactZone>();
            return zones
                .Select(zone => new ContactData(partAId, partBId, type, zone, CreatePlane(zone)))
                .ToList();
        }

        private static ContactPlane CreatePlane(ContactZone zone)
        {
            if (zone.Geometry is null)
            {
                return new ContactPlane(Plane.WorldXY, Vector3d.ZAxis, Point3d.Origin);
            }

            Plane plane = Plane.WorldXY;
            Point3d center = Point3d.Origin;

            if (zone.Geometry is PlaneSurface surface && surface.FrameAt(0.5, 0.5, out var surfacePlane))
            {
                plane = surfacePlane;
                center = surfacePlane.Origin;
            }
            else if (zone.Geometry is BrepFace face && face.FrameAt(0.5, 0.5, out var facePlane))
            {
                plane = facePlane;
                center = facePlane.Origin;
            }
            else
            {
                var bbox = zone.Geometry.GetBoundingBox(true);
                center = bbox.IsValid ? bbox.Center : Point3d.Origin;
                plane = new Plane(center, Vector3d.ZAxis);
            }

            return new ContactPlane(plane, plane.Normal, center);
        }
    }
}
