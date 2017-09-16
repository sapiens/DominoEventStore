using System;
using System.Data.Common;
using SqlFu.Providers.SqlServer;

namespace DominoEventStore.Providers
{
    public static class ProviderExtensions
    {
        public static IConfigureEventStore UseMSSql(this IConfigureEventStore store, string cnx, Func<DbConnection> factory, string schema = null)
        {
            var c=new SqlFuConfiguration();
            c.Configure(new SqlServer2012Provider(factory),cnx,schema);
            store.WithProvider(ASqlDbProvider.CreateFor(SqlServer2012Provider.Id,schema));
            return store;
        }

     
    }
}