using System;

namespace DominoEventStore
{
    public interface IRewriteEventData
    {
        Type HandledType { get; }
        object Rewrite(dynamic jsonData, object deserializedEvent, DateTimeOffset commitDate);
    }
}