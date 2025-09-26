using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyChain.Analysis;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Core.Spatial;
using AssemblyChain.Graphs;

namespace AssemblyChain.Planning;

/// <summary>
/// Naive tree search solver that incrementally builds an assembly by selecting the next part whose dependencies have already
/// been placed. The solver validates stability and path feasibility at each step.
/// </summary>
public sealed class TreeSearchSolver
{
    private readonly StabilityAnalyzer _stabilityAnalyzer;
    private readonly PathFeasibilityChecker _pathChecker;

    public TreeSearchSolver(StabilityAnalyzer stabilityAnalyzer, PathFeasibilityChecker? pathChecker = null)
    {
        _stabilityAnalyzer = stabilityAnalyzer ?? throw new ArgumentNullException(nameof(stabilityAnalyzer));
        _pathChecker = pathChecker ?? new PathFeasibilityChecker();
    }

    public AssemblyPlan Solve(Assembly assembly, Graph adjacency, IEnumerable<string> groundedPartIds)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(adjacency);
        ArgumentNullException.ThrowIfNull(groundedPartIds);
        var grounded = groundedPartIds.ToHashSet();
        var placed = new HashSet<string>(grounded);
        var remaining = assembly.Parts.Select(p => p.Id).Except(grounded).ToHashSet();
        var steps = new List<PlanStep>();
        var diagnostics = new List<string>();
        int index = 0;
        while (remaining.Count > 0)
        {
            var candidate = remaining.FirstOrDefault(partId => adjacency.Neighbours(partId).All(placed.Contains));
            if (candidate is null)
            {
                diagnostics.Add("Unable to find a part whose neighbours are already placed.");
                break;
            }

            if (!_pathChecker.IsFeasible(assembly.PartLookup[candidate], placed.Select(id => assembly.PartLookup[id])))
            {
                diagnostics.Add($"Path for part '{candidate}' is blocked by already placed parts.");
                remaining.Remove(candidate);
                continue;
            }

            var stability = _stabilityAnalyzer.Compute(assembly, grounded.Union(placed).Append(candidate));
            if (stability.Margin < 0)
            {
                diagnostics.Add($"Adding part '{candidate}' would destabilise the structure (margin {stability.Margin:F3}).");
                remaining.Remove(candidate);
                continue;
            }

            steps.Add(new PlanStep(index++, "Place", candidate));
            placed.Add(candidate);
            remaining.Remove(candidate);
        }

        var isValid = remaining.Count == 0;
        if (!isValid)
        {
            diagnostics.Add("Tree search solver failed to place all parts.");
        }

        return new AssemblyPlan("TreeSearch", steps, isValid, diagnostics);
    }
}

/// <summary>
/// Simplified path feasibility check that ensures the candidate part can be moved into position without intersecting existing
/// parts using axis-aligned bounding boxes.
/// </summary>
public sealed class PathFeasibilityChecker
{
    public bool IsFeasible(Part candidate, IEnumerable<Part> placedParts)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(placedParts);
        foreach (var part in placedParts)
        {
            if (candidate.BoundingBox.Overlaps(part.BoundingBox, 1e-3))
            {
                return false;
            }
        }

        return true;
    }
}
