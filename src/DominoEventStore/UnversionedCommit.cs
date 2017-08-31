using System;

namespace DominoEventStore
{
    public class UnversionedCommit
    {
        public UnversionedCommit(string tenantId, Guid entityId, string eventData, Guid commitId, DateTimeOffset timestamp)
        {
            tenantId.MustNotBeEmpty();
            entityId.MustNotBeDefault();
            eventData.MustNotBeEmpty();
            commitId.MustNotBeDefault();
            timestamp.MustNotBeDefault();


            TenantId = tenantId;
            EntityId = entityId;
            EventData = eventData;
            CommitId = commitId;
            Timestamp = timestamp;            
        }

        protected UnversionedCommit()
        {
            
        }

        public Guid CommitId { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public string TenantId { get; private set; }
        public Guid EntityId { get; private set; }
        public string EventData { get; private set; }
    }
}