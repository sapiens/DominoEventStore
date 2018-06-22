using System;

namespace DominoEventStore
{
    public class BatchOperation : IDisposable
    {
        private readonly IStoreBatchProgress _store;
        private readonly dynamic _config;
        private ProcessedCommitsCount _processed;

        public BatchOperation(IStoreBatchProgress store, ReadModelGenerationConfig config)
        {
            _store = store;
            _config = config;
            _processed = _store.StartOrContinue(config.Name);
            EventStore.Logger.Debug($"'{config.Name}' [read model generation] starts/resumes after {_processed.Value} commits");
        }
         public BatchOperation(IStoreBatchProgress store, MigrationConfig config)
        {
            _store = store;
            _config = config;
            _processed = _store.StartOrContinue(config.Name);
            EventStore.Logger.Debug($"'{config.Name}' [migration] starts/resumes after {_processed.Value} commits");
        }

        private bool _hasEnded = false;
        private CommittedEvents _commits;

        /// <summary>
        /// Commits should be ordered ascending by commit date
        /// </summary>
        
        /// <returns></returns>
        public Optional<Commit> GetNextCommit()
        {
            start:
            if (_commits == null)
            {
               EventStore.Logger.Debug($"Getting next batch of '{_config.Name}'");
                _commits= _store.GetNextBatch(_config, _processed);
            }
            if (_commits.IsEmpty)
            {
                EventStore.Logger.Debug($"No more batches for '{_config.Name}'");
                _hasEnded = true;
                return Optional<Commit>.Empty;
            }
            
            var next= _commits.GetNext();
            if (next.IsEmpty)
            {
                _commits = null;
                goto start;                
            }
            _processed++;
            EventStore.Logger.Debug($"{_processed.Value} commits processed as part of '{_config.Name}'");
            return next;
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_hasEnded)
            {
                _store.MarkOperationAsEnded(_config.Name);
                EventStore.Logger.Debug($"'{_config.Name}' ended");
                return;
            }
            if(_processed==0) return;
            _store.UpdateProgress(_config.Name,_processed.Value-1);
            EventStore.Logger.Debug($"'{_config.Name}' saved progress at {_processed.Value-1}");
        }
    }
}