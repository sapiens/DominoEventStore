using System;
using System.Data.Common;
using SqlFu;
using SqlFu.Providers;
using SqlFu.Providers.SqlServer;

namespace DominoEventStore.Providers
{
    class SqlFuConfiguration : IConfigureSqlFu
    {
        private SqlFuConfig _config;
        private IDbProvider _provider;
        private string _cnx;

        public IConfigureSqlFu IntegrateWith(SqlFuConfig config)
        {
            _config = config;
            return this;
        }

        public IConfigureSqlFu WithMSSql(string cnx, Func<DbConnection> factory)
        {
            _provider=new SqlServer2012Provider(factory);
            _cnx = cnx;
            ProviderId = _provider.ProviderId;
            return this;
        }

        public string ProviderId { get; private set; }

        public void Configure()
        {
            _provider.MustNotBeNull("We need a provider for SqlFu");
            _cnx.MustNotBeEmpty("We need a connection string");
            _config = _config ?? SqlFuManager.Config;
            _config.AddProfile<IEventStoreSqlFactory>(_provider,_cnx);
        }
    }
}