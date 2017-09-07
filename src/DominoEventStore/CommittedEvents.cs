using System;
using System.Collections;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class CommittedEvents //: IReadOnlyCollection<Commit>
    {
        private readonly Commit[] _commits;

        public Commit this[int i] => _commits[i];
        public CommittedEvents(Commit[] commits)
        {
            commits.MustNotBeNull();
            _commits = commits;
            IsEmpty = commits.Length == 0;
        }


        private int _i = -1;
        public bool IsEmpty { get;  }

        public Optional<Commit> GetNext()
        {
            _i++;
            if (_commits.Length<=_i) return Optional<Commit>.Empty;
            return new Optional<Commit>(_commits[_i]);
        }
    }
}