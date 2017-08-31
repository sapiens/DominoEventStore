using System;

namespace DominoEventStore
{
    public interface IConfigureQuery
    {
        /// <summary>
        /// Ignored if entity is not set
        /// </summary>
        /// <param name="commitVersion"></param>
        void FromBeginningUntilVersion(int commitVersion);
        /// <summary>
        /// Default is <see cref="EventStore.DefaultTenant"/>
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IConfigureQuery OfTenant(string tenantId);
                
        IConfigureQuery OfEntity(Guid entityId);
        IConfigureQueryByDate WithCommitDate { get; }

        /// <summary>
        /// Ignored if no entity is specified
        /// </summary>
        /// <param name="include"></param>
        /// <returns></returns>
        IConfigureQuery IncludeSnapshots(bool include);
    }

    public interface IConfigureQueryByDate
    {
        IConfigureQuery OlderThan(DateTimeOffset date);
        IConfigureQuery NewerThan(DateTimeOffset date);
        IConfigureQuery Between(DateTimeOffset start,DateTimeOffset end);
    }
}