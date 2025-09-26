using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Model;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace AssemblyChain.Core.Robotics
{
    /// <summary>
    /// Strongly typed representation of the JSON process file consumed by the UR10 execution stack.
    /// </summary>
    public sealed class ProcessSchema
    {
        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        [JsonProperty("schemaVersion")]
        public string SchemaVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets file header metadata.
        /// </summary>
        [JsonProperty("header")]
        public ProcessHeader Header { get; set; } = new();

        /// <summary>
        /// Gets or sets the ordered list of process steps.
        /// </summary>
        [JsonProperty("steps")]
        public List<ProcessStep> Steps { get; set; } = new();

        /// <summary>
        /// Gets or sets optional metadata dictionary.
        /// </summary>
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Converts the schema to JSON text.
        /// </summary>
        public string ToJson(bool indented = true)
        {
            var formatting = indented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting);
        }

        /// <summary>
        /// Writes the schema to disk using the provided options.
        /// </summary>
        /// <param name="options">Export options.</param>
        public void WriteToDisk(ProcessExportOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                throw new InvalidOperationException("Process export requires an explicit output path.");
            }

            var directory = Path.GetDirectoryName(options.OutputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(options.OutputPath, ToJson());
        }

        /// <summary>
        /// Creates a process schema from a solver model.
        /// </summary>
        /// <param name="result">Solver result.</param>
        /// <param name="options">Export options.</param>
        /// <returns>A populated schema instance.</returns>
        public static ProcessSchema FromSolverResult(DgSolverModel result, ProcessExportOptions options)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            options ??= new ProcessExportOptions();

            var steps = result.Steps
                .Select((step, index) => ProcessStep.From(step, result.Vectors[index]))
                .ToList();

            var schema = new ProcessSchema
            {
                Header = ProcessHeader.From(options),
                Steps = steps,
            };

            if (options.IncludeMetadata)
            {
                schema.Metadata = new Dictionary<string, object>
                {
                    ["solver"] = result.SolverType,
                    ["feasible"] = result.IsFeasible,
                    ["optimal"] = result.IsOptimal,
                    ["solveTime"] = result.SolveTimeSeconds,
                    ["stepCount"] = result.StepCount
                };

                foreach (var kvp in result.Metadata)
                {
                    schema.Metadata[kvp.Key] = kvp.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(options.OutputPath))
            {
                schema.Header.OutputPath = options.OutputPath;
            }

            return schema;
        }
    }

    /// <summary>
    /// Header information for exported process files.
    /// </summary>
    public sealed class ProcessHeader
    {
        /// <summary>
        /// Gets or sets the generator identifier.
        /// </summary>
        [JsonProperty("generator")]
        public string Generator { get; set; } = "AssemblyChain";

        /// <summary>
        /// Gets or sets the author value.
        /// </summary>
        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the timestamp (UTC ISO-8601).
        /// </summary>
        [JsonProperty("createdUtc")]
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");

        /// <summary>
        /// Gets or sets the optional output path.
        /// </summary>
        [JsonProperty("output", NullValueHandling = NullValueHandling.Ignore)]
        public string? OutputPath { get; set; }

        /// <summary>
        /// Creates a header from options.
        /// </summary>
        public static ProcessHeader From(ProcessExportOptions options)
        {
            return new ProcessHeader
            {
                Author = options.Author,
                Description = options.Description,
                OutputPath = options.OutputPath
            };
        }
    }

    /// <summary>
    /// Represents a single process step.
    /// </summary>
    public sealed class ProcessStep
    {
        /// <summary>
        /// Gets or sets the sequential index.
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the part identifier.
        /// </summary>
        [JsonProperty("partId")]
        public string PartId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nominal insertion direction.
        /// </summary>
        [JsonProperty("direction")]
        public double[] Direction { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Gets or sets the batch/group identifier.
        /// </summary>
        [JsonProperty("batch")]
        public int Batch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is an insertion.
        /// </summary>
        [JsonProperty("insert")]
        public bool Insert { get; set; }

        /// <summary>
        /// Factory creating a process step from domain types.
        /// </summary>
        public static ProcessStep From(Step step, Vector3d direction)
        {
            return new ProcessStep
            {
                Index = step.Index,
                PartId = step.Part?.Name ?? step.Part?.Id.ToString() ?? $"part-{step.Index}",
                Direction = new[] { direction.X, direction.Y, direction.Z },
                Batch = step.Batch,
                Insert = step.Insert
            };
        }
    }
}
