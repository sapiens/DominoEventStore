using System.Data.SQLite;
using SqlFu;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("sqlite")]
    
    public class SqliteTests : ASpecificStorageTests
    {
        public static string ConnectionString { get; } = "Data Source=test.db;Version=3;New=True;BinaryGUID=False";


        public SqliteTests(ITestOutputHelper t) : base(t)
        {
            
        }

        protected override IDbFactory GetFactory()
        => new SqlFuConfig().CreateFactoryForTesting(new SqlFu.Providers.Sqlite.SqliteProvider(SQLiteFactory.Instance.CreateConnection), ConnectionString);

        protected override void DisposeOther()
        {
            
        }
    }
}