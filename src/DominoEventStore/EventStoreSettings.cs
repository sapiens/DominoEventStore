using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class EventStoreSettings
    {
        Dictionary<Type,IMapEventDataToObject> _eventMappers=new Dictionary<Type, IMapEventDataToObject>();

        public IReadOnlyDictionary<Type, IMapEventDataToObject> EventMappers => _eventMappers;

        public EventStoreSettings AddMapper<T>(AMapFromEventDataToObject<T> mapper) where T : class
        {
            _eventMappers.Add(typeof(T),mapper);
            return this;
        }

    }
}