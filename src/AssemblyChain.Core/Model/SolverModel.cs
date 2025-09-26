using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Domain.Entities;
using Rhino.Geometry;

namespace AssemblyChain.Core.Model
{
    /// <summary>
    /// Read-only solver model.
    /// Contains sequence planning results with steps, vectors, groups, and metadata.
    /// </summary>
    public sealed class DgSolverModel : ISolverModel
    {
        public IReadOnlyList<Step> Steps { get; }
        public IReadOnlyList<Vector3d> Vectors { get; }
        public IReadOnlyList<IReadOnlyList<int>> Groups { get; }
        public bool IsFeasible { get; }
        public bool IsOptimal { get; }
        public string Log { get; }
        public double SolveTimeSeconds { get; }
        public string SolverType { get; }
        public IReadOnlyDictionary<string, object> Metadata { get; }
        public int StepCount => Steps.Count;
        public bool IsAssemblySequence { get; }

        internal DgSolverModel(
            IReadOnlyList<Step> steps,
            IReadOnlyList<Vector3d> vectors,
            IReadOnlyList<IReadOnlyList<int>> groups,
            bool isFeasible,
            bool isOptimal,
            string log,
            double solveTimeSeconds,
            string solverType,
            IReadOnlyDictionary<string, object> metadata = null,
            bool isAssemblySequence = false)
        {
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            Vectors = vectors ?? throw new ArgumentNullException(nameof(vectors));
            Groups = groups ?? throw new ArgumentNullException(nameof(groups));
            IsFeasible = isFeasible;
            IsOptimal = isOptimal;
            Log = log ?? string.Empty;
            SolveTimeSeconds = solveTimeSeconds;
            SolverType = solverType ?? "Unknown";
            Metadata = metadata ?? new Dictionary<string, object>();
            IsAssemblySequence = isAssemblySequence;
            if (Steps.Count != Vectors.Count)
                throw new ArgumentException("Steps and Vectors must have the same length");
        }

        public Step GetStep(int index)
        {
            if (index < 0 || index >= Steps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Steps[index];
        }

        public Vector3d GetVector(int index)
        {
            if (index < 0 || index >= Vectors.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Vectors[index];
        }

        public DgSolverModel ToAssemblySequence()
        {
            if (IsAssemblySequence) return this;

            var assemblySteps = new List<Step>(Steps.Count);
            var assemblyVectors = new List<Vector3d>(Vectors.Count);
            var assemblyGroups = new List<IReadOnlyList<int>>(Groups.Count);

            for (int i = Steps.Count - 1; i >= 0; i--)
            {
                var step = Steps[i];
                assemblySteps.Add(new Step(assemblySteps.Count, step.Part, -step.Direction)
                {
                    Insert = true,
                    Batch = step.Batch
                });
                assemblyVectors.Add(-Vectors[i]);
            }

            for (int i = Groups.Count - 1; i >= 0; i--)
            {
                assemblyGroups.Add(Groups[i]);
            }

            return new DgSolverModel(
                assemblySteps,
                assemblyVectors,
                assemblyGroups,
                IsFeasible,
                IsOptimal,
                Log,
                SolveTimeSeconds,
                SolverType,
                Metadata,
                true
            );
        }

        public string GetSummary()
        {
            return $"{SolverType}: {(IsFeasible ? "Feasible" : "Infeasible")}, " +
                   $"{StepCount} steps, {SolveTimeSeconds:F3}s, " +
                   $"{(IsOptimal ? "Optimal" : "Suboptimal")}";
        }
    }

    // Minimal placeholder for Step (replace with real domain type if available)
    public sealed class Step
    {
        public Step(int index, Part part, Vector3d direction)
        {
            Index = index;
            Part = part;
            Direction = direction;
        }
        public int Index { get; }
        public Part Part { get; }
        public Vector3d Direction { get; }
        public bool Insert { get; set; }
        public int Batch { get; set; }
    }
}



