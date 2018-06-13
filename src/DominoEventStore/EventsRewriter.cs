using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoEventStore
{
    class EventsRewriter
    {
        private readonly IReadOnlyDictionary<Type, IMapEventDataToObject> _mapps;

        public EventsRewriter(IEnumerable<IRewriteEventData> rewrites, IReadOnlyDictionary<Type, IMapEventDataToObject> mapps)
        {
            _mapps = CreateMappersFromRewriters(rewrites, mapps);
        }
        public Commit Rewrite(Commit commit)
        {
            var evs = Utils.UnpackEvents(commit.Timestamp, commit.EventData, _mapps);
            return new Commit(commit.TenantId,commit.EntityId,Utils.PackEvents(evs),commit.CommitId,commit.Timestamp,commit.Version);
        }

        Dictionary<Type, IMapEventDataToObject> CreateMappersFromRewriters(IEnumerable<IRewriteEventData> rew, IReadOnlyDictionary<Type, IMapEventDataToObject> mapps)
        {
            var rez = new Dictionary<Type, IMapEventDataToObject>();
            foreach (var r in rew)
            {

                if (mapps.ContainsKey(r.HandledType))
                {
                    rez.Add(r.HandledType,new LambdaMap(r.HandledType,mapps[r.HandledType],r));
                }
                else
                {
                    rez.Add(r.HandledType, new LambdaMap(r.HandledType, rew: r));
                }
            }

            foreach (var kv in mapps.Where(d => !rez.ContainsKey(d.Key)))
            {
                rez.Add(kv.Key,kv.Value);
            }
            return rez;
        }

        class LambdaMap : IMapEventDataToObject
        {
            private readonly Type _type;
            private readonly IMapEventDataToObject _mapr;
            private readonly IRewriteEventData _rew;


            public LambdaMap(Type type,IMapEventDataToObject mapr=null,IRewriteEventData rew=null)
            {
                _type = type;
                _mapr = mapr;
                _rew = rew;
            }

            public bool Handles(Type type)
                => type == _type;

            public object Map(dynamic existingData, object deserializedEvent, DateTimeOffset commitDate)
            {
                var rez= _mapr?.Map(existingData, deserializedEvent, commitDate)??deserializedEvent;
                return _rew?.Rewrite(existingData, rez, commitDate) ?? rez;
            }
        }
    }
}