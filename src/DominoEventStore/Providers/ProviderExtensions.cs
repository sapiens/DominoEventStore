using System;
using System.Data.Common;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers.SqlServer;

namespace DominoEventStore.Providers
{
    public static class ProviderExtensions
    {
        public static IConfigureEventStore UseMSSql(this IConfigureEventStore store, string cnx, Func<DbConnection> factory, string schema = null)
        {
           RegisterSqlFuConfig(schema);
            var provider=new SqlServerProvider(SqlFuManager.Config.CreateFactory<IDbFactory>(new SqlServer2012Provider(factory),cnx));
            store.WithProvider(provider);
            return store;
        }

        static bool _sqlFuDone=false;
       public static void RegisterSqlFuConfig(string schema=null)
        {
            if (_sqlFuDone) return;
            SqlFuManager.Config.RegisterConverter(o =>
            {
                if (o == null || o == DBNull.Value)
                {
                    return (int?) null;
                }
                else
                {
                   
                    if (o.GetType()==typeof(Int64)) return (int)(long) o;
                    if (o.GetType()==typeof(Int32)) return (int)o;
                    //return (int)o;
                }
                throw new InvalidCastException();
            });
            SqlFuManager.Config.RegisterConverter(o =>
            {
                if (o == null) return null;
                if (o.GetType() == typeof(long)) return (long)o;
                if (o.GetType() == typeof(long?)) return (long?)o;
                throw new InvalidCastException();
            });
            SqlFuManager.Config.ConfigureTableForPoco<Commit>(d =>
            {
                d.Table = new TableName(ASqlDbProvider.CommitsTable, schema);
                d.IdentityColumn = "Id";
            });
            SqlFuManager.Config.ConfigureTableForPoco<Snapshot>(d =>
            {
                d.Table = new TableName(ASqlDbProvider.SnapshotsTable, schema);
                d.IdentityColumn = "Id";
            });
            SqlFuManager.Config.ConfigureTableForPoco<BatchProgress>(d =>
                d.Table = new TableName(ASqlDbProvider.BatchTable, schema));
            _sqlFuDone = true;
        }
    }
}