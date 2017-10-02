using DominoEventStore.Providers;
using SqlFu;
using Xunit.Abstractions;

namespace Tests
{
    public class InMemoryStoreTests : ASpecificStorageTests
    {
        public InMemoryStoreTests(ITestOutputHelper t) : base(t)
        {
        }

        protected override IDbFactory GetFactory()
            => null;
    }
}