// 改造目的：抽象求解模板方法，统一计时和失败处理逻辑。
// 兼容性注意：旧 Solver 调用方式保持一致，仅需传入新的 SolverOptions。
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AssemblyChain.Planning.Model;
using AssemblyChain.Planning.Solver.Backends;
using Rhino.Geometry;

namespace AssemblyChain.Planning.Solver
{
    /// <summary>
    /// Shared interface for all solvers exposed through the facade.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Solves the provided assembly planning problem.
        /// </summary>
        DgSolverModel Solve(
            AssemblyModel assembly,
            Contact.ContactModel contacts,
            ConstraintModel constraints,
            SolverOptions options = default);
    }

    /// <summary>
    /// Base class implementing the template method for solver execution.
    /// </summary>
    public abstract class BaseSolver : ISolver
    {
        private readonly ISolverBackend _backend;
        private readonly string _solverId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSolver"/> class.
        /// </summary>
        protected BaseSolver(string solverId, ISolverBackend backend)
        {
            _solverId = solverId ?? throw new ArgumentNullException(nameof(solverId));
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        /// <inheritdoc />
        public DgSolverModel Solve(
            AssemblyModel assembly,
            Contact.ContactModel contacts,
            ConstraintModel constraints,
            SolverOptions options = default)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));
            if (contacts is null) throw new ArgumentNullException(nameof(contacts));
            if (constraints is null) throw new ArgumentNullException(nameof(constraints));

            var solverOptions = options;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var request = new SolverBackendRequest(
                    _solverId,
                    assembly,
                    contacts,
                    constraints,
                    solverOptions);

                var backendResult = SolveCore(request);
                stopwatch.Stop();
                return MapToModel(backendResult, stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return CreateFailureModel(ex, stopwatch.Elapsed.TotalSeconds);
            }
        }

        /// <summary>
        /// Delegates solving to the backend.
        /// </summary>
        /// <param name="request">Solver request envelope.</param>
        /// <returns>The backend result.</returns>
        protected virtual SolverBackendResult SolveCore(SolverBackendRequest request)
        {
            return _backend.Solve(request);
        }

        private DgSolverModel MapToModel(SolverBackendResult backendResult, double solveTime)
        {
            var steps = backendResult.Steps ?? new List<Step>();
            var motions = backendResult.Motions ?? new List<Vector3d>();
            var groups = backendResult.Groups ?? new List<IReadOnlyList<int>>();
            var metadata = backendResult.Metadata != null
                ? new Dictionary<string, object>(backendResult.Metadata)
                : new Dictionary<string, object>();

            var (feasible, optimal) = backendResult.Outcome switch
            {
                SolverOutcome.Feasible => (true, true),
                SolverOutcome.Infeasible => (false, false),
                SolverOutcome.Conflict => (false, false),
                _ => (false, false)
            };

            metadata["outcome"] = backendResult.Outcome.ToString();

            return new DgSolverModel(
                steps,
                motions,
                groups,
                feasible,
                optimal,
                backendResult.Log ?? string.Empty,
                solveTime,
                _solverId,
                metadata);
        }

        private DgSolverModel CreateFailureModel(Exception exception, double solveTime)
        {
            var metadata = new Dictionary<string, object>
            {
                ["outcome"] = SolverOutcome.Error.ToString(),
                ["exception"] = exception.GetType().Name,
                ["message"] = exception.Message
            };

            return new DgSolverModel(
                new List<Step>(),
                new List<Vector3d>(),
                new List<IReadOnlyList<int>>(),
                isFeasible: false,
                isOptimal: false,
                log: $"{_solverId} failure: {exception.Message}",
                solveTimeSeconds: solveTime,
                solverType: _solverId,
                metadata: metadata);
        }
    }
}
