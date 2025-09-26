using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Helper factory providing convenience constructors for <see cref="ConstraintModel"/>.
    /// </summary>
    public static class ConstraintModelFactory
    {
        /// <summary>
        /// Creates a constraint model with no blocking information, useful for early experimentation.
        /// </summary>
        /// <param name="assembly">Assembly snapshot.</param>
        /// <returns>Empty constraint model.</returns>
        public static ConstraintModel CreateEmpty(AssemblyModel assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var graph = new GraphModel(
                new BlockingGraph(),
                new NonDirectionalBlockingGraph(),
                new Dictionary<int, int>(),
                new List<StronglyConnectedComponent>(),
                new List<BlockingEdge>(),
                $"graph_empty_{assembly.Hash}");

            var partRays = assembly.Parts.ToDictionary(
                part => part.IndexId,
                _ => (IReadOnlyList<Vector3d>)Array.Empty<Vector3d>());

            var motion = new MotionModel(
                partRays,
                new Dictionary<string, IReadOnlyList<Vector3d>>(),
                $"motion_empty_{assembly.Hash}");

            return new ConstraintModel(
                graph,
                motion,
                new Dictionary<int, IReadOnlyList<string>>(),
                new Dictionary<string, IReadOnlyList<string>>(),
                $"constraint_empty_{assembly.Hash}");
        }
    }
}
