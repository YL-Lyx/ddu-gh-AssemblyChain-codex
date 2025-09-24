using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AssemblyChain.Core.Domain.Entities;

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

        private static string ComputeHash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            var hashBytes = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

