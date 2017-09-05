using System;
using System.Collections.Generic;
using System.Linq;
using DominoEventStore;

namespace Tests
{
    public static class Setup
    {
        public static readonly EventStoreSettings EventStoreSettings=new EventStoreSettings()
        {
            
        };

        public static readonly Guid EntityId = Guid.NewGuid();

        public static IEnumerable<Commit> Commits(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Commit("_", Setup.EntityId, Utils.PackEvents(new SomeEvent(), new SomeEvent()), Guid.NewGuid(), DateTimeOffset.Now, i));
        }

    }
}