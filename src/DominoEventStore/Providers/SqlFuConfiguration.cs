using System;
using System.Data.Common;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers;
using SqlFu.Providers.SqlServer;

namespace DominoEventStore.Providers
{
    class SqlFuConfiguration : IConfigureSqlFu
    {
        private SqlFuConfig _config;
        private IDbProvider _provider;
        private string _cnx;
        private string _schema;

        public IConfigureSqlFu IntegrateWith(SqlFuConfig config)
        {
            _config = config;
            return this;
        }

        public IConfigureSqlFu WithMSSql(string cnx, Func<DbConnection> factory, string schema = null)
        {
            _provider=new SqlServer2012Provider(factory);
            _schema = schema;
            _cnx = cnx;
            ProviderId = _provider.ProviderId;
            return this;
        }

        public string ProviderId { get; private set; }

        public string Schema => _schema;

        public void Configure()
        {
            _provider.MustNotBeNull("We need a provider for SqlFu");
            _cnx.MustNotBeEmpty("We need a connection string");
            _config = _config ?? SqlFuManager.Config;
            _config.AddProfile<IEventStoreSqlFactory>(_provider,_cnx);
            _config.ConfigureTableForPoco<Commit>(d =>
            {
                d.Table = new TableName(ASqlDbProvider.CommitsTable, _schema);
                d.IdentityColumn = "Id";
            });
            _config.ConfigureTableForPoco<Snapshot>(d =>
            {
                d.Table = new TableName(ASqlDbProvider.SnapshotsTable, _schema);
                d.IdentityColumn = "Id";
            });
            _config.ConfigureTableForPoco<BatchProgress>(d =>
                d.Table = new TableName(ASqlDbProvider.BatchTable, _schema));
        }
    }
}