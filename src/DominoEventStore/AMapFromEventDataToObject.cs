using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public abstract class AMapFromEventDataToObject<T> : IMapEventDataToObject<T> where T:class
    {
        public bool Handles(Type type)=> typeof(T) == type;

        public object Map(IDictionary<string, object> existingData, object deserializedEvent, DateTimeOffset commitDate)
            => Map(existingData, deserializedEvent as T, commitDate);

        /// <summary>
        /// When the event structure changes, this tells EventStore how to treat the old data.
        /// By default, it's just deserialized to the specified event type, ignoring fields that don't match
        /// </summary>
        /// <param name="existingData">Stored event data as expando</param>
        /// <param name="deserializedEvent">Event with values automatically deserialized from the old data</param>
        /// <param name="commitDate"></param>
        /// <returns></returns>
        public abstract T Map(IDictionary<string, object> existingData, T deserializedEvent, DateTimeOffset commitDate);
    }
}