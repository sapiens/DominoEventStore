using System;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers;

namespace DominoEventStore.Providers
{
    public class SqlFuConfiguration 
    {
        private SqlFuConfig _config;
       
      

     public void Configure(IDbProvider provider,string cnx,string schema=null)
        {
            provider.MustNotBeNull("We need a provider for SqlFu");
            cnx.MustNotBeEmpty("We need a connection string");
            _config = _config ?? SqlFuManager.Config;
            _config.AddProfile<IEventStoreSqlFactory>(provider,cnx);
            _config.Converters.RegisterConverter(o => 
            (o == null || o == DBNull.Value) ? (int?)null : (int)o);
            _config.Converters.RegisterConverter(o =>
            {
                if (o.GetType() == typeof(long)) return (long) o;
                if (o.GetType() == typeof(long?)) return (long?) o;
                throw new InvalidCastException();
            });
            _config.ConfigureTableForPoco<Commit>(d =>
            {
                d.Table = new TableName(ASqlDbProvider.CommitsTable, schema);
                d.IdentityColumn = "Id";
            });
            _config.ConfigureTableForPoco<Snapshot>(d =>
            {
                d.Table = new TableName(ASqlDbProvider.SnapshotsTable, schema);
                d.IdentityColumn = "Id";
            });
            _config.ConfigureTableForPoco<BatchProgress>(d =>
                d.Table = new TableName(ASqlDbProvider.BatchTable, schema));
        }
    }
}