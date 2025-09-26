using System;

namespace AssemblyChain.IO.Contracts
{
    /// <summary>
    /// Options controlling how process JSON files are exported for robotic execution.
    /// </summary>
    public sealed class ProcessExportOptions
    {
        /// <summary>
        /// Gets or sets the absolute or relative path to the JSON file to be written.
        /// </summary>
        public string? OutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include solver metadata in the export.
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets an optional author label stored in the file header.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets an optional description for traceability.
        /// </summary>
        public string? Description { get; set; }
    }
}
