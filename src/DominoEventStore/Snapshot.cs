using System;

namespace DominoEventStore
{
    public class Snapshot
    {
        public int Version { get; set; }
        public Guid EntityId { get; set; }
        public string TenantId { get; set; }
        public string SerializedData { get; set; }
        public DateTimeOffset SnapshotDate { get; set; }
    }
}