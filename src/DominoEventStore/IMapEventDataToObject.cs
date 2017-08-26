using System;

namespace DominoEventStore
{
    public interface IMapEventDataToObject
    {
        bool Handles(Type type);
        object Map(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate);
        
    }

    public interface IMapEventDataToObject<T>:IMapEventDataToObject where T : class
    {
        T Map(dynamic jsonData, T deserializedEvent, DateTimeOffset commitDate);
    }

    public abstract class AMapFromEventDataToObject<T> : IMapEventDataToObject<T> where T:class
    {
        public bool Handles(Type type)=> typeof(T) == type;

        public object Map(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate)
            => Map(jsonData, deserializedEvent as T, commitDate);

        public abstract T Map(dynamic jsonData, T deserializedEvent, DateTimeOffset commitDate);
    }

    public class SomeDataMapper : AMapFromEventDataToObject<SomeData>
    {
        public override SomeData Map(dynamic jsonData, SomeData deserializedEvent, DateTimeOffset commitDate)
        {
            throw new NotImplementedException();
        }
    }

    public class SomeData
    {
        public string Name { get; set; }
    }
}