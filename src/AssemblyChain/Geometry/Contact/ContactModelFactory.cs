using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AssemblyChain.IO.Contracts;
using Rhino.Geometry;

namespace AssemblyChain.Geometry.Contact
{
    /// <summary>
    /// Factory helpers for creating <see cref="ContactModel"/> instances from raw contact data.
    /// </summary>
    public static class ContactModelFactory
    {
        /// <summary>
        /// Creates a <see cref="ContactModel"/> from an enumerable collection of contact records.
        /// </summary>
        /// <param name="contacts">The contact records to materialize.</param>
        /// <param name="hashHint">Optional hash override when determinism must align with external caches.</param>
        /// <returns>A read-only <see cref="ContactModel"/> instance.</returns>
        public static ContactModel FromContacts(IEnumerable<ContactData> contacts, string? hashHint = null)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            var sanitized = contacts
                .Where(contact => contact != null)
                .ToList();

            var hash = string.IsNullOrWhiteSpace(hashHint)
                ? ComputeHash(sanitized)
                : hashHint!;

            return new ContactModel(sanitized, hash);
        }

        private static string ComputeHash(IReadOnlyList<ContactData> contacts)
        {
            var builder = new StringBuilder();
            builder.Append("CONTACT|");

            foreach (var contact in contacts
                .OrderBy(c => c.PartAId)
                .ThenBy(c => c.PartBId)
                .ThenBy(c => c.Type))
            {
                var plane = contact.Plane ?? new ContactPlane(Plane.WorldXY, Vector3d.ZAxis, Point3d.Origin);

                builder.Append(contact.PartAId)
                    .Append(':')
                    .Append(contact.PartBId)
                    .Append(':')
                    .Append((int)contact.Type)
                    .Append(':')
                    .Append(contact.Zone?.Area.ToString("F6") ?? "0")
                    .Append(':')
                    .Append(contact.Zone?.Length.ToString("F6") ?? "0")
                    .Append(':')
                    .Append(contact.Zone?.Volume.ToString("F6") ?? "0")
                    .Append(':')
                    .Append(plane.Normal.X.ToString("F6"))
                    .Append(':')
                    .Append(plane.Normal.Y.ToString("F6"))
                    .Append(':')
                    .Append(plane.Normal.Z.ToString("F6"))
                    .Append(':')
                    .Append(plane.Center.X.ToString("F6"))
                    .Append(':')
                    .Append(plane.Center.Y.ToString("F6"))
                    .Append(':')
                    .Append(plane.Center.Z.ToString("F6"))
                    .Append(':')
                    .Append(contact.FrictionCoefficient.ToString("F6"))
                    .Append('|');
            }

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var hashBytes = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
