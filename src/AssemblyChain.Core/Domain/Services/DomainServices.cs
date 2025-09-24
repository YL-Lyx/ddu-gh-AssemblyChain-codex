using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.Interfaces;
using AssemblyChain.Core.Domain.ValueObjects;
using Rhino.Geometry;

namespace AssemblyChain.Core.Domain.Services
{
    /// <summary>
    /// Domain services for complex business logic
    /// </summary>
    public class DomainServices
    {
        private readonly IPartRepository _partRepository;
        private readonly IAssemblyService _assemblyService;

        public DomainServices(IPartRepository partRepository, IAssemblyService assemblyService)
        {
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _assemblyService = assemblyService ?? throw new ArgumentNullException(nameof(assemblyService));
        }

        /// <summary>
        /// Analyzes disassembly sequence feasibility
        /// </summary>
        public async Task<DisassemblyAnalysis> AnalyzeDisassemblySequenceAsync(Assembly assembly, IEnumerable<Part> removalSequence)
        {
            var analysis = new DisassemblyAnalysis();
            var remainingParts = new HashSet<Part>(assembly.GetAllParts());

            foreach (var partToRemove in removalSequence)
            {
                if (!remainingParts.Contains(partToRemove))
                {
                    analysis.AddError($"Part {partToRemove.Name} is not in the assembly or already removed");
                    continue;
                }

                // Check if part can be removed (no blocking parts)
                var blockingParts = await FindBlockingPartsAsync(assembly, partToRemove, remainingParts);
                if (blockingParts.Any())
                {
                    analysis.AddError($"Part {partToRemove.Name} is blocked by: {string.Join(", ", blockingParts.Select(p => p.Name))}");
                }
                else
                {
                    analysis.RemovableParts.Add(partToRemove);
                    remainingParts.Remove(partToRemove);
                }
            }

            analysis.IsFeasible = !analysis.Errors.Any();
            return analysis;
        }

        /// <summary>
        /// Finds parts that block the removal of a given part
        /// </summary>
        public async Task<IEnumerable<Part>> FindBlockingPartsAsync(Assembly assembly, Part targetPart, IEnumerable<Part> remainingParts)
        {
            var blockingParts = new List<Part>();

            foreach (var otherPart in remainingParts.Where(p => p.Id != targetPart.Id))
            {
                if (await PartsIntersectAsync(targetPart, otherPart))
                {
                    // Check if otherPart is geometrically blocking targetPart
                    if (IsBlocking(targetPart, otherPart))
                    {
                        blockingParts.Add(otherPart);
                    }
                }
            }

            return blockingParts;
        }

        /// <summary>
        /// Calculates optimal assembly sequence using heuristic algorithms
        /// </summary>
        public async Task<AssemblySequence> CalculateOptimalSequenceAsync(Assembly assembly)
        {
            var sequence = new AssemblySequence();
            var remainingParts = new List<Part>(assembly.GetAllParts());
            var stepNumber = 1;

            while (remainingParts.Any())
            {
                // Find parts that can be removed (no blockers)
                var removableParts = new List<Part>();

                foreach (var part in remainingParts)
                {
                    var blockingParts = await FindBlockingPartsAsync(assembly, part, remainingParts);
                    if (!blockingParts.Any())
                    {
                        removableParts.Add(part);
                    }
                }

                if (!removableParts.Any())
                {
                    sequence.AddError("Deadlock detected: no parts can be removed");
                    break;
                }

                // For simplicity, remove the first removable part
                // In practice, you might want to optimize based on criteria
                var partToRemove = removableParts.First();
                sequence.AddStep(stepNumber++, partToRemove, Vector3d.ZAxis); // Default upward motion
                remainingParts.Remove(partToRemove);
            }

            sequence.IsComplete = !remainingParts.Any() && !sequence.Errors.Any();
            return sequence;
        }

        /// <summary>
        /// Validates part geometry and physics compatibility
        /// </summary>
        public ValidationResult ValidatePart(Part part)
        {
            var result = new ValidationResult();

            // Check geometry
            if (!part.HasValidGeometry)
            {
                result.AddError("Part geometry is invalid or missing");
            }

            // Check physics properties
            if (part.Physics.Mass <= 0)
            {
                result.AddError("Part mass must be positive");
            }

            // Check material compatibility
            if (part.Material.Density <= 0)
            {
                result.AddWarning("Material density should be positive");
            }

            // Check for unrealistic combinations
            var calculatedVolume = part.Geometry.Mesh.Volume();
            var expectedMass = part.Material.Density * calculatedVolume / 1000; // Convert cm³ to m³
            var massRatio = part.Physics.Mass / expectedMass;

            if (massRatio < 0.1 || massRatio > 10)
            {
                result.AddWarning($"Mass ({part.Physics.Mass:F2}kg) seems inconsistent with material and geometry (expected ~{expectedMass:F2}kg)");
            }

            return result;
        }

        /// <summary>
        /// Calculates stability of an assembly configuration
        /// </summary>
        public async Task<StabilityAnalysis> AnalyzeStabilityAsync(Assembly assembly)
        {
            var analysis = new StabilityAnalysis();

            // Calculate center of mass
            var allParts = assembly.GetAllParts().ToList();
            var totalMass = allParts.Sum(p => p.Physics.Mass);
            var centerOfMass = Point3d.Origin;

            foreach (var part in allParts)
            {
                var partCOM = CalculateCenterOfMass(part);
                centerOfMass += partCOM * (part.Physics.Mass / totalMass);
            }

            analysis.CenterOfMass = centerOfMass;
            analysis.TotalMass = totalMass;

            // Check stability on different axes
            var supportPolygon = CalculateSupportPolygon(assembly);
            analysis.IsStableXY = supportPolygon.Contains(analysis.CenterOfMass);
            analysis.SupportPolygon = supportPolygon;

            return analysis;
        }

        private async Task<bool> PartsIntersectAsync(Part partA, Part partB)
        {
            // Simplified intersection check - in practice would use proper collision detection
            // Simplified bounding box overlap check (replaces non-existent Intersects)
            var bboxA = partA.Geometry.BoundingBox;
            var bboxB = partB.Geometry.BoundingBox;
            return bboxA.Min.X < bboxB.Max.X && bboxA.Max.X > bboxB.Min.X &&
                   bboxA.Min.Y < bboxB.Max.Y && bboxA.Max.Y > bboxB.Min.Y &&
                   bboxA.Min.Z < bboxB.Max.Z && bboxA.Max.Z > bboxB.Min.Z;
        }

        private bool IsBlocking(Part target, Part blocker)
        {
            // Simplified blocking logic - check if blocker is "above" target
            var targetTop = target.Geometry.BoundingBox.Max.Z;
            var blockerBottom = blocker.Geometry.BoundingBox.Min.Z;

            return blockerBottom > targetTop;
        }

        private Point3d CalculateCenterOfMass(Part part)
        {
            // Simplified COM calculation - use bounding box center
            return part.Geometry.BoundingBox.Center;
        }

        private Polyline CalculateSupportPolygon(Assembly assembly)
        {
            // Simplified support polygon - project all bottom faces
            var points = new List<Point3d>();

            foreach (var part in assembly.GetAllParts())
            {
                var bbox = part.Geometry.BoundingBox;
                points.Add(new Point3d(bbox.Min.X, bbox.Min.Y, bbox.Min.Z));
                points.Add(new Point3d(bbox.Max.X, bbox.Min.Y, bbox.Min.Z));
                points.Add(new Point3d(bbox.Max.X, bbox.Max.Y, bbox.Min.Z));
                points.Add(new Point3d(bbox.Min.X, bbox.Max.Y, bbox.Min.Z));
            }

            return new Polyline(points.Distinct());
        }
    }

    /// <summary>
    /// Result of disassembly sequence analysis
    /// </summary>
    public class DisassemblyAnalysis
    {
        public List<Part> RemovableParts { get; } = new();
        public List<string> Errors { get; } = new();
        public bool IsFeasible { get; set; }

        public void AddError(string error) => Errors.Add(error);
    }

    /// <summary>
    /// Assembly sequence with steps
    /// </summary>
    public class AssemblySequence
    {
        public List<AssemblyStep> Steps { get; } = new();
        public List<string> Errors { get; } = new();
        public bool IsComplete { get; set; }

        public void AddStep(int stepNumber, Part part, Vector3d direction)
        {
            Steps.Add(new AssemblyStep(stepNumber, part, direction));
        }

        public void AddError(string error) => Errors.Add(error);
    }

    /// <summary>
    /// Single step in assembly sequence
    /// </summary>
    public class AssemblyStep
    {
        public int StepNumber { get; }
        public Part Part { get; }
        public Vector3d Direction { get; }

        public AssemblyStep(int stepNumber, Part part, Vector3d direction)
        {
            StepNumber = stepNumber;
            Part = part;
            Direction = direction;
        }
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();

        public bool IsValid => !Errors.Any();

        public void AddError(string error) => Errors.Add(error);
        public void AddWarning(string warning) => Warnings.Add(warning);
    }

    /// <summary>
    /// Stability analysis result
    /// </summary>
    public class StabilityAnalysis
    {
        public Point3d CenterOfMass { get; set; }
        public double TotalMass { get; set; }
        public bool IsStableXY { get; set; }
        public Polyline SupportPolygon { get; set; }
    }
}

