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
}