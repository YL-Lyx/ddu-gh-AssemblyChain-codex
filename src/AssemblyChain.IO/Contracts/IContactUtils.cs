// 改造目的：隔离接触转换工具，供 Facade 与检测管线使用。
// 兼容性注意：保留旧 ContactData 工厂方法，外部可通过依赖注入替换实现。
using System.Collections.Generic;

namespace AssemblyChain.IO.Contracts
{
    /// <summary>
    /// Provides helper operations to transform narrow phase output into domain contacts.
    /// </summary>
    public interface IContactUtils
    {
        /// <summary>
        /// Creates contact data records from detected zones.
        /// </summary>
        /// <param name="partAId">Identifier of the first part.</param>
        /// <param name="partBId">Identifier of the second part.</param>
        /// <param name="zones">Validated contact zones.</param>
        /// <param name="type">Contact type descriptor.</param>
        /// <returns>A list of contact data objects.</returns>
        IReadOnlyList<ContactData> CreateContacts(
            string partAId,
            string partBId,
            IEnumerable<ContactZone> zones,
            ContactType type);
    }
}
