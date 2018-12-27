using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class DuplicateCommitException : Exception
    {
        //public Guid CommitId { get; }

        //public IReadOnlyCollection<object> Events { get; }

        //public DuplicateCommitException(Guid commitId, IEnumerable<object> events)
        //{
        //    CommitId = commitId;
        //    Events =  new List<object>(events);
        //}
    }
}