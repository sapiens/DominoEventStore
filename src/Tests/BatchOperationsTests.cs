 
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using DominoEventStore;
using NSubstitute;


namespace Tests
{
    public class BatchOperationsTests
    {
        private BatchOperation _sut;
        private IStoreBatchProgress _store;

        public BatchOperationsTests()
        {
            _store = Substitute.For<IStoreBatchProgress>();
            _sut = new BatchOperation(_store,CreateReadModelConfig());
            ConfigureStore();
        }

        private void ConfigureStore()
        {
            _store.StartOrContinue("test").Returns(new ProcessedCommitsCount(0));
        }

        public static ReadModelGenerationConfig CreateReadModelConfig()
            => new ReadModelGenerationConfig("test");

        [Fact]
        public void no_commits_returned_on_dispose_marks_operation_end()
        {
            NoCommitsStoreSetup();
            _sut.GetNextCommit();
            _sut.Dispose();
            _store.Received(1).MarkOperationAsEnded("test");
        }

        private void NoCommitsStoreSetup()
        {
            _store.GetNextBatch(Arg.Any<ReadModelGenerationConfig>(), Arg.Any<ProcessedCommitsCount>())
                .Returns(new CommittedEvents(new Commit[0]));
        }

        void StoreSetupWithCommits()
        {
            _store.GetNextBatch(Arg.Any<ReadModelGenerationConfig>(), Arg.Any<ProcessedCommitsCount>())
                .Returns( new CommittedEvents(Setup.Commits(4).ToArray()));
        }

        [Fact]
        public void with_commits_dispose_saves_progress()
        {
            StoreSetupWithCommits();
            _sut.GetNextCommit().HasValue.Should().BeTrue();
            _sut.Dispose();
            _store.Received(1).UpdateProgress("test",0);
        }

        [Fact]
        public void no_commits_returned_on_get_next_returns_empty_commit()
        {
            NoCommitsStoreSetup();
            _sut.GetNextCommit().IsEmpty.Should().BeTrue();
        }


    }
} 
