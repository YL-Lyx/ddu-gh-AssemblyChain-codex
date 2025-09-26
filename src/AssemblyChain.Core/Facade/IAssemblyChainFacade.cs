using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Contracts;
using AssemblyChain.Core.Model;
using AssemblyChain.Core.Solver;

namespace AssemblyChain.Core.Facade
{
    /// <summary>
    /// Defines the contract for the AssemblyChain facade orchestrating planning operations.
    /// </summary>
    public interface IAssemblyChainFacade
    {
        /// <summary>
        /// Gets the contact utilities used by the facade.
        /// </summary>
        IContactUtils ContactUtils { get; }

        /// <summary>
        /// Runs the complete planning pipeline given a structured request.
        /// </summary>
        AssemblyPlanResult RunPlan(AssemblyPlanRequest request);

        /// <summary>
        /// Builds constraint data and solves the planning problem with the configured backend.
        /// </summary>
        DgSolverModel BuildAndSolve(AssemblyModel assembly, SolverOptions options = default, ContactModel? contacts = null, ConstraintModel? constraints = null);

        /// <summary>
        /// Detects contacts for the supplied assembly.
        /// </summary>
        ContactModel DetectContacts(AssemblyModel assembly, DetectionOptions options);

        /// <summary>
        /// Exports a solver result into a robotic process schema.
        /// </summary>
        ProcessSchema ExportProcess(DgSolverModel solverResult, ProcessExportOptions? options = null);

        /// <summary>
        /// Exports dataset JSON artifacts.
        /// </summary>
        DatasetExportResult ExportDataset(AssemblyModel assembly, ContactModel contacts, DgSolverModel solverResult, DatasetExportOptions? options = null);

        /// <summary>
        /// Runs the ONNX inference stub for the provided request.
        /// </summary>
        OnnxInferenceResult RunInference(OnnxInferenceRequest request);
    }
}
