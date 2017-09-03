using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface IStoreBatchProgress
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ProcessedCommitsCount StartOrContinue(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="count"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        CommittedEvents GetNextBatch(ReadModelGenerationConfig config,ProcessedCommitsCount count);

        void UpdateProgress(string name, ProcessedCommitsCount processedCommits);
        void MarkOperationAsEnded(string name);
    }

    public interface ISpecificDbStorage : IStoreBatchProgress
    {
        /// <summary>
        /// Should calculate the version of the entity and use that to detect concurrency problems
        /// </summary>
        /// <param name="commit"></param>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <exception cref="ConcurrencyException"></exception>
        /// <returns></returns>
        Task Append(UnversionedCommit commit);
        /// <summary>
        /// Adds the commit as is
        /// </summary>
        /// <param name="commit"></param>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <returns></returns>
        Task Append(Commit commit);

        Task<Optional<EntityStreamData>> GetData(QueryConfig cfg, CancellationToken token);
        /// <summary>
        /// Creates the tables in the specified/default schema
        /// </summary>
        /// <param name="schema"></param>
        void InitStorage(string schema = null);
        void ResetStorage();
        void DeleteTenant(string tenantId);

        Task Store(Snapshot snapshot);
        /// <summary>
        /// Delete one or all stored snapshots
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="tenantId"></param>
        /// <param name="entityVersion">If missing, it deletes all stored snapshots</param>
        /// <returns></returns>
        Task DeleteSnapshot(Guid entityId, string tenantId, int? entityVersion=null);
    }

    public struct ProcessedCommitsCount
    {
        public ProcessedCommitsCount(int value)
        {
            value.Must(d=>d>=0);
            Value = value;
        }

        public int Value { get; }

        public static implicit operator int(ProcessedCommitsCount d) => d.Value;
        public static implicit operator ProcessedCommitsCount(int d) => new ProcessedCommitsCount(d);
    }

    public class CommittedEvents : IReadOnlyCollection<Commit>
    {
        private readonly Commit[] _commits;

        public CommittedEvents(Commit[] commits)
        {
            _commits = commits;
        }



        public IEnumerator<Commit> GetEnumerator()
        {
            throw new NotImplementedException();
        }

      
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }

        private int _i = -1;

        public Optional<Commit> GetNext()
        {
            _i++;
            if (_commits.Length<=_i) return Optional<Commit>.Empty;
            return new Optional<Commit>(_commits[_i]);
        }
    }

   public class BatchOperation : IDisposable
    {
        private readonly IStoreBatchProgress _store;
        private readonly ReadModelGenerationConfig _config;
        private ProcessedCommitsCount _processed;

        public BatchOperation(IStoreBatchProgress store, ReadModelGenerationConfig config)
        {
            _store = store;
            _config = config;
            _processed = _store.StartOrContinue(config.Name);
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
                _commits= _store.GetNextBatch(_config, _processed);
            }
            if (_commits.IsNullOrEmpty())
            {
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
                return;
            }
            _store.UpdateProgress(_config.Name,_processed.Value-1);
        }
    }
}