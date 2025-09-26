using System;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Data;
using AssemblyChain.Core.Learning;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Robotics;
using AssemblyChain.Core.Solver;
using AssemblyChain.Core.Solver.Backends;

namespace AssemblyChain.Core.Facade
{
    /// <summary>
    /// Facade orchestrating contact detection, sequence planning, robotic export and learning hooks.
    /// </summary>
    public sealed class AssemblyChainFacade : IAssemblyChainFacade
    {
        private readonly IContactUtils _contactUtils;
        private readonly Func<SolverType, ISolver> _solverFactory;
        private readonly Func<SolverType, ISolverBackend> _backendSelector;
        private readonly OnnxInferenceService _inferenceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyChainFacade"/> class.
        /// </summary>
        /// <param name="contactUtils">Contact utilities implementation.</param>
        /// <param name="solverFactory">Factory resolving solver instances for a given solver type.</param>
        /// <param name="inferenceService">ONNX inference stub service.</param>
        /// <param name="backendSelector">Optional backend selector override.</param>
        public AssemblyChainFacade(
            IContactUtils? contactUtils = null,
            Func<SolverType, ISolver>? solverFactory = null,
            OnnxInferenceService? inferenceService = null,
            Func<SolverType, ISolverBackend>? backendSelector = null)
        {
            _contactUtils = contactUtils ?? new ContactUtils();
            _backendSelector = backendSelector ?? (mode => ResolveDefaultBackend(mode));
            _solverFactory = solverFactory ?? (mode => ResolveDefaultSolver(mode, _backendSelector(mode)));
            _inferenceService = inferenceService ?? new OnnxInferenceService();
        }

        /// <summary>
        /// Gets the contact utilities used by the facade.
        /// </summary>
        public IContactUtils ContactUtils => _contactUtils;

        /// <summary>
        /// Runs the complete planning pipeline given a structured request.
        /// </summary>
        /// <param name="request">Request payload.</param>
        /// <returns>Plan result containing contact and solver outputs.</returns>
        public AssemblyPlanResult RunPlan(AssemblyPlanRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var contacts = request.Contacts ?? DetectContacts(request.Assembly, request.Detection ?? new DetectionOptions());
            var constraints = request.Constraints ?? ConstraintModelFactory.CreateEmpty(request.Assembly);

            var solver = _solverFactory(NormalizeSolverType(request.Solver.SolverType));
            var solverResult = solver.Solve(request.Assembly, contacts, constraints, request.Solver);
            return new AssemblyPlanResult(contacts, solverResult);
        }

        /// <summary>
        /// Builds constraint data and solves the planning problem with the configured backend.
        /// </summary>
        /// <param name="assembly">Assembly snapshot.</param>
        /// <param name="options">Solver options (mode, limits).</param>
        /// <param name="contacts">Optional pre-computed contacts.</param>
        /// <param name="constraints">Optional pre-computed constraints.</param>
        /// <returns>The solver result as <see cref="DgSolverModel"/>.</returns>
        public DgSolverModel BuildAndSolve(
            AssemblyModel assembly,
            SolverOptions options = default,
            ContactModel? contacts = null,
            ConstraintModel? constraints = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var detectionOptions = new DetectionOptions();
            var resolvedContacts = contacts ?? DetectContacts(assembly, detectionOptions);
            var resolvedConstraints = constraints ?? ConstraintModelFactory.CreateEmpty(assembly);
            var solverType = NormalizeSolverType(options.SolverType);
            var solver = _solverFactory(solverType);
            var normalizedOptions = options with { SolverType = solverType };

            return solver.Solve(assembly, resolvedContacts, resolvedConstraints, normalizedOptions);
        }

        /// <summary>
        /// Detects contacts for the supplied assembly.
        /// </summary>
        public ContactModel DetectContacts(AssemblyModel assembly, DetectionOptions options)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            options ??= new DetectionOptions();
            return ContactDetection.DetectContacts(assembly, options);
        }

        /// <summary>
        /// Exports a solver result into a robotic process schema.
        /// </summary>
        public ProcessSchema ExportProcess(DgSolverModel solverResult, ProcessExportOptions? options = null)
        {
            if (solverResult == null)
            {
                throw new ArgumentNullException(nameof(solverResult));
            }

            options ??= new ProcessExportOptions();
            var schema = ProcessSchema.FromSolverResult(solverResult, options);
            if (!string.IsNullOrWhiteSpace(options.OutputPath))
            {
                schema.WriteToDisk(options);
            }

            return schema;
        }

        /// <summary>
        /// Exports dataset JSON artifacts.
        /// </summary>
        public DatasetExportResult ExportDataset(
            AssemblyModel assembly,
            ContactModel contacts,
            DgSolverModel solverResult,
            DatasetExportOptions? options = null)
        {
            options ??= new DatasetExportOptions();
            return DatasetExporter.Export(assembly, contacts, solverResult, options);
        }

        /// <summary>
        /// Runs the ONNX inference stub for the provided request.
        /// </summary>
        public OnnxInferenceResult RunInference(OnnxInferenceRequest request)
        {
            return _inferenceService.Run(request);
        }

        private static ISolver ResolveDefaultSolver(SolverType solverType, ISolverBackend backend)
        {
            return solverType switch
            {
                SolverType.CSP => new CspSolver(backend),
                SolverType.SAT => new SatSolver(backend),
                SolverType.MILP => new MilpSolver(backend),
                _ => new CspSolver(backend)
            };
        }

        private static SolverType NormalizeSolverType(SolverType solverType)
        {
            return solverType == SolverType.Auto ? SolverType.CSP : solverType;
        }

        private static ISolverBackend ResolveDefaultBackend(SolverType _)
        {
            return new OrToolsBackend();
        }
    }
}
