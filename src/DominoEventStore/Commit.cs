using System;

namespace DominoEventStore
{
    public class Commit:UnversionedCommit
    {
        public Commit()
        {
            
        }

        public Commit(string tenantId, Guid entityId, string eventData, Guid commitId, DateTimeOffset timestamp,int version) : base(tenantId, entityId, eventData, commitId, timestamp)
        {
            Version = version;
        }

        /// <summary>
        /// Entity version
        /// </summary>
        public int Version { get; private set; }
              
    }
}