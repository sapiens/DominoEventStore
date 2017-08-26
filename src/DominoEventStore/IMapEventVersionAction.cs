using System;

namespace DominoEventStore
{
    public interface IMapEventVersionAction<T,R>
    {
        IMapEventVersionCondition<T> Use(R value);
        IMapEventVersionCondition<T> Use(Func<R> valueAction);
    }
}