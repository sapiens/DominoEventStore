using DominoEventStore.Providers;

namespace Tests
{
    public class InMemoryStoreTests : ASpecificStorageTests
    {
        public InMemoryStoreTests() : base(new InMemory())
        {
        }
    }
}