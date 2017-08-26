using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public class FacadeStore:IStoreEvents
    {
        private readonly ISpecificDbStorage _store;

        public FacadeStore(ISpecificDbStorage store)
        {
            _store = store;
        }

        public Task Append(Guid entityId, Guid commitId, params object[] events)
        {
            throw new NotImplementedException();
        }

        public Task Append(string tenantId, Guid entityId, Guid commitId, params object[] events)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<object>> GetEvents(Guid entityId)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISpecificDbStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commit"></param>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <exception cref="ConcurrencyException"></exception>
        /// <returns></returns>
        Task Append(Commit commit);

        Task<IEnumerable<Commit>> GetCommits(QueryConfig cfg,CancellationToken token);
        /// <summary>
        /// Creates the tables in the specified/default schema
        /// </summary>
        /// <param name="schema"></param>
        void InitStorage(string schema = null);
        Task ResetStorage();
        Task DeleteTenant(string tenantId);
    }

    public class Commit
    {
        public Commit(string tenantId, Guid entityId, string eventData, Guid commitId,DateTimeOffset timestamp,int version)
        {
            tenantId.MustNotBeEmpty();
            entityId.MustNotBeDefault();
            eventData.MustNotBeEmpty();
            commitId.MustNotBeDefault();
            timestamp.MustNotBeDefault();
            version.Must(v=>v>0);

            TenantId = tenantId;
            EntityId = entityId;
            EventData = eventData;
            CommitId = commitId;
            Timestamp = timestamp;
            Version = version;
        }

        /// <summary>
        /// Entity version
        /// </summary>
        public int Version { get; private set; }
        
        public Guid CommitId { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public string TenantId { get; private set; }
        public Guid EntityId { get; private set; }
        public string EventData { get; private set; }
    }
}