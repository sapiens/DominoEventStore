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

        public async Task Append(string tenantId, Guid entityId, Guid commitId, params object[] events)
        {
            tenantId.MustNotBeEmpty();
            entityId.MustNotBeDefault();
            commitId.MustNotBeDefault();
            if (events.IsNullOrEmpty()) return ;

            var commit=new UnversionedCommit(tenantId,entityId,Utils.PackEvents(events),commitId,DateTimeOffset.Now);
            var dbgInfo = new{tenantId,entityId,commitId};
            _settings.Logger.Debug("Appending {@commit} with events {@events}",dbgInfo,events);
            var rez= await _store.Append(commit);
            if (rez.WasSuccessful)
            {
                _settings.Logger.Debug("Append succesful for commit {@commit}",dbgInfo);
                return;
            }
            throw new DuplicateCommitException(commitId,Utils.UnpackEvents(commit.Timestamp,commit.EventData,_settings.EventMappers));
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
                    commits.SelectMany(d =>
                        Utils.UnpackEvents(d.Timestamp, d.EventData, _settings.EventMappers)).ToArray();
            return new EntityEvents(GetEvents(raw.Commits),raw.Commits.Max(d=>d.Version),GetSnapshot(raw.LatestSnapshot.ValueOr(null)?.SerializedData));
            
        }


        public IWorkWithSnapshots Snapshots => this;
        public IAdvancedFeatures Advanced => this;


        public async Task Store(int entityVersion, Guid entityId, object memento, string tenantId = EventStore.DefaultTenant)
        {
            var snapshot=new Snapshot(entityVersion,entityId,tenantId,Utils.PackSnapshot(memento),DateTimeOffset.Now);
            _settings.Logger.Debug("Storing snapshot {@snapshot}",snapshot);
            await _store.Store(snapshot).ConfigureFalse();

            _settings.Logger.Debug("Snapshot for {@entity} stored successfully",new{entityId,tenantId,entityVersion});
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
            _settings.Logger.Debug("Getting events with query {@query}",new{config.TenantId,config.EntityId,config.IgnoreSnapshots,config.DateStart,config.DateEnd});
            var raw = await _store.GetData(config, token ?? CancellationToken.None).ConfigureFalse();
            var dbg = new{config.TenantId,config.EntityId};
            if (raw.HasValue)
            {
                var events = ConvertToEntityEvents(raw.Value);
                _settings.Logger.Debug("Query for {@entity} returned "+events.Count+" events",dbg);
                
                return new Optional<EntityEvents>(events);
            }
            else
            {                
                _settings.Logger.Debug("Query for {@entity} returned empty",dbg);
                return Optional<EntityEvents>.Empty;
            }
        }


      public void MigrateEventsTo(IStoreEvents newStorage, string name, Action<IConfigMigration> config = null)
      {
          name.MustNotBeEmpty();
          var conf = new MigrationConfig(name);
          config?.Invoke(conf);
          var l = _settings.Logger;

         var rew=new EventsRewriter(conf.Converters,_settings.EventMappers);
            l.Debug("Starting store migration with batch operation: {name}",name);
         using (var operation = new BatchOperation(_store, conf))
          {              
              Optional<Commit> commit;
              do
              {
                  commit = operation.GetNextCommit();
                  if (commit.HasValue)
                  {
                      l.Debug("Importing commit {commit}",commit.Value.CommitId);
                      newStorage.Advanced.ImportCommit(rew.Rewrite(commit.Value));
                  }
                  
              } while (commit.HasValue);


          }
          l.Debug("Migration {name} completed",name);
   
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
}