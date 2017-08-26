using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DominoEventStore
{
    /// <summary>
    /// This should be treated as a singleton
    /// </summary>
    public interface IStoreEvents
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="commitId"></param>
        /// <param name="events"></param>
        
        /// <returns></returns>
        Task Append(Guid entityId,Guid commitId,params object[] events);
        Task Append(string tenantId,Guid entityId,Guid commitId,params object[] events);

        Task<IReadOnlyCollection<object>> GetEvents(Guid entityId);
        
        //Task<IReadOnlyCollection<object>> GetAllCommittedEvents(string tenantId=EventStore.DefaultTenant);

        

    }
}