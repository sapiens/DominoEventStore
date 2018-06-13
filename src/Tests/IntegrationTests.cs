
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using CavemanTools.Logging;
using DominoEventStore;
using NSubstitute.Exceptions;
using SqlFu;
using SqlFu.Providers.SqlServer;


namespace Tests
{
    public class IntegrationTests:IDisposable
    {
        private const int MaxEntities = 500;
        private IStoreEvents _dest;
        private IStoreEvents _src;
        Fixture _fixture=new Fixture();
        private List<object> _events=new List<object>();
        private List<Guid> _entities;


        public IntegrationTests()
        {
            LogManager.OutputTo(s=>Trace.WriteLine(s));
            
            _dest = EventStore.Build(c =>
            {
                c.UseMSSql(SqlClientFactory.Instance.CreateConnection, SqlServerTests.ConnectionString);                
            });

            
            _src = EventStore.Build(c =>
                c.UseSqlite(SQLiteFactory.Instance.CreateConnection, SqliteTests.ConnectionString));
        }

       
        async Task SetupSrc()
        {
            _entities=new List<Guid>();
            for (var i = 0; i < MaxEntities;i++)
            {
                var id = Guid.NewGuid();
                _entities.Add(id);
                var @event = i%2==0?(object)_fixture.Create<Event1>():_fixture.Create<Event2>();
                _events.Add(@event);
                await _src.Append(id, Guid.NewGuid(),@event);
            }
            
        }


        public class RewriteEvent : ARewriteEvent<Event1>
        {
            public override Event1 Rewrite(dynamic jsonData, Event1 deserializedEvent, DateTimeOffset commitDate)
            {
                deserializedEvent.Nr += 60;
                return deserializedEvent;
            }
        }

      [Fact]
        public async Task migrate_from_sqlite_to_sqlserver()
        {
            await SetupSrc();
            loop:
            var i = new Random().Next(0,MaxEntities);
            
            _src.Advanced.MigrateEventsTo(_dest, "bubu",c=>c.BatchSize(50).AddConverters(new RewriteEvent()));
            var evs = await _dest.GetEvents(_entities[i]);
            evs.Value.Count.Should().Be(1);
            var orig = _events[i].CastAs<Event1>();
            if (orig==null) goto loop;
            
            var ev = evs.Value.First() as Event1;
            ev.Nr.Should().Be(60+orig.Nr);            
        }

        [Fact]
        public async Task regenerate_read_model()
        {
            await SetupSrc();
            var count = 0;
            _src.Advanced.GenerateReadModel("gen", ev =>
            {
                count++;
            });
            count.Should().Be(MaxEntities);
        }

        public void Dispose()
        {
            _src.Advanced.ResetStorage();
            _dest.Advanced.ResetStorage();
        }
    }
} 
