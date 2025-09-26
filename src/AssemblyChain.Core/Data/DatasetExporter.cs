using System;
using System.Collections.Generic;
using System.IO;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Model;
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
            AssemblyModel assembly,
            ContactModel contacts,
            DgSolverModel solverResult,
            DatasetExportOptions options)
        {
            ValidateArguments(assembly, contacts, solverResult);

            options ??= new DatasetExportOptions();
            EnsureOutputDirectory(options.OutputDirectory);

            var record = CreateDatasetRecord(assembly, contacts, solverResult, options);
            WriteDatasetRecord(options.OutputDirectory, assembly.Name, record);

            return new DatasetExportResult(recordCount: 1, outputDirectory: options.OutputDirectory);
        }

        private static void ValidateArguments(
            AssemblyModel assembly,
            ContactModel contacts,
            DgSolverModel solverResult)
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
        }

        private static void EnsureOutputDirectory(string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("Output directory must be provided", nameof(outputDirectory));
            }

            Directory.CreateDirectory(outputDirectory);
        }

        private static DatasetRecord CreateDatasetRecord(
            AssemblyModel assembly,
            ContactModel contacts,
            DgSolverModel solverResult,
            DatasetExportOptions options)
        {
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
                record.Geometry = BuildGeometryRecords(assembly);
            }

            return record;
        }

        private static List<DatasetGeometryRecord> BuildGeometryRecords(AssemblyModel assembly)
        {
            var geometry = new List<DatasetGeometryRecord>(assembly.PartCount);

            foreach (var part in assembly.Parts)
            {
                geometry.Add(new DatasetGeometryRecord
                {
                    PartId = part.Id,
                    PartName = part.Name,
                    BoundingBox = part.BoundingBox.ToString()
                });
            }

            return geometry;
        }

        private static void WriteDatasetRecord(
            string outputDirectory,
            string assemblyName,
            DatasetRecord record)
        {
            var fileName = Path.Combine(
                outputDirectory,
                $"{assemblyName.Replace(' ', '_')}_dataset.json");

            var payload = JsonConvert.SerializeObject(record, Formatting.Indented);
            File.WriteAllText(fileName, payload);
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
