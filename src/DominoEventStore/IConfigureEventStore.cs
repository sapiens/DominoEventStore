
namespace DominoEventStore
{
    public interface IConfigureEventStore
    {
        /// <summary>
        /// Register mappers for existing data to events. Use it when the event structure changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        IConfigureEventStore AddMapper<T>(AMapFromEventDataToObject<T> mapper) where T : class;
        IConfigureEventStore WithProvider(ISpecificDbStorage store,string schema=null);    
    }
}