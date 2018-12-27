using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Logging;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers.SqlServer;

namespace DominoEventStore.Providers
{
    public abstract class ASqlDbProvider:ISpecificDbStorage
    {
        private readonly IDbFactory _db;

        public const string CommitsTable = "ES_Commits";
        public const string SnapshotsTable = "ES_Snapshots";
        public const string BatchTable = "ES_Batch";
      
        protected ASqlDbProvider(IDbFactory db)
        {
            _db = db;

            db.Configuration.OnCommand = cmd => EventStore.Logger.Debug(cmd.FormatCommand());
            db.Configuration.OnException = (cmd,ex) => EventStore.Logger.Debug(ex,cmd.FormatCommand());
        }



        public ProcessedCommitsCount StartOrContinue(string name)
        {
            using (var db = _db.Create())
            {
                var skip = db.QueryValue(q =>
                    q.From<BatchProgress>().Where(d => d.Name == name).Select(d => d.Skip).MapTo<long?>());
                if (skip == null)
                {
                    skip = 0;
                    db.Insert(new BatchProgress() { Name = name });
                }
                return new ProcessedCommitsCount((int)skip.Value);
            }
        }

        public CommittedEvents GetNextBatch(ReadModelGenerationConfig config, ProcessedCommitsCount count)
        {
            using (var db = _db.Create())
            {
                var all = db.QueryAs(q => q
                    .FromAnonymous(new {Id = 1, TenantId = "", EntityId = Guid.Empty},
                       new TableName(CommitsTable, Schema)).Where(d => true)
                    .AndIf(() => config.EntityId.HasValue, d => d.EntityId == config.EntityId.Value)
                    .AndIf(() => !config.TenantId.IsNullOrEmpty(), d => d.TenantId == config.TenantId)
                    .OrderBy(d => d.Id)
                    .Limit(config.BatchSize, count.Value)
                    .SelectAll(useAsterisk: true).MapTo<Commit>()
                );
                return new CommittedEvents(all.ToArray());
            }
        }

        public CommittedEvents GetNextBatch(MigrationConfig config, ProcessedCommitsCount count)
        {
            using (var db = _db.Create())
            {
                var all = db.QueryAs(q => q
                    .FromAnonymous(new { Id = 1, TenantId = "", EntityId = Guid.Empty },
                       new TableName(CommitsTable, Schema)).Where(d => true)
                    
                    .AndIf(() => !config.TenantId.IsNullOrEmpty(), d => d.TenantId == config.TenantId)
                    .OrderBy(d => d.Id)
                    .Limit(config.BatchSize, count.Value)
                    .SelectAll(useAsterisk: true).MapTo<Commit>()
                );
                return new CommittedEvents(all.ToArray());
            }
        }

        public void UpdateProgress(string name, ProcessedCommitsCount processedCommits)
        {
            using (var db = _db.Create())
            {
                db.Update<BatchProgress>().Set(d => d.Skip, processedCommits.Value)
                    .Where(d => d.Name == name).Execute();
            }
        }
    

        public void MarkOperationAsEnded(string name)
        {
            using (var db = _db.Create())
            {
                db.DeleteFrom<BatchProgress>(d => d.Name == name);
            }
        }

        public async Task Append(params UnversionedCommit[] commits)
        {
            using (var db = await _db.CreateAsync(CancellationToken.None))
            {
                try
                {
                    using (var t = db.BeginTransaction())
                    {

                        foreach (var commit in commits)
                        {
                            var max=await db.QueryValueAsync<int?>(q =>
                                        q.From<Commit>().Where(d => d.EntityId == commit.EntityId && d.TenantId == commit.TenantId)
                                            .Select(d => d.Max(d.Version)).MapTo<int?>(),CancellationToken.None).ConfigureFalse()??0;
                            var com=new Commit(max+1,commit);
                            await db.InsertAsync(com, CancellationToken.None).ConfigureFalse();
                        }
                  
                        t.Commit();                   
                    }
                }

                catch (DbException ex) 
                {
                    if (ex.Message.Contains(DuplicateCommmitMessage))
                    {
                        throw new DuplicateCommitException();
                        //var existing = await db
                        //    .QueryRowAsync<Commit>(
                        //        q => q.From<Commit>()
                        //            .Where(d =>d.CommitId==commit.CommitId && d.EntityId == commit.EntityId && d.TenantId == commit.TenantId)
                        //            .Limit(1)
                        //            .SelectAll(useAsterisk: true), CancellationToken.None).ConfigureFalse();
                        //return new AppendResult(existing);
                    }

                    if (ex.Message.Contains(DuplicateVersion))
                    {
                        throw new ConcurrencyException();
                    }
                    throw;
                }
              
            }
        }

        protected virtual string DuplicateVersion => "Ver";

        protected virtual string DuplicateCommmitMessage { get; } = "Cid";
        public void Import(Commit commit)
        {
            using (var db = _db.Create())
            {
                try
                {
                    db.Insert(commit);
                }
                catch (DbException ex) when (_db.Provider.IsUniqueViolation(ex))
                {
                    //ignore duplicates
                }
            }
        }

        public async Task<Optional<EntityStreamData>> GetData(QueryConfig cfg, CancellationToken token)
        {
            using (var db = await _db.CreateAsync(token).ConfigureFalse())
            {
                var snapshot = Optional<Snapshot>.Empty;
                if (!cfg.IgnoreSnapshots) snapshot=await GetSnapshot(db,cfg).ConfigureFalse();
                var vers = snapshot.IsEmpty ? 1:snapshot.Value.Version+1;
                token.ThrowIfCancellationRequested();
                var all=await db.QueryAsAsync<Commit>(
                    q => q.From<Commit>()
                    .Where(d => d.TenantId == cfg.TenantId)
                    .AndIf(() => cfg.EntityId.HasValue, d => d.EntityId == cfg.EntityId.Value)
                    .And(d=>d.Version>=Math.Max(cfg.VersionStart,vers))
                    .AndIf(()=>cfg.DateEnd.HasValue,d=>d.Timestamp<=cfg.DateEnd)
                    .AndIf(()=>cfg.DateStart.HasValue,d=>d.Timestamp>=cfg.DateStart)
                    .AndIf(()=>cfg.VersionEnd.HasValue,d=>d.Version<=cfg.VersionEnd)
                    .OrderBy(d=>d.Version)
                    .SelectAll(useAsterisk:true)                                                            
                    , token).ConfigureFalse();
                var rez=new EntityStreamData();
                rez.LatestSnapshot = snapshot;
                rez.Commits = all;
                return rez.ToOptional();
            }
            
        }

        private async Task<Optional<Snapshot>> GetSnapshot(DbConnection db, QueryConfig cfg)
        {
            if(cfg.EntityId==null) return Optional<Snapshot>.Empty;
            var row = await db.QueryRowAsync<Snapshot>(
                q => q.From<Snapshot>()
                    .Where(d => d.EntityId == cfg.EntityId && d.TenantId == cfg.TenantId)
                    .OrderByDescending(d => d.Version)
                    .Limit(1)
                    .SelectAll(useAsterisk: true), CancellationToken.None).ConfigureFalse();
            return row == null ? Optional<Snapshot>.Empty :new Optional<Snapshot>(row);
        }

        public void InitStorage()
        {
            using (var db=_db.Create())
            {
                db.Execute(GetInitStorageSql(Schema));
            }
        }

        protected abstract string GetInitStorageSql(string schema);

        public string Schema { get; set; }
        public void ResetStorage()
        {
            using (var db = _db.Create())
            {
                db.DeleteFrom<Commit>();
                db.DeleteFrom<Snapshot>();                
            }            
        }

        public void DeleteTenant(string tenantId)
        {
            tenantId.MustNotBeEmpty();
            using (var db = _db.Create())
            {
                db.DeleteFrom<Commit>(d => d.TenantId == tenantId);
            }
        }

        public async Task Store(Snapshot snapshot)
        {
            using (var db = await _db.CreateAsync(CancellationToken.None))
            {
                var rez = await db.Update<Snapshot>().Set(d => d.SerializedData, snapshot.SerializedData)
                    .Set(d => d.Version, snapshot.Version)
                    .Where(d => d.EntityId == snapshot.EntityId && d.TenantId == snapshot.TenantId)
                    .ExecuteAsync(CancellationToken.None).ConfigureFalse();
                if (rez == 0)
                {
                    await db.InsertAsync(snapshot, CancellationToken.None);
                }

            }
        }

        public async Task DeleteSnapshot(Guid entityId, string tenantId, int? entityVersion = null)
        {
            using (var db = await _db.CreateAsync(CancellationToken.None))
            {
                if (entityVersion==null)
                await db.DeleteFromAsync<Snapshot>(CancellationToken.None,
                    d => d.EntityId == entityId && d.TenantId == tenantId).ConfigureFalse();
                else
                    await db.DeleteFromAsync<Snapshot>(CancellationToken.None,
                        d => d.EntityId == entityId && d.TenantId == tenantId && d.Version== entityVersion).ConfigureFalse();
            }
        }
    }
}