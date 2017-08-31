using System;

namespace DominoEventStore
{
    public class QueryConfig:IConfigureQuery,IConfigureQueryByDate
    {
        public string TenantId { get; set; }
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Commit version
        /// </summary>
        public int VersionStart { get; set; } = 1;
        public int? VersionEnd { get; set; }
        public bool IgnoreSnapshots { get; set; } = true;
        public DateTimeOffset? DateEnd { get; set; }
        public DateTimeOffset? DateStart { get; set; }

        public IConfigureQueryByDate WithCommitDate => this;

        public void FromBeginningUntilVersion(int commitVersion)
        {
            commitVersion.Must(d=>d>0);
            VersionEnd = commitVersion;
        }

    
        IConfigureQuery IConfigureQuery.IncludeSnapshots(bool include)
        {
            IgnoreSnapshots = !include;
            return this;
        }

        public IConfigureQuery OfTenant(string tenantId)
        {
            tenantId.MustNotBeEmpty();
            TenantId = tenantId;
            return this;
        }

        public IConfigureQuery OfEntity(Guid entityId)
        {
            entityId.MustNotBeDefault();
            EntityId = entityId;
            return this;
        }

        public IConfigureQuery OlderThan(DateTimeOffset date)
        {
            DateEnd = date;
            return this;
        }

        public IConfigureQuery NewerThan(DateTimeOffset date)
        {
            DateStart = date;
            return this;
        }

        public IConfigureQuery Between(DateTimeOffset start, DateTimeOffset end)
        {
            this.DateStart = start;
            DateEnd = end;
            return this;
        }
    }
}
