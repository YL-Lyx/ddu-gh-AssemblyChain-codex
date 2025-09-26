using System.Collections.Generic;

namespace AssemblyChain.Core.Contracts
{
    /// <summary>
    /// Options controlling dataset export for downstream learning workflows.
    /// </summary>
    public sealed class DatasetExportOptions
    {
        /// <summary>
        /// Gets or sets the directory where dataset files are written.
        /// </summary>
        public string OutputDirectory { get; set; } = "dataset";

        /// <summary>
        /// Gets or sets a value indicating whether to embed mesh geometry.
        /// </summary>
        public bool IncludeGeometry { get; set; }

        /// <summary>
        /// Gets or sets an optional set of tags to annotate each record.
        /// </summary>
        public IReadOnlyList<string>? Tags { get; set; }
    }
}
