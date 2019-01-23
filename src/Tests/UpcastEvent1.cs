using System;
using System.Collections.Generic;
using DominoEventStore;

namespace Tests
{
    public class UpcastEvent1 : AMapFromEventDataToObject<Event1>
    {
        public override Event1 Map(IDictionary<string, object> existingData, Event1 deserializedEvent,
            DateTimeOffset commitDate)
        {
            deserializedEvent.Nr += 10;
            return deserializedEvent;
        }
    }
}