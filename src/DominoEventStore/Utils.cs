using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DominoEventStore
{
    public static class Utils
    {
        class EventWrap
        {
            public string Type { get; set; }
            public dynamic Data { get; set; }
        }

        private static readonly JsonSerializerSettings Settings=new JsonSerializerSettings()
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        private static readonly JsonSerializerSettings SnapshotSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public static string PackEvents(params object[] events)
            => JsonConvert.SerializeObject(events.Select(d=> new EventWrap() { Type = d.GetType().AssemblyQualifiedName, Data = d }),Formatting.Indented);

        public static IReadOnlyCollection<object> UnpackEvents(DateTimeOffset commitDate,string data,IReadOnlyDictionary<Type,IMapEventDataToObject> upcasters)
        {
            var d = JsonConvert.DeserializeObject<EventWrap[]>(data);
            return d.Select(c =>
            {
                var jo = c.Data as JObject;
                var type = Type.GetType(c.Type);
                var des=jo.ToObject(type);
                upcasters.TryGetValue(type, out var upcast);
                return upcast?.Map(c.Data, des, commitDate)?? des;                
            }).ToArray();            
            
        }

        public static string PackSnapshot(object memento)
        {
            return JsonConvert.SerializeObject(memento, SnapshotSerializerSettings);
        }

        public static object UnpackSnapshot(string snapData)
        {
            return JsonConvert.DeserializeObject(snapData,SnapshotSerializerSettings);
        }

    }

    public class EventStoreSettings
    {
        Dictionary<Type,IMapEventDataToObject> _eventMappers=new Dictionary<Type, IMapEventDataToObject>();

        public IReadOnlyDictionary<Type, IMapEventDataToObject> EventMappers => _eventMappers;
        
    }
}