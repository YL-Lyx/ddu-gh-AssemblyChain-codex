// 改造目的：统一 SAT 求解器模板，委派后端并输出可行性结果。
// 兼容性注意：提供旧类名兼容包装。
using AssemblyChain.Planning.Solver.Backends;

namespace AssemblyChain.Planning.Solver
{
    /// <summary>
    /// SAT solver wrapper.
    /// </summary>
    public sealed class SatSolver : BaseSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SatSolver"/> class.
        /// </summary>
        /// <param name="backend">Backend implementation.</param>
        public SatSolver(ISolverBackend? backend = null)
            : base("SAT", backend ?? new OrToolsBackend())
        {
        }
    }

    /// <summary>
    /// Legacy casing preserved for compatibility.
    /// </summary>
    public sealed class SATsolver : SatSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SATsolver"/> class.
        /// </summary>
        public SATsolver(ISolverBackend? backend = null) : base(backend)
        {
        }
    }
}
