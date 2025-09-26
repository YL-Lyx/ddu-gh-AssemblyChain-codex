using System;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Represents a request to run the assembly planning pipeline.
    /// </summary>
    public sealed class AssemblyPlanRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyPlanRequest"/> class.
        /// </summary>
        /// <param name="assembly">Immutable assembly snapshot.</param>
        /// <param name="contacts">Optional pre-computed contact model.</param>
        /// <param name="constraints">Optional constraint model.</param>
        /// <param name="detection">Detection options used when <paramref name="contacts"/> is not provided.</param>
        /// <param name="solver">Solver options configuring the backend.</param>
        public AssemblyPlanRequest(
            IModelQuery assembly,
            IContactModel? contacts,
            IConstraintModel? constraints,
            DetectionOptions? detection,
            ISolverOptions solver)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Contacts = contacts;
            Constraints = constraints;
            Detection = detection;
            Solver = solver;
        }

        /// <summary>
        /// Gets the immutable assembly snapshot.
        /// </summary>
        public IModelQuery Assembly { get; }

        /// <summary>
        /// Gets the pre-computed contact model when available.
        /// </summary>
        public IContactModel? Contacts { get; }

        /// <summary>
        /// Gets the constraint model if provided.
        /// </summary>
        public IConstraintModel? Constraints { get; }

        /// <summary>
        /// Gets the detection options used for generating contacts when they are not supplied.
        /// </summary>
        public DetectionOptions? Detection { get; }

        /// <summary>
        /// Gets the solver options controlling the backend.
        /// </summary>
        public ISolverOptions Solver { get; }
    }
}
