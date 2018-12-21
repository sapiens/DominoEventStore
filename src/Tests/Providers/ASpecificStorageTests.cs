using DominoEventStore;
using FluentAssertions;
using SqlFu;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Logging;
using Xunit;
using Xunit.Abstractions;
using Utils = DominoEventStore.Utils;

namespace Tests
{
    public abstract class ASpecificStorageTests:IDisposable
    {
        private readonly ITestOutputHelper _t;
        private readonly ISpecificDbStorage _store;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _src;


        protected ASpecificStorageTests(ITestOutputHelper t)
        {
          
            _t = t;
            _store = Setup.GetDbStorage(GetFactory(),t);
            _src=new CancellationTokenSource();
            _cancellationToken = _src.Token;
            _store.InitStorage();
        }

        protected abstract IDbFactory GetFactory();
        protected virtual void DisposeOther()
        {
            
        }

        //[Fact]
        //public async Task mini_benchmark()
        //{
        //    LogManager.OutputTo(f => { });
        //    var s = new Stopwatch();
        //    var arr = Enumerable.Range(1, 1000).Select(d => Setup.UnversionedCommit()).ToArray();

            
        //    s.Start();
        //    //Parallel.For(1, 1000, async (i, state) =>
        //    //{
        //    //    await _store.Append(arr[i - 1]);
        //    //});
        //    foreach (var g in arr) await _store.Append(g);
        //    s.Stop();
        //    _t.WriteLine($"1000 commits in {s.ElapsedMilliseconds}ms");
        //}

        [Fact]
        public async Task append_then_get_events_no_snapshot()
        {
            var commit1 = Setup.UnversionedCommit();
            var commit2 = Setup.UnversionedCommit(guid:commit1.EntityId);
            var commit3 = Setup.UnversionedCommit();
            await _store.Append(commit1);
            await _store.Append(commit2);
            await _store.Append(commit3);
            var data = await _store.GetData(Config(c=>c.OfEntity(commit1.EntityId)), _cancellationToken);
            var commits = data.Value.Commits.ToArray();
            commits.Length.Should().Be(2);
            commits[0].Version.Should().Be(1);
            commits[0].EventData.Should().Be(commit1.EventData);
            commits[1].Version.Should().Be(2);
            commits[1].EventData.Should().Be(commit2.EventData);
        }

        [Fact]
        public async Task append_then_get_events_with_snapshot()
        {
            var commit1 = Setup.UnversionedCommit();
            var commit2 = Setup.UnversionedCommit(guid: commit1.EntityId);
            var commit3 = Setup.UnversionedCommit(guid: commit1.EntityId);
            var snapshot = Setup.Snapshot(2, commit1.EntityId);

            await _store.Append(commit1);
            await _store.Append(commit2);
            await _store.Store(snapshot);
            await _store.Append(commit3);

            var data = await _store.GetData(Config(c => c.OfEntity(commit1.EntityId).IncludeSnapshots(true)), _cancellationToken);
            var commits = data.Value.Commits.ToArray();
            data.Value.LatestSnapshot.HasValue.Should().BeTrue();
            data.Value.LatestSnapshot.Value.Should().BeEquivalentTo(snapshot,i=>i.Excluding(d=>d.SnapshotDate));
            commits.Length.Should().Be(1);
            commits[0].Version.Should().Be(3);
        }


        [Fact]
        public async Task append_with_snapshot_0_events_after()
        {
            var commit1 = Setup.UnversionedCommit();
            var commit2 = Setup.UnversionedCommit(guid: commit1.EntityId);
            
            var snapshot = Setup.Snapshot(2, commit1.EntityId);
            await _store.Append(commit1);
            await _store.Append(commit2);
            await _store.Store(snapshot);
            

            var data = await _store.GetData(Config(c => c.OfEntity(commit1.EntityId).IncludeSnapshots(true)), _cancellationToken);
            var commits = data.Value.Commits.ToArray();
            data.Value.LatestSnapshot.HasValue.Should().BeTrue();
            data.Value.LatestSnapshot.Value.Should().BeEquivalentTo(snapshot,i=>i.Excluding(d=>d.SnapshotDate));
            commits.Length.Should().Be(0);            
        }

        [Fact]
        public async Task duplicate_commit_returns_stored_commit()
        {
            var commit1 = Setup.UnversionedCommit();
            await _store.Append(commit1);

            var result=await _store.Append(commit1);
            result.WasSuccessful.Should().BeFalse();
            result.DuplicateCommit.Should().BeEquivalentTo(new Commit(1,commit1),c=>c.Excluding(d=>d.Timestamp));            
        }

#if !IN_MEMORY
          [Fact]
        public async Task concurrency_Exception_when_trying_to_commit_with_an_existing_version()
        {
            if(Setup.IsAppVeyor) return;
            var commit = Setup.UnversionedCommit();
            var comm2 = Setup.UnversionedCommit(guid: commit.CommitId);
            await _store.Append(commit);
            _store.Import(new Commit(2, commit));
            try
            {
                await _store.Append(Setup.UnversionedCommit(guid: commit.CommitId));
                throw new Exception();
            }
            catch (ConcurrencyException ex)
            {
                
            }
            catch
            {
                throw new Exception();
            }

        }
#endif


        QueryConfig Config(Action<IConfigureQuery> cfg)
        {
            var c=new QueryConfig();
            cfg(c);
            return c;
        }

        [Fact]
        public async Task existing_snapshot_with_same_entity_version_is_replaced()
        {
            var snap = Setup.Snapshot(3, Guid.NewGuid());
            await _store.Store(snap);

            var snap1 = Setup.Snapshot(3, snap.EntityId);
            await _store.Store(snap1);

            var get = await _store.GetData(Config(c => c.OfEntity(snap.EntityId).IncludeSnapshots(true)),
                _cancellationToken);
            var mem2 = Utils.UnpackSnapshot(get.Value.LatestSnapshot.Value.SerializedData) as SomeMemento;
            var mem1 = Utils.UnpackSnapshot(snap1.SerializedData) as SomeMemento;
            mem2.Name.Should().Be(mem1.Name);
            mem2.IsOpen.Should().Be(mem1.IsOpen);            


        }

        [Fact]
        public async Task only_most_recent_snapshot_is_used()
        {
            var snap = Setup.Snapshot(3, Guid.NewGuid());
            await _store.Store(snap);

            var snap1 = Setup.Snapshot(4, snap.EntityId);
            await _store.Store(snap1);

            var get = await _store.GetData(Config(c => c.OfEntity(snap.EntityId).IncludeSnapshots(true)),
                _cancellationToken);
            var mem2 = Utils.UnpackSnapshot(get.Value.LatestSnapshot.Value.SerializedData) as SomeMemento;
            var mem1 = Utils.UnpackSnapshot(snap1.SerializedData) as SomeMemento;
            mem2.Name.Should().Be(mem1.Name);
            mem2.IsOpen.Should().Be(mem1.IsOpen);            
        }

        [Fact]
        public async Task delete_snapshot()
        {
            var snap = Setup.Snapshot(3, Guid.NewGuid());
            await _store.Append(Setup.UnversionedCommit(snap.TenantId, snap.EntityId));
            await _store.Store(snap);
            var get = await _store.GetData(Config(c => c.OfEntity(snap.EntityId).IncludeSnapshots(true)),
                _cancellationToken);
            get.Value.LatestSnapshot.Value.Should().BeEquivalentTo(snap,i=>i.Excluding(d=>d.SnapshotDate));
            await _store.DeleteSnapshot(snap.EntityId, snap.TenantId);
            get = await _store.GetData(Config(c => c.OfEntity(snap.EntityId).IncludeSnapshots(true)),
                _cancellationToken);
            get.Value.LatestSnapshot.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public async Task delete_specific_snapshot()
        {
            var snap = Setup.Snapshot(2, Guid.NewGuid());
            await _store.Append(Setup.UnversionedCommit(snap.TenantId, snap.EntityId));
            await _store.Store(snap);
            var snap1 = Setup.Snapshot(3, snap.EntityId, snap.TenantId);
            await _store.Store(snap1);
            
            await _store.DeleteSnapshot(snap.EntityId, snap.TenantId,snap.Version);
            var get = await _store.GetData(Config(c => c.OfEntity(snap.EntityId).IncludeSnapshots(true)),
                _cancellationToken);
            get.Value.LatestSnapshot.Value.Should().BeEquivalentTo(snap1,c=>c.Excluding(d=>d.SnapshotDate));

            await _store.DeleteSnapshot(snap.EntityId, snap.TenantId, snap1.Version);
            get = await _store.GetData(Config(c => c.OfEntity(snap.EntityId).IncludeSnapshots(true)),
                _cancellationToken);
            get.Value.LatestSnapshot.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void batch_start_returns_0()
        {
            _store.StartOrContinue(Guid.NewGuid().ToString()).Value.Should().Be(0);
        }

        [Fact]
        public void batch_continue_returns_savepoint()
        {
            var test =Guid.NewGuid().ToString();
            _store.StartOrContinue(test);
            _store.UpdateProgress(test,5);
            _store.StartOrContinue(test).Value.Should().Be(5);
        }

        [Fact]
        public void batch_ends()
        {
            var test = Guid.NewGuid().ToString();
            _store.StartOrContinue(test);
            _store.UpdateProgress(test, 5);
            _store.MarkOperationAsEnded(test);
            _store.StartOrContinue(test).Value.Should().Be(0);
        }

        [Fact]
        public async Task batch_get_next_for_read_model_all_events()
        {
            var entity = Guid.NewGuid();
            await _store.Append(Setup.UnversionedCommit(guid: entity));
            await _store.Append(Setup.UnversionedCommit(guid: entity));
            await _store.Append(Setup.UnversionedCommit("1"));

            var rm=new ReadModelGenerationConfig("test");
            
            var rez=_store.GetNextBatch(rm, 0);
            rez.IsEmpty.Should().BeFalse();
            var first = rez.GetNext().Value;
            first.EntityId.Should().Be(entity);
            first.TenantId.Should().Be("_");
            first.Version.Should().Be(1);

            var second = rez.GetNext().Value;
            second.EntityId.Should().Be(entity);
            second.TenantId.Should().Be("_");
            second.Version.Should().Be(2);

            var third = rez.GetNext().Value;
            third.EntityId.Should().NotBe(entity);
            third.TenantId.Should().Be("1");
            third.Version.Should().Be(1);

            rez.GetNext().IsEmpty.Should().BeTrue();
        }

        [Fact]
        public async Task batch_get_next_for_migration_all_events()
        {
            var entity = Guid.NewGuid();
            await _store.Append(Setup.UnversionedCommit(guid: entity));
            await _store.Append(Setup.UnversionedCommit(guid: entity));
            await _store.Append(Setup.UnversionedCommit("1"));

            var rm=new MigrationConfig("test");
            
            var rez=_store.GetNextBatch(rm, 0);
            rez.IsEmpty.Should().BeFalse();
            var first = rez.GetNext().Value;
            first.EntityId.Should().Be(entity);
            first.TenantId.Should().Be("_");
            first.Version.Should().Be(1);

            var second = rez.GetNext().Value;
            second.EntityId.Should().Be(entity);
            second.TenantId.Should().Be("_");
            second.Version.Should().Be(2);

            var third = rez.GetNext().Value;
            third.EntityId.Should().NotBe(entity);
            third.TenantId.Should().Be("1");
            third.Version.Should().Be(1);

            rez.GetNext().IsEmpty.Should().BeTrue();
        }

        public void Dispose()
        {
            _src.Cancel(true);
            _store.ResetStorage();
            DisposeOther();
        }
    }
}