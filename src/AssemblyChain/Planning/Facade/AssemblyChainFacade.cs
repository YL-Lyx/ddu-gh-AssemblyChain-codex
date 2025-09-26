using System;
using System.Threading;
using System.Threading.Tasks;
using AssemblyChain.Geometry.Contact;
using AssemblyChain.IO.Contracts;
using AssemblyChain.IO.Data;
using AssemblyChain.Analysis.Learning;
using AssemblyChain.Planning.Model;
using AssemblyChain.Robotics;
using AssemblyChain.Planning.Solver;
using AssemblyChain.Planning.Solver.Backends;

namespace AssemblyChain.Planning.Facade
{
    /// <summary>
    /// Facade orchestrating contact detection, sequence planning, robotic export and learning hooks.
    /// </summary>
    public sealed class AssemblyChainFacade
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
        /// Runs the complete planning pipeline asynchronously to avoid blocking the UI thread.
        /// </summary>
        /// <param name="request">Request payload.</param>
        /// <param name="cancellationToken">Cancellation token used to abort the computation.</param>
        /// <returns>A task that represents the asynchronous planning operation.</returns>
        public Task<AssemblyPlanResult> RunPlanAsync(AssemblyPlanRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return RunPlan(request);
            }, cancellationToken);
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
        /// Builds constraint data and solves the planning problem asynchronously.
        /// </summary>
        /// <param name="assembly">Assembly snapshot.</param>
        /// <param name="options">Solver options (mode, limits).</param>
        /// <param name="contacts">Optional pre-computed contacts.</param>
        /// <param name="constraints">Optional pre-computed constraints.</param>
        /// <param name="cancellationToken">Cancellation token used to abort the computation.</param>
        /// <returns>A task that represents the asynchronous solver operation.</returns>
        public Task<DgSolverModel> BuildAndSolveAsync(
            AssemblyModel assembly,
            SolverOptions options = default,
            ContactModel? contacts = null,
            ConstraintModel? constraints = null,
            CancellationToken cancellationToken = default)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return BuildAndSolve(assembly, options, contacts, constraints);
            }, cancellationToken);
        }

        /// <summary>
        /// Detects contacts for the supplied assembly.
        /// </summary>
        /// <param name="assembly">Assembly snapshot to analyze.</param>
        /// <param name="options">Detection options controlling contact search strategy.</param>
        /// <returns>The computed contact model.</returns>
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
        /// <param name="solverResult">Solver output to transform into a process schema.</param>
        /// <param name="options">Optional export customization options.</param>
        /// <returns>A process schema ready for downstream robotic execution.</returns>
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
        /// Exports a solver result into a robotic process schema asynchronously.
        /// </summary>
        /// <param name="solverResult">Solver output to transform into a process schema.</param>
        /// <param name="options">Optional export customization options.</param>
        /// <param name="cancellationToken">Cancellation token used to abort the export.</param>
        /// <returns>A task that represents the asynchronous export operation.</returns>
        public Task<ProcessSchema> ExportProcessAsync(
            DgSolverModel solverResult,
            ProcessExportOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (solverResult == null)
            {
                throw new ArgumentNullException(nameof(solverResult));
            }

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ExportProcess(solverResult, options);
            }, cancellationToken);
        }

        /// <summary>
        /// Exports dataset JSON artifacts.
        /// </summary>
        /// <param name="assembly">Assembly snapshot associated with the dataset.</param>
        /// <param name="contacts">Contact model describing part interactions.</param>
        /// <param name="solverResult">Solver output to include in the dataset.</param>
        /// <param name="options">Optional dataset export parameters.</param>
        /// <returns>A result object describing the exported dataset files.</returns>
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
        /// <param name="request">Inference request payload.</param>
        /// <returns>The inference result.</returns>
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
