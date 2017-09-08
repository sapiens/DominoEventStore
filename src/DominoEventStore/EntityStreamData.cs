using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoEventStore
{
    public class EntityStreamData
    {
        public Optional<Snapshot> LatestSnapshot { get; set; }
        public IEnumerable<Commit> Commits { get; set; }=Enumerable.Empty<Commit>();

        public Optional<EntityStreamData> ToOptional() => LatestSnapshot.IsEmpty && Commits.IsNullOrEmpty()
            ? Optional<EntityStreamData>.Empty
            : new Optional<EntityStreamData>(this);
    }
}