using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AssemblyChain.Core.Domain.Entities;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.Utils
{
    /// <summary>
    /// Utilities for generating content hashes for caching and change detection.
    /// </summary>
    public static class Hashing
    {
        public static string ForAssembly(IReadOnlyList<Part> parts)
        {
            var sb = new StringBuilder();
            sb.Append("ASM|");
            foreach (var part in (parts ?? Array.Empty<Part>()).OrderBy(p => p.IndexId))
            {
                sb.Append($"{part.IndexId}:{part.Mesh?.Vertices.Count ?? 0}:{part.Physics?.Mass ?? 0:F6}|");
            }
            return ComputeHash(sb.ToString());
        }

        public static string ForContacts(string assemblyHash, object options)
        {
            var content = $"CONTACT|{assemblyHash}|{options?.GetHashCode() ?? 0}";
            return ComputeHash(content);
        }

        public static string ForGraphs(string contactHash, object options)
        {
            var content = $"GRAPH|{contactHash}|{options?.GetHashCode() ?? 0}";
            return ComputeHash(content);
        }

        public static string ForMotion(string contactHash, object options)
        {
            var content = $"MOTION|{contactHash}|{options?.GetHashCode() ?? 0}";
            return ComputeHash(content);
        }

        public static string ForConstraints(string graphHash, string motionHash)
        {
            var content = $"CONSTRAINT|{graphHash}|{motionHash}";
            return ComputeHash(content);
        }

        public static string ForSolver(string constraintHash, object options)
        {
            var content = $"SOLVER|{constraintHash}|{options?.GetHashCode() ?? 0}";
            return ComputeHash(content);
        }

        /// <summary>
        /// Generates a hash key for centroid coordinates with quantization.
        /// </summary>
        public static string ForCentroid(Point3d centroid, double tolerance)
        {
            var quant = System.Math.Max(tolerance * 0.1, 1e-6);
            var x = System.Math.Round(centroid.X / quant) * quant;
            var y = System.Math.Round(centroid.Y / quant) * quant;
            var z = System.Math.Round(centroid.Z / quant) * quant;
            return $"{x:F6},{y:F6},{z:F6}";
        }

        /// <summary>
        /// Generates a hash key for area values with quantization.
        /// </summary>
        public static string ForArea(double area, double tolerance)
        {
            var quant = System.Math.Max(tolerance * tolerance * 0.1, 1e-12);
            var roundedArea = System.Math.Round(area / quant) * quant;
            return $"{roundedArea:F12}";
        }

        /// <summary>
        /// Generates a hash key for plane parameters with quantization.
        /// </summary>
        public static string ForPlane(Plane plane, double tolerance)
        {
            var n = plane.Normal; n.Unitize();
            var d = plane.DistanceTo(Point3d.Origin);
            var quant = System.Math.Max(tolerance * 0.1, 1e-6);
            var nx = System.Math.Round(n.X / quant) * quant;
            var ny = System.Math.Round(n.Y / quant) * quant;
            var nz = System.Math.Round(n.Z / quant) * quant;
            var dd = System.Math.Round(d / quant) * quant;
            return $"{nx:F6},{ny:F6},{nz:F6},{dd:F6}";
        }

        private static string ComputeHash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            var hashBytes = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

