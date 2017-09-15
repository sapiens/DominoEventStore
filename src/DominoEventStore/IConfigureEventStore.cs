namespace DominoEventStore
{
    public interface IConfigureEventStore
    {
        IConfigureEventStore AddMapper<T>(AMapFromEventDataToObject<T> mapper) where T : class;
        IConfigureEventStore WithProvider(ISpecificDbStorage store,string schema=null);
    }
}