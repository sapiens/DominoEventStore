using DominoEventStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Utf8Json;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class JsonStuff
    {
        private readonly ITestOutputHelper _w;
        private static MyEvent[] _myEvents = new[] { new MyEvent(), new MyEvent() { Name = "Strula" } };

        public JsonStuff(ITestOutputHelper w)
        {
            _w = w;
        }
        [Fact]
        public void pack_unpack_events()
        {
        
            var ts = new Stopwatch();
            ts.Start();
            var ser = PackEvents();
            _w.WriteLine($"Packing in {ts.Elapsed}");
            ts.Restart();
            var events = Utils.UnpackEvents(DateTimeOffset.Now, ser, new Dictionary<Type, IMapEventDataToObject>());
            _w.WriteLine($"Unpacking in {ts.Elapsed}");
            ts.Stop();
            events.Count.Should().Be(2);
            var last = events.Skip(1).First().CastAs<MyEvent>();
            last.Name.Should().Be("Strula");
            last.Enum.Should().Be(MyEnum.First);
            last.Age.Should().Be(23);
        }

        private static string PackEvents()
        {
            var ser = Utils.PackEvents(_myEvents);
            return ser;
        }


       
        [Fact]
        public void pack_unpack_events_with_upcasting()
        {
            var ser = Utils.PackEvents(new[] { new MyEvent() { Age = 23 }, new MyEvent() { Name = "Strula", Age = 15 } });
            var events = Utils.UnpackEvents(DateTimeOffset.Now, ser, new Dictionary<Type, IMapEventDataToObject>()
            {
                { typeof(MyEvent),new MyEventUpcase()}
            });
            var last = events.Skip(1).First().CastAs<MyEvent>();
            var first = events.First().CastAs<MyEvent>();
            first.Age.Should().Be(33);
            last.Name.Should().Be("Strula");
            last.Age.Should().Be(25);
        }


        [Fact]
        public void pack_unpack_memento()
        {
            var memento = new MyMemento() { Age = 23, Data = "Hi" };
            var data = Utils.PackSnapshot(memento);
            var unpackSnapshot = Utils.UnpackSnapshot(data);
            var des = unpackSnapshot as MyMemento;
            des.Should().BeEquivalentTo(memento);
        }

        public class MyMemento
        {
            public int Age { get; set; }
            public string Data { get; set; }
            public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

        }

        public enum MyEnum
        {
            None,
            First, Second
        }

        public class MyEvent
        {
            public string Name { get; set; } = "Bula";
            public MyEnum Enum { get; set; } = MyEnum.First;
            public Guid SomeId { get; set; } = Guid.NewGuid();
            public int Age { get; set; } = 23;
            public DateTime Date { get; set; } = DateTime.Now;
        }

        public class MyEventUpcase : AMapFromEventDataToObject<MyEvent>
        {
            
            public override MyEvent Map(IDictionary<string, object> existingData, MyEvent deserializedEvent,
              DateTimeOffset commitDate)
            {
                var age = Convert.ToInt32(existingData["Age"]);
                deserializedEvent.Age = age + 10;
                return deserializedEvent;
            }
        }
    }
}
