using DominoEventStore.Providers;

namespace Tests
{
    public class SqliteTests : ASpecificStorageTests
    {
        public static string ConnectionString { get; } = "Data Source=:memory:;Version=3;New=True;BinaryGUID=False";


        public SqliteTests() : base(Setup.GetDbFactory<SqliteTests>())
        {
            
        }
    }
}