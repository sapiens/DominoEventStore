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
     
        /// <returns></returns>
        CommittedEvents GetNextBatch(ReadModelGenerationConfig config,ProcessedCommitsCount count);
        CommittedEvents GetNextBatch(MigrationConfig config,ProcessedCommitsCount count);

        void UpdateProgress(string name, ProcessedCommitsCount processedCommits);
        void MarkOperationAsEnded(string name);
    }
}