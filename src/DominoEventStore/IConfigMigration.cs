namespace DominoEventStore
{
    public interface IConfigMigration
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">At least 100</param>
        /// <returns></returns>
        IConfigMigration BatchSize(int size);
        /// <summary>
        /// By default all commits from the store are migrated,
        /// Specify here if you want only one specific tenant to be migrated
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IConfigMigration OnlyTenant(string tenantId);

        IConfigMigration AddConverters(params IRewriteEventData[] converters);
    }
}