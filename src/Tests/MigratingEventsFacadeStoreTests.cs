﻿ 
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DominoEventStore;
using NSubstitute;
using Xunit.Abstractions;


namespace Tests
{
    public class MigratingEventsFacadeStoreTests
    {
        private ISpecificDbStorage _store;
        private StoreFacade _sut;
        private IStoreEvents _dest;
      //  private EventStoreSettings _settings= new EventStoreSettings();
        private FakeImport _importer;
        private List<object> _events;
        private CommittedEvents _commits;
        private EventStoreSettings _settings;

        public MigratingEventsFacadeStoreTests(ITestOutputHelper h)
        {
            _store = Substitute.For<ISpecificDbStorage>();
            _settings = Setup.EventStoreSettings(h);
            _sut = new StoreFacade(_store, _settings);
            _dest = Substitute.For<IStoreEvents>();
            _importer=new FakeImport();
            _dest.Advanced.Returns(_importer);
        }

        void SetupStore()
        {
            _store.StartOrContinue("m").Returns(new ProcessedCommitsCount(0));

            _store.GetNextBatch(Arg.Any<MigrationConfig>(), new ProcessedCommitsCount(3)).Returns(new CommittedEvents(new Commit[0]));
            CreateEvents();
            _commits=new CommittedEvents(
                new []{
                    Setup.Commit(_events[0],_events[1]),
                    Setup.Commit(_events[2]),
                    Setup.Commit(_events[3]) }
            );
            _store.GetNextBatch(Arg.Any<MigrationConfig>(), new ProcessedCommitsCount(0)).Returns(_commits);
        }


        private void CreateEvents()
        {
            _events = Setup.Events();
        }



        private static MigrationConfig GetMigrationConfig()
        {
            return new MigrationConfig("m");
        }

        [Fact]
        public void simple_import_no_rewriting_no_casting()
        {
            SetupStore();
            _sut.Advanced.MigrateEventsTo(_dest,"m");
           _importer.Commits.Count.Should().Be(3);
           _importer.Commits[0].Should().BeEquivalentTo(_commits[0]);
           _importer.Commits[1].Should().BeEquivalentTo(_commits[1]);
           _importer.Commits[2].Should().BeEquivalentTo(_commits[2]);

        }

        [Fact]
        public void import_no_rewriting_with_casting()
        {
            _settings.AddMapper(new UpcastEvent1());
            SetupStore();
            _sut.Advanced.MigrateEventsTo(_dest,"m");
           _importer.Commits.Count.Should().Be(3);
            //we avoid double upcasting
            var evs = _importer.Commits[0].GetEvents(new Dictionary<Type, IMapEventDataToObject>());
            evs.First().CastAs<Event1>().Nr.Should().Be(_events[0].CastAs<Event1>().Nr + 10);
            
        }

        [Fact]
        public void import_rewriting_with_casting()
        {
            _settings.AddMapper(new UpcastEvent1());
            SetupStore();
            _sut.Advanced.MigrateEventsTo(_dest,"m", c =>
            {
                c.AddConverters(new RewriteEvent1());
            });
           _importer.Commits.Count.Should().Be(3);
            //we avoid double upcasting
            var evs = _importer.Commits[0].GetEvents(new Dictionary<Type, IMapEventDataToObject>());
            var event1 = evs.First().CastAs<Event1>();
            event1.Nr.Should().Be(_events[0].CastAs<Event1>().Nr+10);
            event1.Name.Should().Be("rewritten");
        }

        [Fact]
        public void import_rewriting_with_no_casting()
        {
            SetupStore();
            _sut.Advanced.MigrateEventsTo(_dest,"m", c =>
            {
                c.AddConverters(new RewriteEvent1());
            });
           _importer.Commits.Count.Should().Be(3);
            //we avoid double upcasting
            var evs = _importer.Commits[0].GetEvents(new Dictionary<Type, IMapEventDataToObject>());
            var event1 = evs.First().CastAs<Event1>();
            event1.Nr.Should().Be(_events[0].CastAs<Event1>().Nr);
            event1.Name.Should().Be("rewritten");
        }



        class FakeImport : IAdvancedFeatures
        {
            public void MigrateEventsTo(IStoreEvents newStorage, string name, Action<IConfigMigration> config = null)
            {
                throw new NotImplementedException();
            }

            public void ResetStorage()
            {
                throw new NotImplementedException();
            }

            public List<Commit> Commits { get; private set; }= new List<Commit>();

            public void ImportCommit(Commit commits)
            {
              Commits.Add(commits);
            }

            public void DeleteTenant(string tenantId)
            {
                throw new NotImplementedException();
            }

            public void GenerateReadModel(string operationName, Action<dynamic> modelUpdater, Action<IConfigReadModelGeneration> config = null)
            {
                throw new NotImplementedException();
            }
        }
    }
} 
