using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface IAdvancedFeatures
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newStorage"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        void MigrateEventsTo(IStoreEvents newStorage, string name, Action<IConfigMigration> config = null);
        void ResetStorage();
        void ImportCommit(Commit commits);
        /// <summary>
        /// You can't delete the default (<see cref="EventStore.DefaultTenant"/>) tenant 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        void DeleteTenant(string tenantId);

        /// <summary>
        /// Regenerates read model using the provided function. 
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="modelUpdater"></param>
        /// <param name="config">By default, all the events will be processed</param>
        /// <returns></returns>
        void GenerateReadModel(string operationName, Action<dynamic> modelUpdater, Action<IConfigReadModelGeneration> config = null);
    }
}