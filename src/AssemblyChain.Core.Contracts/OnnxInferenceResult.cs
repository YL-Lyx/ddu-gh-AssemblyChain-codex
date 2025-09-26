using System.Collections.Generic;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Result stub returned from ONNX inference operations.
    /// </summary>
    public sealed class OnnxInferenceResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnnxInferenceResult"/> class.
        /// </summary>
        /// <param name="scores">Label-score pairs predicted by the model.</param>
        public OnnxInferenceResult(IReadOnlyDictionary<string, double> scores)
        {
            Scores = scores ?? new Dictionary<string, double>();
        }

        /// <summary>
        /// Gets the label-score pairs predicted by the model.
        /// </summary>
        public IReadOnlyDictionary<string, double> Scores { get; }
    }
}
