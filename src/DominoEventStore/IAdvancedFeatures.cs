using System;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface IAdvancedFeatures
    {
        Task WriteEventsTo(IStoreEvents newStorage, params IRewriteEventData[] converters);
        Task ResetStorage();
        Task DeleteTenant(string tenantId);
        /// <summary>
        /// Regenerates read model using the provided function
        /// </summary>
        /// <param name="modelUpdater"></param>
        /// <param name="tenantId">If not specified it's all tenants. Use <see cref="DefaultTenant"/>  for the default tenant</param>
        /// <param name="entityId">If not specified it's all entities</param>
        /// <returns></returns>
        Task GenerateReadModel(Func<dynamic, Task> modelUpdater,string tenantId="",Guid? entityId=null);
    }
}