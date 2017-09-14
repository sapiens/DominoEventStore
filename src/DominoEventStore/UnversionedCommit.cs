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

        public Guid CommitId { get; protected set; }
        public DateTimeOffset Timestamp { get; protected set; }
        public string TenantId { get; protected set; }
        public Guid EntityId { get; protected set; }
        public string EventData { get; protected set; }
    }
}