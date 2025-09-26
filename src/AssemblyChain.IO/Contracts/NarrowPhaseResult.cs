// 改造目的：统一窄相检测输出结构，消除对 ContactModel 的直接依赖。
// 兼容性注意：保留结果集合，允许上层继续组装 ContactData。
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AssemblyChain.IO.Contracts
{
    /// <summary>
    /// Represents the outcome of a narrow-phase detection pass.
    /// </summary>
    public sealed class NarrowPhaseResult
    {
        private NarrowPhaseResult(
            bool success,
            IReadOnlyList<ContactZone> zones,
            IReadOnlyDictionary<string, object> metadata,
            IReadOnlyList<string> diagnostics)
        {
            IsSuccessful = success;
            Zones = zones ?? Array.Empty<ContactZone>();
            Metadata = metadata ?? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
            Diagnostics = diagnostics ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets a value indicating whether the detection succeeded.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Gets the zones that have been validated by the detection pipeline.
        /// </summary>
        public IReadOnlyList<ContactZone> Zones { get; }

        /// <summary>
        /// Gets metadata about the detection run (timings, algorithm choices, etc.).
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets human readable diagnostics that can be logged or surfaced to the UI.
        /// </summary>
        public IReadOnlyList<string> Diagnostics { get; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="zones">The validated contact zones.</param>
        /// <param name="metadata">Optional metadata entries.</param>
        /// <param name="diagnostics">Optional diagnostic messages.</param>
        public static NarrowPhaseResult Success(
            IEnumerable<ContactZone> zones,
            IDictionary<string, object>? metadata = null,
            IEnumerable<string>? diagnostics = null)
        {
            return new NarrowPhaseResult(
                true,
                new ReadOnlyCollection<ContactZone>((zones ?? Array.Empty<ContactZone>()).ToList()),
                metadata != null
                    ? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(metadata))
                    : new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()),
                new ReadOnlyCollection<string>((diagnostics ?? Array.Empty<string>()).ToList()));
        }

        /// <summary>
        /// Creates a failed result, preserving diagnostics.
        /// </summary>
        /// <param name="diagnostics">Failure details.</param>
        /// <param name="metadata">Optional metadata entries.</param>
        public static NarrowPhaseResult Failure(
            IEnumerable<string>? diagnostics = null,
            IDictionary<string, object>? metadata = null)
        {
            return new NarrowPhaseResult(
                false,
                Array.Empty<ContactZone>(),
                metadata != null
                    ? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(metadata))
                    : new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()),
                new ReadOnlyCollection<string>((diagnostics ?? Array.Empty<string>()).ToList()));
        }

        /// <summary>
        /// Merges two results, keeping diagnostics and union of zones.
        /// </summary>
        /// <param name="other">The other result to merge.</param>
        /// <returns>A combined result representing both pipelines.</returns>
        public NarrowPhaseResult Merge(NarrowPhaseResult other)
        {
            other = other ?? Failure();
            return other.IsSuccessful && IsSuccessful
                ? Success(Zones.Concat(other.Zones), MergeMetadata(Metadata, other.Metadata), Diagnostics.Concat(other.Diagnostics))
                : Failure(Diagnostics.Concat(other.Diagnostics), MergeMetadata(Metadata, other.Metadata));
        }

        private static IDictionary<string, object> MergeMetadata(
            IReadOnlyDictionary<string, object> left,
            IReadOnlyDictionary<string, object> right)
        {
            var dict = new Dictionary<string, object>();
            foreach (var kv in left)
            {
                dict[kv.Key] = kv.Value;
            }

            foreach (var kv in right)
            {
                dict[$"{kv.Key}"] = kv.Value;
            }

            return dict;
        }
    }
}
