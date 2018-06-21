using System;
using System.Collections.Generic;
using Serilog;

namespace DominoEventStore
{
    public class EventStoreSettings:IConfigureEventStore
    {
        Dictionary<Type,IMapEventDataToObject> _eventMappers=new Dictionary<Type, IMapEventDataToObject>();

        public EventStoreSettings(ILogger logger)
        {
            Logger = logger;
        }

        public IReadOnlyDictionary<Type, IMapEventDataToObject> EventMappers => _eventMappers;

        public ISpecificDbStorage Store { get; private set; }

        public IConfigureEventStore AddMapper<T>(AMapFromEventDataToObject<T> mapper) where T : class
        {
            _eventMappers.Add(typeof(T),mapper);
            Logger.Debug($"Added event mapper {typeof(T)}");
            return this;
        }

        public IConfigureEventStore WithProvider(ISpecificDbStorage store,string schema="")
        {
            store.MustNotBeNull();
            Logger.Debug($"Using provider {store.GetType()}");
            Store = store;
            Store.Schema = schema;
            return this;
        }
        
        public ILogger Logger { get; private set; }

        public void EnsureIsValid()
        {
            Store.MustNotBeNull();
        }
    }
}