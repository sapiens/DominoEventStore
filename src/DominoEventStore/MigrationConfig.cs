using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class MigrationConfig:IConfigMigration
    {
        public MigrationConfig(string name)
        {
            Name = name;
        }

        /// <summary>
        /// How many commits to process per batch. Default is 1000
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        public string TenantId { get; set; }

        public string Name { get; private set; }

        public List<IRewriteEventData> Converters { get; }=new List<IRewriteEventData>();
        IConfigMigration IConfigMigration.BatchSize(int size)
        {
            //size.Must(d=>d>100);
            BatchSize = size;
            return this;
        }

        public IConfigMigration OnlyTenant(string tenantId)
        {
            TenantId = tenantId;
            return this;
        }

        public IConfigMigration AddConverters(params IRewriteEventData[] converters)
        {
            Converters.AddRange(converters);
            return this;
        }
    }
}