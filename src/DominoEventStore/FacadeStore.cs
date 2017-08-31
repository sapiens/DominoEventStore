using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


namespace DominoEventStore
{
  public class FacadeStore:IStoreEvents,IWorkWithSnapshots
    {
        private readonly ISpecificDbStorage _store;
        private readonly EventStoreSettings _settings;

        public FacadeStore(ISpecificDbStorage store,EventStoreSettings settings)
        {
            _store = store;
            _settings = settings;
        }

        public Task Append(Guid entityId, Guid commitId, params object[] events)
            => Append(EventStore.DefaultTenant, entityId, commitId, events);

        public Task Append(string tenantId, Guid entityId, Guid commitId, params object[] events)
        {
            tenantId.MustNotBeEmpty();
            entityId.MustNotBeDefault();
            commitId.MustNotBeDefault();
            if (events.IsNullOrEmpty()) return Task.CompletedTask;

            var commit=new UnversionedCommit(tenantId,entityId,Utils.PackEvents(events),commitId,DateTimeOffset.Now);
            return _store.Append(commit);
        }

        public Task<Optional<EntityEvents>> GetEvents(Guid entityId, string tenantId = EventStore.DefaultTenant,
            CancellationToken? token = null, bool includeSnapshots = false)
            => GetEvents(g => g.OfTenant(tenantId).OfEntity(entityId).IncludeSnapshots(includeSnapshots), token);

        private EntityEvents ConvertToEntityEvents(EntityStreamData raw)
        {
            Optional<object> GetSnapshot(string sData)
            {
                if (sData.IsNullOrEmpty()) return Optional<object>.Empty;
                return new Optional<object>(Utils.UnpackSnapshot(sData));
            }

            IReadOnlyCollection<object> GetEvents(IEnumerable<Commit> commits)
                =>
                    commits.OrderBy(d => d.Version).SelectMany(d =>
                        Utils.UnpackEvents(d.Timestamp, d.EventData, _settings.EventMappers)).ToArray();
            return new EntityEvents(GetEvents(raw.Commits),raw.Commits.Max(d=>d.Version),GetSnapshot(raw.LatestSnapshot.ValueOr(null)?.SerializedData));
            
        }


        public IWorkWithSnapshots Snapshots => this;
      
             
        public Task Store(int entityVersion, Guid entityId, object snapshot, string tenantId = EventStore.DefaultTenant)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int entityVersion, Guid entityId, string tenantId = EventStore.DefaultTenant)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAll(Guid entityId, string tenantId = EventStore.DefaultTenant)
        {
            throw new NotImplementedException();
        }

    
        public async Task<Optional<EntityEvents>> GetEvents(Action<IConfigureQuery> advancedConfig, CancellationToken? token = null)
        {
            var config = new QueryConfig();

            advancedConfig(config);
            var raw = await _store.GetData(config, token ?? CancellationToken.None).ConfigureFalse();
            return raw.HasValue ? new Optional<EntityEvents>(ConvertToEntityEvents(raw.Value)) : Optional<EntityEvents>.Empty;
        }
    }

    public interface ISpecificDbStorage
    {
        /// <summary>
        /// Should calculate the version of the entity and use that to detect concurrency problems
        /// </summary>
        /// <param name="commit"></param>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <exception cref="ConcurrencyException"></exception>
        /// <returns></returns>
        Task Append(UnversionedCommit commit);
        /// <summary>
        /// Adds the commit as is
        /// </summary>
        /// <param name="commit"></param>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <returns></returns>
        Task Append(Commit commit);

       Task<Optional<EntityStreamData>> GetData(QueryConfig cfg, CancellationToken token);
      /// <summary>
        /// Creates the tables in the specified/default schema
        /// </summary>
        /// <param name="schema"></param>
        void InitStorage(string schema = null);
        Task ResetStorage();
        Task DeleteTenant(string tenantId);
    }

    public class EntityStreamData
    {
        public Optional<Snapshot> LatestSnapshot { get; set; }
        public IEnumerable<Commit> Commits { get; set; }=Enumerable.Empty<Commit>();
    }

    public class UnversionedCommit
    {
        public UnversionedCommit(string tenantId, Guid entityId, string eventData, Guid commitId, DateTimeOffset timestamp)
        {
            tenantId.MustNotBeEmpty();
            entityId.MustNotBeDefault();
            eventData.MustNotBeEmpty();
            commitId.MustNotBeDefault();
            timestamp.MustNotBeDefault();


            TenantId = tenantId;
            EntityId = entityId;
            EventData = eventData;
            CommitId = commitId;
            Timestamp = timestamp;            
        }

        protected UnversionedCommit()
        {
            
        }

        public Guid CommitId { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public string TenantId { get; private set; }
        public Guid EntityId { get; private set; }
        public string EventData { get; private set; }
    }

    public class Commit:UnversionedCommit
    {
       public Commit()
        {
            
        }

        public Commit(string tenantId, Guid entityId, string eventData, Guid commitId, DateTimeOffset timestamp,int version) : base(tenantId, entityId, eventData, commitId, timestamp)
        {
            Version = version;
        }

        /// <summary>
        /// Entity version
        /// </summary>
        public int Version { get; private set; }
              
    }
}