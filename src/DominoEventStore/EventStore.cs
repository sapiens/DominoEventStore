using System;

namespace DominoEventStore
{
    public class EventStore
    {
        public const string DefaultTenant = "_";

        public static IStoreEvents Build(Action<IConfigureEventStore> cfg)
        {
            cfg.MustNotBeDefault();
            var settings=new EventStoreSettings();
            cfg(settings);
            settings.EnsureIsValid(); 
            settings.Store.InitStorage();
            return new StoreFacade(settings.Store,settings);
        }
    }
}