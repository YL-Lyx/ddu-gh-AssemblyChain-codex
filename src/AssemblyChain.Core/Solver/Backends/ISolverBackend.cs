// 改造目的：抽象求解后端接口，支持多种 CSP/MILP/SAT 实现。
// 兼容性注意：保持求解请求结构简单，可由现有 Solver 包装调用。
using System.Collections.Generic;
using AssemblyChain.Core.Model;

namespace AssemblyChain.Core.Solver.Backends
{
    /// <summary>
    /// Enumerates the possible outcomes of a backend call.
    /// </summary>
    public enum SolverOutcome
    {
        /// <summary>At least one feasible solution was found.</summary>
        Feasible,
        /// <summary>No solution satisfies the constraints.</summary>
        Infeasible,
        /// <summary>The model is unsatisfiable due to conflicting constraints.</summary>
        Conflict,
        /// <summary>An unexpected backend error occurred.</summary>
        Error
    }

    /// <summary>
    /// Backend-agnostic request envelope describing the problem to solve.
    /// </summary>
    public sealed record SolverBackendRequest(
        string SolverId,
        AssemblyModel Assembly,
        Contact.ContactModel Contacts,
        ConstraintModel Constraints,
        SolverOptions Options);

    /// <summary>
    /// Backend response envelope.
    /// </summary>
    public sealed class SolverBackendResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolverBackendResult"/> class.
        /// </summary>
        public SolverBackendResult(
            SolverOutcome outcome,
            IReadOnlyList<Step> steps,
            IReadOnlyList<Rhino.Geometry.Vector3d> motions,
            IReadOnlyList<IReadOnlyList<int>> groups,
            string log,
            IReadOnlyDictionary<string, object> metadata)
        {
            Outcome = outcome;
            Steps = steps;
            Motions = motions;
            Groups = groups;
            Log = log;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the solver outcome classification.
        /// </summary>
        public SolverOutcome Outcome { get; }

        /// <summary>
        /// Gets the resulting step sequence.
        /// </summary>
        public IReadOnlyList<Step> Steps { get; }

        /// <summary>
        /// Gets the motion vectors per step.
        /// </summary>
        public IReadOnlyList<Rhino.Geometry.Vector3d> Motions { get; }

        /// <summary>
        /// Gets additional grouping information for batch operations.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<int>> Groups { get; }

        /// <summary>
        /// Gets textual log emitted by the backend.
        /// </summary>
        public string Log { get; }

        /// <summary>
        /// Gets backend-specific metadata useful for diagnostics.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }
    }

    /// <summary>
    /// Defines a pluggable solver backend.
    /// </summary>
    public interface ISolverBackend
    {
        /// <summary>
        /// Solves the provided model and returns a structured result.
        /// </summary>
        /// <param name="request">The problem specification.</param>
        /// <returns>A <see cref="SolverBackendResult"/> describing the backend response.</returns>
        SolverBackendResult Solve(SolverBackendRequest request);
    }
}
