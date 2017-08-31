using System;

namespace DominoEventStore
{
    public class Snapshot
    {
        public Snapshot(int version, Guid entityId, string tenantId, string serializedData, DateTimeOffset snapshotDate)
        {
            Version = version;
            EntityId = entityId;
            TenantId = tenantId;
            SerializedData = serializedData;
            SnapshotDate = snapshotDate;
        }

        public int Version { get; private set; }
        public Guid EntityId { get; private set; }
        public string TenantId { get; private set; }
        public string SerializedData { get; private set; }
        public DateTimeOffset SnapshotDate { get; private set; }
    }
}