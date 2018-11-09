using System;
using Serilog;

namespace DominoEventStore
{
    public class EventStore
    {
        public const string DefaultTenant = "_";

        public static EventStore WithLogger(ILogger logger)
        {
            logger.MustNotBeNull();
            Logger = logger.ForContext<EventStore>();
            return new EventStore();
        }

        [Obsolete("Will be removed in the next iteration. Use the other overload",false)]
        public static EventStore WithLogger(Action<LoggerConfiguration> cfg, LoggerConfiguration existing = null)
        {
            existing=existing??new LoggerConfiguration();
            existing.MinimumLevel.Debug();
            cfg(existing);
            var es=new EventStore();
            Logger = existing.CreateLogger().ForContext<EventStore>();
            return es;
        }

        public static ILogger Logger { get; private set; }

        public IStoreEvents Build(Action<IConfigureEventStore> cfg)
        {
            cfg.MustNotBeDefault();            
            var settings=new EventStoreSettings();
            cfg(settings);
            settings.EnsureIsValid(); 
            EventStore.Logger.Information("Event Store configured");
            EventStore.Logger.Debug("Making sure the db is initiated");
            settings.Store.InitStorage();
            EventStore.Logger.Debug("Event store ready!");
            return new StoreFacade(settings.Store,settings);
        }
    }
}