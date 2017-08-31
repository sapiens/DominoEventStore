using System;
using System.Collections;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class EntityEvents:IReadOnlyCollection<object>
    {
        private readonly IReadOnlyCollection<object> _events;
        public Optional<object> LatestSnapshot { get; }=Optional<object>.Empty;

        public static readonly EntityEvents Empty=new EntityEvents();

        private EntityEvents()
        {
            _events=new object[0];
            Version = 0;
        }

        public EntityEvents(IReadOnlyCollection<object> events,int version,Optional<object> latestSnapshot)
        {
            _events = events;
            LatestSnapshot = latestSnapshot;
            Version = version;
        }

        public IEnumerator<object> GetEnumerator()
            => _events.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _events.Count;

        /// <summary>
        /// Latest commit version
        /// </summary>
        public int Version { get; }
    }
}