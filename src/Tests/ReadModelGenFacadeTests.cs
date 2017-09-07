 
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using DominoEventStore;
using NSubstitute;
using Xunit.Abstractions;


namespace Tests
{
    public class ReadModelGenFacadeTests
    {
        private readonly ITestOutputHelper _h;
        private ISpecificDbStorage _storage;
        private StoreFacade _sut;
        private ReadModelGenerationConfig _config;

        public ReadModelGenFacadeTests(ITestOutputHelper h)
        {
            _h = h;
            _storage = Substitute.For<ISpecificDbStorage>();
            _sut = new StoreFacade(_storage, Setup.EventStoreSettings);
            _config=new ReadModelGenerationConfig("test");            
        }

        void SetupCommits()
        {
            _storage.StartOrContinue("test").Returns(new ProcessedCommitsCount(0));

            _storage.GetNextBatch(Arg.Any<ReadModelGenerationConfig>(), 2)
                .Returns(new CommittedEvents(new Commit[0]));
            _storage.GetNextBatch(Arg.Any<ReadModelGenerationConfig>(), 0)
                .Returns(Setup.CommittedEvents<Event1, Event2>(2));
        }

        [Fact]
        public void read_model_function_is_invoked_for_each_event()
        {
            var r=new List<Type>();
            SetupCommits();
            _sut.GenerateReadModel("test",e=>{ r.Add(e.GetType());});
            
            r.Count(d => d == typeof(Event1)).Should().Be(2);
            r.Count(d => d == typeof(Event2)).Should().Be(2);
        }


    }
} 
