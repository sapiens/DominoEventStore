namespace DominoEventStore
{
    public interface IRewriteEventData
    {
        bool CanHandle();
        object Rewrite(JsonedEvent evnt);
    }
}