using System;
using System.Threading;
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

        IStoreUnitOfWork StartCommit(Guid id);

        /// <summary>
        /// Gets all the events from the beginning until present unless overriden. 
        /// For performance reasons, by default it doesn't check for snapshots.
        /// You have to enable those explicitly
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="tenantId"></param>
        /// <param name="token"></param>
        /// <param name="includeSnapshots"></param>
        /// <returns></returns>
        Task<Optional<EntityEvents>> GetEvents(Guid entityId, string tenantId = EventStore.DefaultTenant, CancellationToken? token = null, bool includeSnapshots = false);
        /// <summary>
        /// Customize your query a bit more, if you need more finesse
        /// </summary>
        /// <param name="advancedConfig"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Optional<EntityEvents>> GetEvents(Action<IConfigureQuery> advancedConfig, CancellationToken? token = null);
        
        IWorkWithSnapshots Snapshots { get; }
        IAdvancedFeatures Advanced { get; }
    }
}