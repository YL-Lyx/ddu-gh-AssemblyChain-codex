using System.Collections.Generic;
using AssemblyChain.Core.Model;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Request payload for ONNX inference stubs.
    /// </summary>
    public sealed class OnnxInferenceRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnnxInferenceRequest"/> class.
        /// </summary>
        /// <param name="assembly">Assembly snapshot to evaluate.</param>
        /// <param name="features">Optional pre-computed features.</param>
        public OnnxInferenceRequest(AssemblyModel assembly, IReadOnlyDictionary<string, double>? features = null)
        {
            Assembly = assembly;
            Features = features;
        }

        /// <summary>
        /// Gets the assembly snapshot.
        /// </summary>
        public AssemblyModel Assembly { get; }

        /// <summary>
        /// Gets the optional feature set used to shortcut preprocessing.
        /// </summary>
        public IReadOnlyDictionary<string, double>? Features { get; }
    }
}
