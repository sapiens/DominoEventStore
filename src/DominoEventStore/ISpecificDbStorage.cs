using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore
{
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
}