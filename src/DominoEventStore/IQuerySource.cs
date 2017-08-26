namespace DominoEventStore
{
    public interface IQuerySource
    {
        IQuerySource DefaultTenant();
        IQuerySource FromTenant();
    }
}