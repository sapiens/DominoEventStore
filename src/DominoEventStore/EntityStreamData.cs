using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoEventStore
{
    public class EntityStreamData
    {
        public Optional<Snapshot> LatestSnapshot { get; set; }
        public IEnumerable<Commit> Commits { get; set; }=Enumerable.Empty<Commit>();
    }
}