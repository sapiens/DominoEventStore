using System;
using DominoEventStore;

namespace Tests
{
    public static class Setup
    {
        public static readonly EventStoreSettings EventStoreSettings=new EventStoreSettings()
        {
            
        };

        public static readonly Guid EntityId = Guid.NewGuid();
    }
}