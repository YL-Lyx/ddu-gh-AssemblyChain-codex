using System.Collections.Generic;
using System.Threading.Tasks;
using AssemblyChain.Core.Domain.Entities;

namespace AssemblyChain.Core.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Part entities
    /// </summary>
    public interface IPartRepository
    {
        /// <summary>
        /// Gets a part by ID
        /// </summary>
        Task<Part> GetByIdAsync(int id);

        /// <summary>
        /// Gets all parts
        /// </summary>
        Task<IEnumerable<Part>> GetAllAsync();

        /// <summary>
        /// Gets parts by assembly ID
        /// </summary>
        Task<IEnumerable<Part>> GetByAssemblyIdAsync(int assemblyId);

        /// <summary>
        /// Gets parts with physics properties
        /// </summary>
        Task<IEnumerable<Part>> GetPhysicsPartsAsync();

        /// <summary>
        /// Adds a new part
        /// </summary>
        Task AddAsync(Part part);

        /// <summary>
        /// Updates an existing part
        /// </summary>
        Task UpdateAsync(Part part);

        /// <summary>
        /// Deletes a part by ID
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Checks if a part exists
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Gets the next available ID
        /// </summary>
        Task<int> GetNextIdAsync();
    }
}
