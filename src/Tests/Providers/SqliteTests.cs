using System.IO;
using DominoEventStore.Providers;

namespace Tests
{
    public class SqliteTests : ASpecificStorageTests
    {
        public static string ConnectionString { get; } = "Data Source=test.db;Version=3;New=True;BinaryGUID=False";


        public SqliteTests() : base(Setup.GetDbFactory<SqliteTests>())
        {
            
        }

        protected override void DisposeOther()
        {
            //File.Delete("test.db");
        }
    }
}