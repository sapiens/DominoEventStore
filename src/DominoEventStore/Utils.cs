using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoEventStore
{
    public static class Utils
    {
        public class JsonWrap
        {
            public string Type { get; set; }
            public dynamic Data { get; set; }

            public JsonWrap()
            {
                
            }

            public JsonWrap(object data)
            {
                data.MustNotBeNull();
                Type = data.GetType().AssemblyQualifiedName;
                Data = data;
            }
        }


     public static string PackEvents(IEnumerable<object> events)
            => ToJson(events.Select(d => new JsonWrap(d)));

        public static IReadOnlyCollection<object> UnpackEvents(DateTimeOffset commitDate, string data, IReadOnlyDictionary<Type, IMapEventDataToObject> upcasters)
        {
            var d = Utf8Json.JsonSerializer.Deserialize<JsonWrap[]>(data);
            return d.Select(c =>
            {
                var des = DynamicToObject(c);
                var type = des.GetType();
                                    
                upcasters.TryGetValue(type, out var upcast);
                return upcast?.Map(c.Data, des, commitDate) ?? des;
            }).ToArray();

        }

       
        
        static object DynamicToObject(JsonWrap w) =>
            Utf8Json.JsonSerializer.NonGeneric.Deserialize(Type.GetType(w.Type), Utf8Json.JsonSerializer.NonGeneric.Serialize(w.Data));

        static string ToJson(object o) => Utf8Json.JsonSerializer.PrettyPrint(Utf8Json.JsonSerializer.Serialize(o));

        public static string PackSnapshot(object memento)
        {
            memento.MustNotBeNull();
            return ToJson(new JsonWrap(memento));            
        }

        public static object UnpackSnapshot(string snapData)
        {
            var wrap = Utf8Json.JsonSerializer.Deserialize<JsonWrap>(snapData);
            return DynamicToObject(wrap);            
        }

    }
}