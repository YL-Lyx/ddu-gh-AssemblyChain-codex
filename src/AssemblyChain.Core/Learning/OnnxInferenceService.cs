using System;
using System.Collections.Generic;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Model;

namespace AssemblyChain.Core.Learning
{
    /// <summary>
    /// Lightweight stub that documents the expected behaviour for future ONNX integrations.
    /// </summary>
    public sealed class OnnxInferenceService
    {
        /// <summary>
        /// Executes inference for the provided request. The default implementation returns deterministic scores
        /// based on simple heuristics to allow upstream tooling to be exercised without the heavy dependency footprint.
        /// </summary>
        /// <param name="request">Inference request.</param>
        /// <returns>Stubbed inference result.</returns>
        public OnnxInferenceResult Run(OnnxInferenceRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Assembly is not AssemblyModel assembly)
            {
                throw new ArgumentException("AssemblyModel instance is required", nameof(request));
            }

            var graspability = 0.5;
            if (request.Features != null && request.Features.TryGetValue("graspability", out var featureValue))
            {
                graspability = featureValue;
            }

            var scores = new Dictionary<string, double>
            {
                ["stability"] = Math.Clamp(assembly.PartCount / 10.0, 0.0, 1.0),
                ["graspability"] = graspability,
                ["complexity"] = Math.Clamp(assembly.PartCount / 25.0, 0.0, 1.0)
            };

            return new OnnxInferenceResult(scores);
        }
    }
}
