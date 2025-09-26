// 改造目的：提供 OR-Tools 求解后端桩，统一可行性输出。
// 兼容性注意：当前实现为占位，真实环境需引用 Google.OrTools 相关 NuGet 包。
using System;
using System.Collections.Generic;
using AssemblyChain.Core.Model;
using Rhino.Geometry;

namespace AssemblyChain.Core.Solver.Backends
{
    /// <summary>
    /// OR-Tools backend adapter (placeholder implementation).
    /// </summary>
    public sealed class OrToolsBackend : ISolverBackend
    {
        /// <inheritdoc />
        public SolverBackendResult Solve(SolverBackendRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // NOTE: 实际实现应引用 Google.OrTools（示例 NuGet: Google.OrTools 9.x）并构造 CP-SAT/MIP 模型。
            // 在缺乏外部依赖时返回占位结果，保持上层流程可测试。
            var steps = new List<Step>();
            var motions = new List<Vector3d>();
            var groups = new List<IReadOnlyList<int>>();

            var metadata = new Dictionary<string, object>
            {
                ["solver"] = request.SolverId,
                ["backend"] = "ortools-stub",
                ["timestamp"] = DateTime.UtcNow
            };

            return new SolverBackendResult(
                SolverOutcome.Infeasible,
                steps,
                motions,
                groups,
                "OR-Tools backend stub executed (no-op)",
                metadata);
        }
    }
}
