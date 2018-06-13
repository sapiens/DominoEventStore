using System;

namespace DominoEventStore
{
    public interface IMapEventDataToObject
    {
        bool Handles(Type type);
        object Map(dynamic existingData, object deserializedEvent, DateTimeOffset commitDate);
        
    }

    public interface IMapEventDataToObject<T>:IMapEventDataToObject where T : class
    {
        /// <summary>
        /// Allows you to change the values of the event
        /// </summary>
        /// <param name="existingData">Stored event data</param>
        /// <param name="deserializedEvent">Event instance containing values automatically deserialized from existing data</param>
        /// <param name="commitDate"></param>
        /// <returns></returns>
        T Map(dynamic existingData, T deserializedEvent, DateTimeOffset commitDate);
    }
}