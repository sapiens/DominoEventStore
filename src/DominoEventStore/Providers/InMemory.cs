﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore.Providers
{
    public class InMemory:ISpecificDbStorage
    {
        object _sync=new object();

        Dictionary<string,int> _batch=new Dictionary<string, int>();

        public ProcessedCommitsCount StartOrContinue(string name)
        {
            return _batch.GetValueOrCreate(name,()=>0);
        }

        public CommittedEvents GetNextBatch(ReadModelGenerationConfig config, ProcessedCommitsCount count)
        {
            IEnumerable<Commit> all=_commits;
            if (!config.TenantId.IsNullOrEmpty()) all = all.Where(d => d.TenantId == config.TenantId);
            if (config.EntityId.HasValue) all = all.Where(d => d.EntityId == config.EntityId.Value);
            all = all.OrderBy(d => d.Timestamp);
            return new CommittedEvents(all.ToArray());
        }

        public string Schema { get; set; } = "";

        public CommittedEvents GetNextBatch(MigrationConfig config, ProcessedCommitsCount count)
        {
            IEnumerable<Commit> all = _commits;
            if (!config.TenantId.IsNullOrEmpty()) all = all.Where(d => d.TenantId == config.TenantId);            
            all = all.OrderBy(d => d.Timestamp);
            return new CommittedEvents(all.ToArray());
        }

        public void UpdateProgress(string name, ProcessedCommitsCount processedCommits)
        {
            _batch[name] = processedCommits;
        }

        public void MarkOperationAsEnded(string name)
        {
            _batch.Remove(name);
        }


        List<Commit> _commits=new List<Commit>();

        public Task Append(params UnversionedCommit[] commits)
        {
            lock (_sync)
            {
                foreach (var commit in commits)
                {
                    var all = _commits.Where(d => d.TenantId == commit.TenantId && d.EntityId == commit.EntityId);
                    var dup = all.FirstOrDefault(d => d.CommitId == commit.CommitId);
                    if (dup != null)
                    {
                        return Task.FromResult(new AppendResult(dup));
                    }
                    var max=!all.Any()?0:all.Max(d => d.Version);
                    var c=new Commit(max+1,commit);
                    _commits.Add(c);
                }
               
            }

            return Task.CompletedTask;
        }

        public void Import(Commit commit)
        {
            var all = _commits.Where(d => d.TenantId == commit.TenantId && d.EntityId == commit.EntityId);
            var dup = all.FirstOrDefault(d => d.CommitId == commit.CommitId);
            if (dup!=null) return;
            _commits.Add(commit);
        }

        public Task<Optional<EntityStreamData>> GetData(QueryConfig cfg, CancellationToken token)
        {
            var esd=new EntityStreamData();
            if (!cfg.IgnoreSnapshots) esd.LatestSnapshot = GetSnapshot(cfg);
            var ver = esd.LatestSnapshot.IsEmpty ? 1 : esd.LatestSnapshot.Value.Version + 1;
            cfg.VersionStart = Math.Max(cfg.VersionStart, ver);
            cfg.VersionEnd = cfg.VersionEnd ?? int.MaxValue;
            lock (_sync)
            {
                esd.Commits =
                    _commits.Where(d => d.TenantId == cfg.TenantId).Where(d =>
                        {

                            if (cfg.EntityId.HasValue) return d.EntityId == cfg.EntityId;
                            return true;
                        }).Where(d => d.Version >= cfg.VersionStart && d.Version <= cfg.VersionEnd)
                        .Where(d => d.Timestamp >= (cfg.DateStart ?? DateTimeOffset.MinValue))
                        .Where(d => d.Timestamp <= (cfg.DateEnd ?? DateTimeOffset.MaxValue))
                        .OrderBy(d=>d.Version)
                        .ToArray();
            }

            return Task.FromResult(esd.ToOptional());
        }

        private Optional<Snapshot> GetSnapshot(QueryConfig cfg)
        {
            cfg.EntityId.MustNotBeDefault();
            lock (_sync)
            {
                var all = _snapshots.GetValueOrDefault(cfg.EntityId.Value,new List<Snapshot>()).ToArray();
                if (!all.Any()) return Optional<Snapshot>.Empty;
                var snapshot = all.OrderByDescending(d => d.Version).FirstOrDefault();
                return snapshot==null?Optional<Snapshot>.Empty:new Optional<Snapshot>(snapshot);
            }
        }

        public void InitStorage()
        {
          
        }

        public void ResetStorage()
        {
            _commits.Clear();
            _snapshots.Clear();
        }

        public void DeleteTenant(string tenantId)
        {
            lock (_sync)
            {
                _commits.RemoveAll(d => d.TenantId == tenantId);
            }
        }
        Dictionary<Guid,List<Snapshot>> _snapshots=new Dictionary<Guid, List<Snapshot>>();
        public Task Store(Snapshot snapshot)
        {
            lock (_sync)
            {
                var arr = _snapshots.GetValueOrCreate(snapshot.EntityId, () => new List<Snapshot>());
                if (arr.Any(d => d.Version == snapshot.Version))
                {
                    arr.RemoveAll(d => d.Version == snapshot.Version);
                }
                arr.Add(snapshot);
            }
            
            return Task.CompletedTask;
        }

        public Task DeleteSnapshot(Guid entityId, string tenantId, int? entityVersion = null)
        {
            lock (_sync)
            {
               
                if (entityVersion == null)
                {
                    _snapshots.Remove(entityId);

                }
                else
                {
                     var snaps = _snapshots.GetValueOrDefault(entityId, new List<Snapshot>());
                     snaps.RemoveAll(d => d.Version == entityVersion);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}