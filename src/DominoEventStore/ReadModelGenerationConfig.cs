using System;

namespace DominoEventStore
{
    public class ReadModelGenerationConfig:IConfigReadModelGeneration
    {
        public string Name { get; }

        /// <summary>
        /// How many commits to process per batch. Default is 1000
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        public string TenantId { get; set; }

        public Guid? EntityId { get; set; }

        public ReadModelGenerationConfig(string name)
        {
            Name = name;
        }

        public IConfigReadModelGeneration ForTheDefaultTenant()
            => ForTenant(EventStore.DefaultTenant);

        public IConfigReadModelGeneration ForTenant(string id)
        {
            TenantId = id;
            return this;
        }

        public IConfigReadModelGeneration ForEntity(Guid id)
        {
            id.MustNotBeDefault();
            EntityId = id;
            return this;
        }

        public IConfigReadModelGeneration StartingWithDate(DateTimeOffset start)
        {
            StartDate = start;
            return this;
        }

        public DateTimeOffset? StartDate { get; set; }
      
     
    }
}