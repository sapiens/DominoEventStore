using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace DominoEventStore
{
  public class StoreFacade:IStoreEvents,IWorkWithSnapshots, IAdvancedFeatures
  {
        private readonly ISpecificDbStorage _store;

      public void ImportCommit(Commit commit)
          => _store.Import(commit);

      private readonly EventStoreSettings _settings;

        public StoreFacade(ISpecificDbStorage store,EventStoreSettings settings)
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
        public IAdvancedFeatures Advanced => this;


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


      public void MigrateEventsTo(string name, IStoreEvents newStorage, Action<IConfigMigration> config = null)
      {
          name.MustNotBeEmpty();
          var conf = new MigrationConfig(name);
          config?.Invoke(conf);

         var rew=new EventsRewriter(conf.Converters,_settings.EventMappers);

         using (var operation = new BatchOperation(_store, conf))
          {
              Optional<Commit> commit;
              do
              {
                  commit = operation.GetNextCommit();
                  if (commit.HasValue)
                  {
                      newStorage.Advanced.ImportCommit(rew.Rewrite(commit.Value));
                  }
                  
              } while (commit.HasValue);


          }

   
      }

      public void ResetStorage() => _store.ResetStorage();

        public void DeleteTenant(string tenantId)
        {
            tenantId.MustNotBe(EventStore.DefaultTenant);
         _store.DeleteTenant(tenantId);
        }

      public void GenerateReadModel(string operationName, Action<dynamic> modelUpdater, Action<IConfigReadModelGeneration> config = null)
      {
            operationName.MustNotBeEmpty();
            var conf=new ReadModelGenerationConfig(operationName);
            config?.Invoke(conf);

          void HandleCommit(Commit commit,Action<dynamic> updater)
          {
              var evs = Utils.UnpackEvents(commit.Timestamp, commit.EventData, _settings.EventMappers);
              foreach (var ev in evs)
              {
                    updater((dynamic) ev);                    
              }
            }
          
         
            using (var operation = new BatchOperation(_store,conf))
            {
                Optional<Commit> commit;
                do
                {
                    commit = operation.GetNextCommit();
                    if (commit.HasValue)
                    {
                        HandleCommit(commit.Value, modelUpdater);
                    }
                } while (commit.HasValue);


            }
          
        }
  }

    class EventsRewriter
    {
        private readonly IReadOnlyDictionary<Type, IMapEventDataToObject> _mapps;

        public EventsRewriter(IEnumerable<IRewriteEventData> rewrites, IReadOnlyDictionary<Type, IMapEventDataToObject> mapps)
        {
            _mapps = CreateMappersFromRewriters(rewrites, mapps);
        }
        public Commit Rewrite(Commit commit)
        {
            var evs = Utils.UnpackEvents(commit.Timestamp, commit.EventData, _mapps);
            return new Commit(commit.TenantId,commit.EntityId,Utils.PackEvents(evs),commit.CommitId,commit.Timestamp,commit.Version);
        }

        Dictionary<Type, IMapEventDataToObject> CreateMappersFromRewriters(IEnumerable<IRewriteEventData> rew, IReadOnlyDictionary<Type, IMapEventDataToObject> mapps)
        {
            var rez = new Dictionary<Type, IMapEventDataToObject>();
            foreach (var r in rew)
            {

                if (mapps.ContainsKey(r.HandledType))
                {
                    rez.Add(r.HandledType,new LambdaMap(r.HandledType,mapps[r.HandledType],r));
                }
                else
                {
                    rez.Add(r.HandledType, new LambdaMap(r.HandledType, rew: r));
                }
            }

            foreach (var kv in _mapps.Where(d => !rez.ContainsKey(d.Key)))
            {
                rez.Add(kv.Key,kv.Value);
            }
            return rez;
        }

        class LambdaMap : IMapEventDataToObject
        {
            private readonly Type _type;
            private readonly IMapEventDataToObject _mapr;
            private readonly IRewriteEventData _rew;


            public LambdaMap(Type type,IMapEventDataToObject mapr=null,IRewriteEventData rew=null)
            {
                _type = type;
                _mapr = mapr;
                _rew = rew;
            }

            public bool Handles(Type type)
                => type == _type;

            public object Map(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate)
            {
                var rez= _mapr?.Map(jsonData, deserializedEvent, commitDate)??deserializedEvent;
                return _rew?.Rewrite(jsonData, rez, commitDate) ?? rez;
            }
        }
    }
}