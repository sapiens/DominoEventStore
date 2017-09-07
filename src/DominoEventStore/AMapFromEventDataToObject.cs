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

    public abstract class ARewriteEvent<T> : IRewriteEventData where T:class 
    {
        protected ARewriteEvent()
        {
            HandledType = typeof(T);
        }
        public Type HandledType { get; }

        public object Rewrite(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate)
            => Rewrite(jsonData, deserializedEvent as T, commitDate);

        public abstract T Rewrite(dynamic jsonData, T deserializedEvent, DateTimeOffset commitDate);
    }
}