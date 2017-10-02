using CavemanTools.Logging;
using DominoEventStore.Providers;
using SqlFu;
using SqlFu.Configuration;

namespace DominoEventStore
{
    public static class ProviderExtensions
    {
        public static IConfigureEventStore UseMSSql(this IConfigureEventStore store, IDbFactory factory, string schema = null)
        {
           RegisterSqlFuConfig(factory.Configuration, schema);
            var provider=new SqlServerProvider(factory);
            store.WithProvider(provider);
            return store;
        }
        public static IConfigureEventStore UseSqlite(this IConfigureEventStore store, IDbFactory factory)
        {
           RegisterSqlFuConfig(factory.Configuration);
            var provider=new SqliteProvider(factory);
            store.WithProvider(provider);
            return store;
        }

 
       public static void RegisterSqlFuConfig(SqlFuConfig config,string schema=null)
        {
            config.ConfigureTableForPoco<Commit>(d =>
            {
                d.TableName = new TableName(ASqlDbProvider.CommitsTable, schema);                    
            });
            config.ConfigureTableForPoco<Snapshot>(d =>
            {
                d.TableName = new TableName(ASqlDbProvider.SnapshotsTable, schema);                    
            });
            config.ConfigureTableForPoco<BatchProgress>(d =>d.TableName = new TableName(ASqlDbProvider.BatchTable, schema));   
                    
        }
    }
}