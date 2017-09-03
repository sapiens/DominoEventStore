using System;

namespace DominoEventStore
{
    public interface IConfigureQueryByDate
    {
        IConfigureQuery OlderThan(DateTimeOffset date);
        IConfigureQuery NewerThan(DateTimeOffset date);
        IConfigureQuery Between(DateTimeOffset start,DateTimeOffset end);
    }
}