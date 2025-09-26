using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Shared base class providing duplicated logic for all custom Goo wrappers.
    /// </summary>
    /// <typeparam name="T">Underlying value type.</typeparam>
    public abstract class AcGhGooBase<T> : GH_Goo<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcGhGooBase{T}"/> class.
        /// </summary>
        protected AcGhGooBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcGhGooBase{T}"/> class with the specified value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        protected AcGhGooBase(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new wrapper instance using the provided value.
        /// </summary>
        /// <param name="value">Value to wrap.</param>
        /// <returns>New wrapper instance.</returns>
        protected abstract AcGhGooBase<T> CreateInstance(T value);

        /// <summary>
        /// Creates an empty wrapper instance.
        /// </summary>
        /// <returns>Empty wrapper.</returns>
        protected abstract AcGhGooBase<T> CreateEmpty();

        /// <inheritdoc />
        public override IGH_Goo Duplicate()
        {
            return Value == null ? CreateEmpty() : CreateInstance(Value);
        }

        /// <inheritdoc />
        public override bool IsValid => Value != null;

        /// <inheritdoc />
        public override string ToString()
        {
            return Value?.ToString() ?? $"Null {TypeName}";
        }
    }
}
