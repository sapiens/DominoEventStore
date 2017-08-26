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
        public static readonly JsonSerializerSettings Settings=new JsonSerializerSettings()
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public static string PackEvents(params object[] events)
            => JsonConvert.SerializeObject(events.Select(d=> new EventWrap() { Type = d.GetType().AssemblyQualifiedName, Data = d }));

        public static IReadOnlyCollection<object> UnpackEvents(DateTimeOffset commitDate,string data,IDictionary<Type,IMapEventDataToObject> upcasters)
        {
            var d = JsonConvert.DeserializeObject<EventWrap[]>(data);
            return d.Select(c =>
            {
                var jo = c.Data as JObject;
                var type = Type.GetType(c.Type);
                var des=jo.ToObject(type);
                var upcast = upcasters.GetValueOrDefault(type);
                return upcast?.Map(c.Data, des, commitDate)?? des;                
            }).ToArray();            
            
        }

        

    }
}