using System;
using CavemanTools.Logging;
using Serilog;

namespace DominoEventStore
{
    public class EventStore
    {
        public const string DefaultTenant = "_";

        public static EventStore WithLogger(Action<LoggerConfiguration> cfg, LoggerConfiguration existing = null)
        {
            existing=existing??new LoggerConfiguration();
            existing.MinimumLevel.Debug()/*.Enrich.WithProperty("type", "DominoES")*/;
            cfg(existing);
            var es=new EventStore();
            es.Logger = existing.CreateLogger().ForContext<EventStore>();
            return es;
        }

        private ILogger Logger { get; set; }

        public IStoreEvents Build(Action<IConfigureEventStore> cfg)
        {
            cfg.MustNotBeDefault();            
            var settings=new EventStoreSettings(Logger);
            cfg(settings);
            settings.EnsureIsValid(); 
            settings.Logger.Information("Event Store configured");
            settings.Logger.Debug("Making sure the db is initiated");
            settings.Store.InitStorage();
            settings.Logger.Debug("Event store ready!");
            return new StoreFacade(settings.Store,settings);
        }
    }
}