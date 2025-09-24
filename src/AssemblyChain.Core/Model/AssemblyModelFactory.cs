using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Toolkit.Utils;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Factory helpers for creating <see cref="AssemblyModel"/> instances from mutable domain assemblies.
    /// </summary>
    public static class AssemblyModelFactory
    {
        /// <summary>
        /// Creates a read-only <see cref="AssemblyModel"/> snapshot from a mutable <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly">The mutable assembly source.</param>
        /// <returns>A cached snapshot instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is null.</exception>
        public static AssemblyModel Create(Assembly assembly, AssemblyModel existing = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var parts = MaterializeParts(assembly);
            var hash = Hashing.ForAssembly(parts);
            if (existing != null && string.Equals(existing.Hash, hash, StringComparison.Ordinal))
            {
                return existing;
            }

            var readOnlyParts = new ReadOnlyCollection<Part>(parts);
            return new AssemblyModel(readOnlyParts, assembly.Name, hash);
        }

        private static List<Part> MaterializeParts(Assembly assembly)
        {
            return assembly
                .GetAllParts()
                .Where(p => p != null)
                .ToList();
        }
    }
}
