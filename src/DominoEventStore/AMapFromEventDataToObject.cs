using System;

namespace DominoEventStore
{
    public abstract class AMapFromEventDataToObject<T> : IMapEventDataToObject<T> where T:class
    {
        public bool Handles(Type type)=> typeof(T) == type;

        public object Map(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate)
            => Map(jsonData, deserializedEvent as T, commitDate);

        public abstract T Map(dynamic jsonData, T deserializedEvent, DateTimeOffset commitDate);
    }
}