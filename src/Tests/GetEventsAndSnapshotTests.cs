using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoEventStore;
using Xunit;
using FluentAssertions;
using NSubstitute;
using Ploeh.AutoFixture;

namespace Tests
{
    public class GetEventsAndSnapshotTests
    {
        private FacadeStore _sut;
        private ISpecificDbStorage _storage;
        Fixture _fixture=new Fixture();

        public GetEventsAndSnapshotTests()
        {
            _storage = Substitute.For<ISpecificDbStorage>();
            _sut = new FacadeStore(_storage,Setup.EventStoreSettings);
        }

        [Fact]
        public async Task events_for_non_existing_entity_returns_empty()
        {
            var rez = await _sut.GetEvents(Guid.NewGuid());
            rez.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public async Task four_events_without_snapshot()
        {
            var data=new EntityStreamData()
            {
                Commits = SetupCommits(2)
            };
            _storage.GetData(Arg.Any<QueryConfig>(), CancellationToken.None)
                .Returns(new Optional<EntityStreamData>(data));
            var raw = await _sut.GetEvents(Setup.EntityId);
            raw.Value.LatestSnapshot.IsEmpty.Should().BeTrue();
            var evs = raw.Value;
            evs.Count.Should().Be(4);
            evs.Version.Should().Be(2);            
        }

        [Fact]
        public async Task four_events_with_snapshot()
        {
            var data=new EntityStreamData()
            {
                Commits = SetupCommits(2),LatestSnapshot = new Optional<Snapshot>(CreateSnapshot())
            };
            _storage.GetData(Arg.Any<QueryConfig>(), CancellationToken.None)
                .Returns(new Optional<EntityStreamData>(data));
            var raw = await _sut.GetEvents(Setup.EntityId);
            var mem = raw.Value.LatestSnapshot.Value.CastAs<SomeMemento>();
            mem.Should().NotBeNull();
            mem.Name.Should().NotBeNullOrEmpty();
            mem.CreatedOn.Should().NotBe(new DateTimeOffset());
            var evs = raw.Value;
            evs.Count.Should().Be(4);
            evs.Version.Should().Be(2);            
        }

        private Snapshot CreateSnapshot() 
            => new Snapshot(2, Guid.NewGuid(), "_", Utils.PackSnapshot(_fixture.Create<SomeMemento>()), DateTimeOffset.Now);

        IEnumerable<Commit> SetupCommits(int count)
        {
            return Enumerable.Range(1, 2)
                .Select(i => new Commit("_", Setup.EntityId,Utils.PackEvents(_fixture.Create<SomeEvent>(), _fixture.Create<SomeEvent>()), Guid.NewGuid(),DateTimeOffset.Now, i));                        
        }

    }

    public class SomeEvent
     {
         public int Id { get; set; }
         public string Name { get; set; }
         public DateTime CreatedOn { get; set; }
         public string Email { get; set; }
    }

    public class SomeMemento
    {
        public bool IsOpen { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string Name { get; set; }
    }
}