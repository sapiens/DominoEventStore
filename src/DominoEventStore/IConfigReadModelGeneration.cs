using System;

namespace DominoEventStore
{
    public interface IConfigReadModelGeneration
    {
        IConfigReadModelGeneration ForTheDefaultTenant();
        IConfigReadModelGeneration ForTenant(string id);
        IConfigReadModelGeneration ForEntity(Guid id);
        IConfigReadModelGeneration StartingWithDate(DateTimeOffset start);
        

    }
}