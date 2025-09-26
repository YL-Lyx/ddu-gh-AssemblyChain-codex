using System;
using System.Collections.Generic;
using System.IO;
using AssemblyChain.Core.Contracts;
using Newtonsoft.Json;

namespace AssemblyChain.Core.Data
{
    /// <summary>
    /// Responsible for exporting dataset snapshots used by downstream learning workflows.
    /// </summary>
    public static class DatasetExporter
    {
        /// <summary>
        /// Exports the dataset for the provided assembly and solver outcome.
        /// </summary>
        /// <param name="assembly">Assembly snapshot.</param>
        /// <param name="contacts">Contact model.</param>
        /// <param name="solverResult">Solver result.</param>
        /// <param name="options">Export options.</param>
        /// <returns>A summary of the exported dataset.</returns>
        public static DatasetExportResult Export(
            IModelQuery assembly,
            IContactModel contacts,
            ISolverModel solverResult,
            DatasetExportOptions options)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            if (solverResult == null)
            {
                throw new ArgumentNullException(nameof(solverResult));
            }

            options ??= new DatasetExportOptions();

            Directory.CreateDirectory(options.OutputDirectory);

            var record = new DatasetRecord
            {
                AssemblyName = assembly.Name,
                PartCount = assembly.PartCount,
                ContactCount = contacts.ContactCount,
                SolverSummary = solverResult.GetSummary(),
                SolverMetadata = solverResult.Metadata,
                Tags = options.Tags,
                IncludeGeometry = options.IncludeGeometry
            };

            if (options.IncludeGeometry)
            {
                record.Geometry = new List<DatasetGeometryRecord>();
                foreach (var part in assembly.Parts)
                {
                    record.Geometry.Add(new DatasetGeometryRecord
                    {
                        PartId = part.Id,
                        PartName = part.Name,
                        BoundingBox = part.BoundingBox.ToString()
                    });
                }
            }

            var fileName = Path.Combine(options.OutputDirectory, $"{assembly.Name.Replace(' ', '_')}_dataset.json");
            File.WriteAllText(fileName, JsonConvert.SerializeObject(record, Formatting.Indented));

            return new DatasetExportResult(recordCount: 1, outputDirectory: options.OutputDirectory);
        }

        private sealed class DatasetRecord
        {
            public string AssemblyName { get; set; } = string.Empty;

            public int PartCount { get; set; }

            public int ContactCount { get; set; }

            public string SolverSummary { get; set; } = string.Empty;

            public IReadOnlyDictionary<string, object> SolverMetadata { get; set; }
                = new Dictionary<string, object>();

            public IReadOnlyList<string>? Tags { get; set; }

            public bool IncludeGeometry { get; set; }

            public List<DatasetGeometryRecord>? Geometry { get; set; }
        }

        private sealed class DatasetGeometryRecord
        {
            public int PartId { get; set; }

            public string PartName { get; set; } = string.Empty;

            public string BoundingBox { get; set; } = string.Empty;
        }
    }
}
