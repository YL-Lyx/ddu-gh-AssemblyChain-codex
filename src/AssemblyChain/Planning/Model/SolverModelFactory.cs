using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyChain.Planning.Model
{
    /// <summary>
    /// Factory utilities for constructing <see cref="DgSolverModel"/> instances in tests and tooling.
    /// </summary>
    public static class SolverModelFactory
    {
        /// <summary>
        /// Creates a <see cref="DgSolverModel"/> from the provided components.
        /// </summary>
        /// <param name="steps">Ordered sequence steps.</param>
        /// <param name="vectors">Motion vectors associated with each step.</param>
        /// <param name="groups">Optional batching groups.</param>
        /// <param name="isFeasible">Whether the solution is feasible.</param>
        /// <param name="isOptimal">Whether the solution is optimal.</param>
        /// <param name="log">Diagnostic log output.</param>
        /// <param name="solveTimeSeconds">Reported solve time.</param>
        /// <param name="solverType">Solver identifier.</param>
        /// <param name="metadata">Additional metadata.</param>
        /// <param name="isAssemblySequence">Indicates if the model represents an assembly sequence.</param>
        /// <returns>A populated <see cref="DgSolverModel"/>.</returns>
        public static DgSolverModel Create(
            IEnumerable<Step> steps,
            IEnumerable<Rhino.Geometry.Vector3d> vectors,
            IEnumerable<IReadOnlyList<int>>? groups = null,
            bool isFeasible = true,
            bool isOptimal = true,
            string? log = null,
            double solveTimeSeconds = 0.0,
            string? solverType = null,
            IReadOnlyDictionary<string, object>? metadata = null,
            bool isAssemblySequence = false)
        {
            if (steps == null)
            {
                throw new ArgumentNullException(nameof(steps));
            }

            if (vectors == null)
            {
                throw new ArgumentNullException(nameof(vectors));
            }

            var stepList = steps.ToList();
            var vectorList = vectors.ToList();

            if (stepList.Count != vectorList.Count)
            {
                throw new ArgumentException("Steps and vectors must have the same number of elements.", nameof(vectors));
            }

            var groupList = groups != null
                ? groups.Select(g => (IReadOnlyList<int>)g.ToList()).ToList()
                : new List<IReadOnlyList<int>>();

            var metadataDictionary = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>();

            return new DgSolverModel(
                stepList,
                vectorList,
                groupList,
                isFeasible,
                isOptimal,
                log ?? string.Empty,
                solveTimeSeconds,
                solverType ?? "Unknown",
                metadataDictionary,
                isAssemblySequence);
        }
    }
}
