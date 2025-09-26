// 改造目的：重构 CSP 求解器以复用 BaseSolver 模板，并委派给后端。
// 兼容性注意：类名保持，外部可继续通过 CSPSolver 调用求解。
using AssemblyChain.Planning.Solver.Backends;

namespace AssemblyChain.Planning.Solver
{
    /// <summary>
    /// Constraint satisfaction solver delegating work to the configured backend.
    /// </summary>
    public sealed class CspSolver : BaseSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CspSolver"/> class.
        /// </summary>
        /// <param name="backend">Backend implementation (defaults to OR-Tools stub).</param>
        public CspSolver(ISolverBackend? backend = null)
            : base("CSP", backend ?? new OrToolsBackend())
        {
        }
    }
    /// <summary>
    /// Legacy casing preserved for backward compatibility.
    /// </summary>
    public sealed class CSPsolver : CspSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSPsolver"/> class.
        /// </summary>
        public CSPsolver(ISolverBackend? backend = null) : base(backend)
        {
        }
    }
}
