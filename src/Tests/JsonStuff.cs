using DominoEventStore;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class JsonStuff
    {
        private readonly ITestOutputHelper _w;

        public JsonStuff(ITestOutputHelper w)
        {
            _w = w;
        }
        [Fact]
        public void pack_unpack_events()
        {
            var ser = PackEvents();
            var events=Utils.UnpackEvents(DateTimeOffset.Now,ser,new Dictionary<Type, IMapEventDataToObject>());
            events.Count.Should().Be(2);
            var last = events.Skip(1).First().CastAs<MyEvent>();
            last.Name.Should().Be("Strula");
            last.Age.Should().Be(23);
        }

        private static string PackEvents()
        {
            var ser = Utils.PackEvents(new []{new MyEvent(), new MyEvent() {Name = "Strula"}});
            //_w.WriteLine(s);
            return ser;
        }

        [Fact]
        public void pack_unpack_events_with_upcasting()
        {
            var ser=Utils.PackEvents(new []{new MyEvent(){Age = 23}, new MyEvent(){Name = "Strula",Age = 15}});        
            var events=Utils.UnpackEvents(DateTimeOffset.Now,ser,new Dictionary<Type, IMapEventDataToObject>()
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
            var memento = new MyMemento() {Age = 23, Data = "Hi"};
            var data = Utils.PackSnapshot(memento);
            var unpackSnapshot = Utils.UnpackSnapshot(data);
            var des = unpackSnapshot as MyMemento;
            des.ShouldBeEquivalentTo(memento);            
        }

        public class MyMemento
        {
            public int Age { get; set; }
            public string Data { get; set; }
            public DateTimeOffset Date { get; set; }=DateTimeOffset.Now;

        }
      public  class MyEvent
        {
            public string Name { get; set; } = "Bula";
            public Guid SomeId { get; set; }=Guid.NewGuid();
            public int Age { get; set; } = 23;
            public DateTime Date { get; set; }
        }

      public class MyEventUpcase : AMapFromEventDataToObject<MyEvent>
        {
            public override MyEvent Map(dynamic jsonData, MyEvent deserializedEvent, DateTimeOffset commitDate)
            {
                deserializedEvent.Age = jsonData.Age+10;
                return deserializedEvent;
            }
        }
    }
}
