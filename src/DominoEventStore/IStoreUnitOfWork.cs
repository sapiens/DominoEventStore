using System;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface IStoreUnitOfWork:IDisposable
    {
        void Append(string tenantId, Guid entityId, params object[] events);
        void Append(Guid entityId, params object[] events);
        Task Commit();
    }
}