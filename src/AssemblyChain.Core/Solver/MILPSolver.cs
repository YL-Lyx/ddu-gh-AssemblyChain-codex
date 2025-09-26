// 改造目的：重构 MILP 求解器为统一模板，输出后端结果。
// 兼容性注意：保留旧类型名称的包装以避免上层破坏。
using AssemblyChain.Core.Solver.Backends;

namespace AssemblyChain.Core.Solver
{
    /// <summary>
    /// Mixed Integer Linear Programming solver wrapper.
    /// </summary>
    public sealed class MilpSolver : BaseSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MilpSolver"/> class.
        /// </summary>
        /// <param name="backend">Backend implementation to use.</param>
        public MilpSolver(ISolverBackend? backend = null)
            : base("MILP", backend ?? new OrToolsBackend())
        {
        }
    }

    /// <summary>
    /// Legacy casing preserved for compatibility.
    /// </summary>
    public sealed class MILPsolver : MilpSolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MILPsolver"/> class.
        /// </summary>
        public MILPsolver(ISolverBackend? backend = null) : base(backend)
        {
        }
    }
}
