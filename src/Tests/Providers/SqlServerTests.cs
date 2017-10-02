using System.Data.SqlClient;
using System.Data.SQLite;
using DominoEventStore;
using DominoEventStore.Providers;
using SqlFu;
using SqlFu.Providers.SqlServer;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Sql server")]
    public class SqlServerTests : ASpecificStorageTests
    {
        public static string ConnectionString =>
            Setup.IsAppVeyor
                ? @"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=Password12!"
                : @"Data Source=.\SQLExpress;Initial Catalog=tempdb;Integrated Security=True;MultipleActiveResultSets=True";
        public SqlServerTests(ITestOutputHelper t) : base(t)
        {
          
        }
        protected override IDbFactory GetFactory()
            => new SqlFuConfig().CreateFactoryForTesting(new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection), ConnectionString);
    }
}