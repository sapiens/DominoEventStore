using System;
using System.Collections.Generic;

namespace DominoEventStore
{
    public class SomeDataMapper : AMapFromEventDataToObject<SomeData>
    {
        public override SomeData Map(IDictionary<string, object> existingData, SomeData deserializedEvent,
            DateTimeOffset commitDate)
        {
            throw new NotImplementedException();
        }
    }
}