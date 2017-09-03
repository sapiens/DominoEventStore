using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class EventStoreSettings
    {
        Dictionary<Type,IMapEventDataToObject> _eventMappers=new Dictionary<Type, IMapEventDataToObject>();

        public IReadOnlyDictionary<Type, IMapEventDataToObject> EventMappers => _eventMappers;
        
    }
}