using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class EventStoreSettings:IConfigureEventStore
    {
        Dictionary<Type,IMapEventDataToObject> _eventMappers=new Dictionary<Type, IMapEventDataToObject>();
        private ISpecificDbStorage _store;

        public IReadOnlyDictionary<Type, IMapEventDataToObject> EventMappers => _eventMappers;

        public ISpecificDbStorage Store => _store;



        public IConfigureEventStore AddMapper<T>(AMapFromEventDataToObject<T> mapper) where T : class
        {
            _eventMappers.Add(typeof(T),mapper);
            return this;
        }

        public IConfigureEventStore WithProvider(ISpecificDbStorage store,string schema="")
        {
            store.MustNotBeNull();
            _store = store;
            _store.Schema = schema;
            return this;
        }

        public void EnsureIsValid()
        {
            _store.MustNotBeNull();
        }
    }
}