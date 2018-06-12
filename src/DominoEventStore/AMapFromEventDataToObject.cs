using System;

namespace DominoEventStore
{
    public abstract class AMapFromEventDataToObject<T> : IMapEventDataToObject<T> where T:class
    {
        public bool Handles(Type type)=> typeof(T) == type;

        public object Map(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate)
            => Map(jsonData, deserializedEvent as T, commitDate);

        /// <summary>
        /// When the event structure changes, this tells EventStore how to treat the old data.
        /// By default, it's just deserialized to the specified event type, ignoring fields that don't match
        /// </summary>
        /// <param name="jsonData">Stored event data as expando</param>
        /// <param name="deserializedEvent">Event with old data automatically deserialized</param>
        /// <param name="commitDate"></param>
        /// <returns></returns>
        public abstract T Map(dynamic jsonData, T deserializedEvent, DateTimeOffset commitDate);
    }
}