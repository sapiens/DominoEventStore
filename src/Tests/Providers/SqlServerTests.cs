using DominoEventStore;
using DominoEventStore.Providers;

namespace Tests
{
    public class SqlServerTests : ASpecificStorageTests
    {
        public static string ConnectionString =>
            Setup.IsAppVeyor
                ? @"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=Password12!"
                : @"Data Source=.\SQLExpress;Initial Catalog=tempdb;Integrated Security=True;MultipleActiveResultSets=True";
        public SqlServerTests() : base(Setup.GetDbFactory<SqlServerTests>())
        {
          
        }
    }
}