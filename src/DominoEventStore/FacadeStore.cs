using System;
using System.Collections.Generic;
using System.Linq;
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
      
             
        public Task Store(int entityVersion, Guid entityId, object memento, string tenantId = EventStore.DefaultTenant)
        {
            var snapshot=new Snapshot(entityVersion,entityId,tenantId,Utils.PackSnapshot(memento),DateTimeOffset.Now);
            return _store.Store(snapshot);
            
        }

        public Task Delete(Guid entityId, int entityVersion, string tenantId = EventStore.DefaultTenant)
            => _store.DeleteSnapshot(entityId,tenantId,entityVersion);

        /// <summary>
        /// Deletes all stored snapshots for entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public Task DeleteAll(Guid entityId, string tenantId = EventStore.DefaultTenant)
            => _store.DeleteSnapshot(entityId, tenantId);

    
        public async Task<Optional<EntityEvents>> GetEvents(Action<IConfigureQuery> advancedConfig, CancellationToken? token = null)
        {
            var config = new QueryConfig();

            advancedConfig(config);
            var raw = await _store.GetData(config, token ?? CancellationToken.None).ConfigureFalse();
            return raw.HasValue ? new Optional<EntityEvents>(ConvertToEntityEvents(raw.Value)) : Optional<EntityEvents>.Empty;
        }
    }
}