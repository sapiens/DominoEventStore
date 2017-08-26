using System;

namespace DominoEventStore
{
    public class QueryConfig
    {
        public QueryConfig()
        {
          
        }
        public string TenantId { get; set; }
        public Guid EntityId { get; set; }

        /// <summary>
        /// Commit version
        /// </summary>
        public int VersionStart { get; set; }

        public DateTimeOffset? DateStart { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
    }
}
