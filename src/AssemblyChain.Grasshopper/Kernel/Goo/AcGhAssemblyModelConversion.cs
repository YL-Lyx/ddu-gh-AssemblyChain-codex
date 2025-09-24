using System;
using System.Runtime.CompilerServices;
using AssemblyChain.Core.Model;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Helper for converting assembly goo into cached <see cref="AssemblyModel"/> snapshots.
    /// </summary>
    internal static class AcGhAssemblyModelConversion
    {
        private sealed class CacheEntry
        {
            public AssemblyModel Snapshot;
        }

        private static readonly ConditionalWeakTable<AcGhAssemblyGoo, CacheEntry> Cache = new();

        public static bool TryGetSnapshot(AcGhAssemblyGoo assemblyGoo, out AssemblyModel snapshot, out string error)
        {
            snapshot = null;
            error = string.Empty;

            if (assemblyGoo == null)
            {
                error = "Assembly goo was not provided.";
                return false;
            }

            var assembly = assemblyGoo.Value;
            if (assembly == null)
            {
                error = "Assembly data is null.";
                return false;
            }

            try
            {
                var entry = Cache.GetValue(assemblyGoo, _ => new CacheEntry());
                snapshot = AssemblyModelFactory.Create(assembly, entry.Snapshot);
                entry.Snapshot = snapshot;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
