using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface ISpecificDbStorage : IStoreBatchProgress
    {
       // /// <summary>
       // /// Should calculate the version of the entity and use that to detect concurrency problems.
       // /// If commit id exists, the existing commit will be returned 
       // /// </summary>
       // /// <param name="commit"></param>
       // /// <exception cref="ConcurrencyException"></exception>
       ///// <returns></returns>
       // Task<AppendResult> Append(UnversionedCommit commit);
        /// <summary>
        /// Should calculate the versions of each entity and use that to detect concurrency problems.
        /// </summary>
        /// <param name="commits"></param>
        /// <exception cref="ConcurrencyException"></exception>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <returns></returns>
        Task Append(params UnversionedCommit[] commits);
        /// <summary>
        /// Adds the commit as is. Duplicates should be ignored
        /// </summary>
        /// <param name="commit"></param>        
        /// <returns></returns>
        void Import(Commit commit);

        Task<Optional<EntityStreamData>> GetData(QueryConfig cfg, CancellationToken token);

        /// <summary>
        /// Creates the tables in the specified/default schema. If they already exist, ignore them
        /// </summary>
        void InitStorage();

        /// <summary>
        /// Empty means default schema
        /// </summary>
        string Schema { get; set; }

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

    public interface IUnitOfWork : IDisposable
    {
        void Commit();
    }
}