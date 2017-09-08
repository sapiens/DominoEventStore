using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoEventStore;
using FluentAssertions;
using Xunit;

namespace Tests
{
    public abstract class ASpecificStorageTests
    {
        private readonly ISpecificDbStorage _store;

        protected ASpecificStorageTests(ISpecificDbStorage store)
        {
            _store = store;
        }
        [Fact]
        public async Task append_then_get_events_no_snapshot()
        {
            var commit1 = Setup.UnversionedCommit();
            var commit2 = Setup.UnversionedCommit(guid:commit1.EntityId);
            var commit3 = Setup.UnversionedCommit();
            await _store.Append(commit1, Setup.EventDeserializerWIthoutUpcasting());
            await _store.Append(commit2, Setup.EventDeserializerWIthoutUpcasting());
            await _store.Append(commit3, Setup.EventDeserializerWIthoutUpcasting());
            var data = await _store.GetData(Config(c=>c.OfEntity(commit1.EntityId)), CancellationToken.None);
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

            await _store.Append(commit1, Setup.EventDeserializerWIthoutUpcasting());
            await _store.Append(commit2, Setup.EventDeserializerWIthoutUpcasting());
            await _store.Store(snapshot);
            await _store.Append(commit3, Setup.EventDeserializerWIthoutUpcasting());

            var data = await _store.GetData(Config(c => c.OfEntity(commit1.EntityId).IncludeSnapshots(true)), CancellationToken.None);
            var commits = data.Value.Commits.ToArray();
            data.Value.LatestSnapshot.HasValue.Should().BeTrue();
            data.Value.LatestSnapshot.Value.ShouldBeEquivalentTo(snapshot);
            commits.Length.Should().Be(1);
            commits[0].Version.Should().Be(3);
        }


        [Fact]
        public async Task append_with_snapshot_0_events_after()
        {
            var commit1 = Setup.UnversionedCommit();
            var commit2 = Setup.UnversionedCommit(guid: commit1.EntityId);
            
            var snapshot = Setup.Snapshot(2, commit1.EntityId);
            await _store.Append(commit1, Setup.EventDeserializerWIthoutUpcasting());
            await _store.Append(commit2, Setup.EventDeserializerWIthoutUpcasting());
            await _store.Store(snapshot);
            

            var data = await _store.GetData(Config(c => c.OfEntity(commit1.EntityId).IncludeSnapshots(true)), CancellationToken.None);
            var commits = data.Value.Commits.ToArray();
            data.Value.LatestSnapshot.HasValue.Should().BeTrue();
            data.Value.LatestSnapshot.Value.ShouldBeEquivalentTo(snapshot);
            commits.Length.Should().Be(0);            
        }

        [Fact]
        public async Task duplicate_commit_throws_and_returns_committed_events()
        {
            var commit1 = Setup.UnversionedCommit();
            await _store.Append(commit1, Setup.EventDeserializerWIthoutUpcasting());

            try
            {
                await _store.Append(commit1, Setup.EventDeserializerWIthoutUpcasting());
                throw new InvalidOperationException();
            }
            catch (DuplicateCommitException ex)
            {
                ex.CommitId.Should().Be(commit1.CommitId);
                var ev = new Commit(1, commit1).GetEvents(ImmutableDictionary<Type, IMapEventDataToObject>.Empty);
                ex.Events.Count.Should().Be(1);
                ex.Events.First().CastAs<Event1>().ShouldBeEquivalentTo(ev.First().CastAs<Event1>());
            }
            catch 
            {
                throw new InvalidOperationException();
            }
        }

        [Fact]
        public async Task concurrency_Exception_when_trying_to_commit_with_an_existing_version()
        {
            var commit = Setup.UnversionedCommit();
            var comm2 = Setup.UnversionedCommit(guid: commit.CommitId);
            await _store.Append(commit);
            _store.Import(new Commit(2,commit));
            

        }

        QueryConfig Config(Action<IConfigureQuery> cfg)
        {
            var c=new QueryConfig();
            cfg(c);
            return c;
        }
    }
}