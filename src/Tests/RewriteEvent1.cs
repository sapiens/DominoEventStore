using System;
using DominoEventStore;

namespace Tests
{
    public class RewriteEvent1:ARewriteEvent<Event1>
    {
        public override Event1 Rewrite(dynamic jsonData, Event1 deserializedEvent, DateTimeOffset commitDate)
        {
            deserializedEvent.Name = "rewritten";
            return deserializedEvent;
        }
    }
}