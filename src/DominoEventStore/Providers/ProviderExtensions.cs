using System;

namespace DominoEventStore.Providers
{
    public static class ProviderExtensions
    {
        public static IConfigureEventStore UseSqlFu(this IConfigureEventStore store,Action<IConfigureSqlFu> config)
        {
            var c=new SqlFuConfiguration();
            config(c);
            c.Configure();
            store.WithProvider(ASqlDbProvider.CreateFor(c.ProviderId));
            return store;
        }

     
    }
}