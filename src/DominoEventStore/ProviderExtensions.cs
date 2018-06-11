using System;
using System.Data.Common;
using DominoEventStore.Providers;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers.SqlServer;

namespace DominoEventStore
{
    public static class ProviderExtensions
    {
        public static IConfigureEventStore UseMSSql(this IConfigureEventStore store,Func<DbConnection> factory, string cnx,string schema = null)
        {
            SqlFuManager.Configure(d=>
            {
                d.AddProfile(new SqlServer2012Provider(factory), cnx, "mssql");
                RegisterSqlFuConfig(d, schema);
            });
            
            var provider=new SqlServerProvider(SqlFuManager.GetDbFactory("mssql"));
            store.WithProvider(provider);
            return store;
        }
        public static IConfigureEventStore UseSqlite(this IConfigureEventStore store,Func<DbConnection> factory, string cnx,string schema = null)
        {
            SqlFuManager.Configure(d=>
            {
                d.AddProfile(new SqlFu.Providers.Sqlite.SqliteProvider(factory), cnx, "sqlite");
                RegisterSqlFuConfig(d, schema);
            });
            
            var provider=new SqliteProvider(SqlFuManager.GetDbFactory("sqlite"));
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