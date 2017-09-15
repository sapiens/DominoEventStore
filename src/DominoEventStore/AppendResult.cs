namespace DominoEventStore
{
    public class AppendResult
    {
        public static readonly  AppendResult Ok=new AppendResult();

        private AppendResult()
        {
            WasSuccessful = true;
        }

        public bool WasSuccessful { get;  }

        public AppendResult(Commit commit)
        {
            DuplicateCommit = commit;
        }

        public Commit DuplicateCommit { get; }
    }
}