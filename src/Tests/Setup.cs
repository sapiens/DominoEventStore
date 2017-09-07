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

        public static IEnumerable<Commit> Commits(int count) => Commits<SomeEvent, SomeEvent>(count);
        public static Commit Commit(params object[] events) => new Commit("_",Guid.NewGuid(), Utils.PackEvents(events),Guid.NewGuid(), DateTimeOffset.Now, 1);

        public static IEnumerable<Commit> Commits<T,V>(int count) where T : class, new() where V : class, new()
        {
            return Enumerable.Range(1, count)
                .Select(i => new Commit("_", Setup.EntityId, Utils.PackEvents(new Object[]{new T(), new V()}), Guid.NewGuid(), DateTimeOffset.Now, i));
        }

        public static CommittedEvents CommittedEvents<T, V>(int count) where T : class, new() where V : class, new()
            => new CommittedEvents(Setup.Commits<T, V>(count).ToArray());

        public static IEnumerable<object> GetEvents(this Commit commit, IReadOnlyDictionary<Type, IMapEventDataToObject> upc)
        {
            return Utils.UnpackEvents(commit.Timestamp, commit.EventData, upc);
        }
    }
}