using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Toolkit.Processing
{
    /// <summary>
    /// Creates assemblies from a collection of parts while tracking diagnostics.
    /// </summary>
    public static class AssemblyBuilder
    {
        public sealed class AssemblyCreationResult
        {
            public AssemblyCreationResult(Assembly? assembly)
            {
                Assembly = assembly;
            }

            public Assembly? Assembly { get; }
            public int SuccessCount { get; init; }
            public int FailureCount { get; init; }
            public List<ProcessingMessage> Messages { get; } = new();
            public bool HasAssembly => Assembly != null;
        }

        public static AssemblyCreationResult Build(string? baseName, IEnumerable<Part?> parts)
        {
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            var name = string.IsNullOrWhiteSpace(baseName) ? "Assembly" : baseName!.Trim();
            var partList = parts.ToList();
            var validParts = partList.Where(p => p != null).Cast<Part>().ToList();
            var failureCount = partList.Count - validParts.Count;

            var result = validParts.Count == 0
                ? new AssemblyCreationResult(null)
                {
                    FailureCount = failureCount
                }
                : new AssemblyCreationResult(CreateAssembly(name, validParts))
                {
                    SuccessCount = validParts.Count,
                    FailureCount = failureCount
                };

            if (failureCount > 0)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Warning,
                    $"Skipped {failureCount} invalid part entries."));
            }

            if (!result.HasAssembly)
            {
                result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Error, "No valid parts found."));
                return result;
            }

            var report = $"Created assembly '{name}' with {result.SuccessCount} parts";
            if (failureCount > 0)
            {
                report += $", {failureCount} failed";
            }

            result.Messages.Add(new ProcessingMessage(ProcessingMessageLevel.Remark, report));
            return result;
        }

        private static Assembly CreateAssembly(string name, IReadOnlyList<Part> parts)
        {
            var assembly = new Assembly(0, name);
            foreach (var part in parts)
            {
                assembly.AddPart(part);
            }

            return assembly;
        }
    }
}
