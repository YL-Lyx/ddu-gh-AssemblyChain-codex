using System;

namespace AssemblyChain.Core.Toolkit.Processing
{
    /// <summary>
    /// Represents a diagnostic message emitted while processing geometry or assembly data.
    /// </summary>
    public readonly struct ProcessingMessage
    {
        public ProcessingMessage(ProcessingMessageLevel level, string text)
        {
            Level = level;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Message severity.
        /// </summary>
        public ProcessingMessageLevel Level { get; }

        /// <summary>
        /// Message text.
        /// </summary>
        public string Text { get; }
    }

    /// <summary>
    /// Severity levels for processing diagnostics.
    /// </summary>
    public enum ProcessingMessageLevel
    {
        Remark,
        Warning,
        Error
    }
}
