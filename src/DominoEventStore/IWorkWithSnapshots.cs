using System;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface IWorkWithSnapshots
    {
        /// <summary>
        /// If a snapshot for the same version exists, it will be replaced
        /// </summary>
        /// <param name="entityVersion">Snapshot represents this version of the entity state</param>
        /// <param name="entityId"></param>
        /// <param name="memento"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        Task Store(int entityVersion, Guid entityId, object memento, string tenantId = EventStore.DefaultTenant);

        Task Delete(Guid entityId, int entityVersion, string tenantId = EventStore.DefaultTenant);
        Task DeleteAll(Guid entityId, string tenantId = EventStore.DefaultTenant);
    }
}