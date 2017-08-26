using System;
using System.Linq.Expressions;

namespace DominoEventStore
{
    public interface IMapEventVersionCondition<T>
    {
        IMapEventVersionAction<T,R> WhenMissingColumn<R>(Expression<Func<T, R>> column);
    }
}