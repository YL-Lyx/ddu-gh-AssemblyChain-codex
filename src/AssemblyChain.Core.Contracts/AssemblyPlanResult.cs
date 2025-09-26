using System;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Result returned from the facade after executing the planning pipeline.
    /// </summary>
    public sealed class AssemblyPlanResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyPlanResult"/> class.
        /// </summary>
        /// <param name="contacts">Computed contact model.</param>
        /// <param name="solverResult">Sequence planning result.</param>
        public AssemblyPlanResult(IContactModel contacts, ISolverModel solverResult)
        {
            Contacts = contacts ?? throw new ArgumentNullException(nameof(contacts));
            SolverResult = solverResult ?? throw new ArgumentNullException(nameof(solverResult));
        }

        /// <summary>
        /// Gets the contact model produced during processing.
        /// </summary>
        public IContactModel Contacts { get; }

        /// <summary>
        /// Gets the solver result produced during processing.
        /// </summary>
        public ISolverModel SolverResult { get; }
    }
}
