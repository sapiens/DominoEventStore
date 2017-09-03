using System;

namespace DominoEventStore
{
    public class SomeDataMapper : AMapFromEventDataToObject<SomeData>
    {
        public override SomeData Map(dynamic jsonData, SomeData deserializedEvent, DateTimeOffset commitDate)
        {
            throw new NotImplementedException();
        }
    }
}