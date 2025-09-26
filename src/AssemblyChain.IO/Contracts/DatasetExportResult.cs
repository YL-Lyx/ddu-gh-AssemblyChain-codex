namespace AssemblyChain.IO.Contracts
{
    /// <summary>
    /// Summary returned after a dataset export operation.
    /// </summary>
    public sealed class DatasetExportResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetExportResult"/> class.
        /// </summary>
        /// <param name="recordCount">Number of records exported.</param>
        /// <param name="outputDirectory">Directory path used for export.</param>
        public DatasetExportResult(int recordCount, string outputDirectory)
        {
            RecordCount = recordCount;
            Directory = outputDirectory;
        }

        /// <summary>
        /// Gets the number of records written to disk.
        /// </summary>
        public int RecordCount { get; }

        /// <summary>
        /// Gets the directory path used for export.
        /// </summary>
        public string Directory { get; }
    }
}
