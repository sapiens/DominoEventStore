using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using DominoEventStore;
using DominoEventStore.Providers;
using Ploeh.AutoFixture;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers;
using SqlFu.Providers.SqlServer;
using Utils = DominoEventStore.Utils;

namespace Tests
{
    public static class Setup
    {
        public static readonly EventStoreSettings EventStoreSettings=new EventStoreSettings()
        {
            
        };

        public const string TestSchema = "guest";

       public static ISpecificDbStorage GetDbFactory<T>() 
        {
            var provs=new Dictionary<Type,ISpecificDbStorage>()
            {
                {
                    typeof(SqlServerTests)
                    ,new SqlServerProvider(SqlFuManager.Config.CreateFactory<IEventStoreSqlFactory>(
                        new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection),SqlServerTests.ConnectionString))
                }
            };

            var option = provs[typeof(T)];
            option.Schema = "guest";
            SqlFuManager.Config.ConfigureTableForPoco<Commit>(d =>
            {
                d.Table=new TableName(ASqlDbProvider.CommitsTable,TestSchema);
                d.IdentityColumn = "Id";
            });
            SqlFuManager.Config.ConfigureTableForPoco<Snapshot>(d =>
            {
                d.Table=new TableName(ASqlDbProvider.SnapshotsTable,TestSchema);
                d.IdentityColumn = "Id";
            });
            return option;
        }

        public static readonly bool IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

        public static readonly Guid EntityId = Guid.NewGuid();

        public static IEnumerable<Commit> Commits(int count) => Commits<SomeEvent, SomeEvent>(count);
        public static Commit Commit(params object[] events) => new Commit("_",Guid.NewGuid(), Utils.PackEvents(events),Guid.NewGuid(), DateTimeOffset.Now, 1);

        public static IEnumerable<Commit> Commits<T,V>(int count) where T : class, new() where V : class, new()
        {
            return Enumerable.Range(1, count)
                .Select(i => new Commit("_", Setup.EntityId, Utils.PackEvents(new Object[]{new T(), new V()}), Guid.NewGuid(), DateTimeOffset.Now, i));
        }

        public static CommittedEvents CommittedEvents<T, V>(int count) where T : class, new() where V : class, new()
            => new CommittedEvents(Setup.Commits<T, V>(count).ToArray());

        public static IEnumerable<object> GetEvents(this Commit commit, IReadOnlyDictionary<Type, IMapEventDataToObject> upc)
        {
            return Utils.UnpackEvents(commit.Timestamp, commit.EventData, upc);
        }

        public static UnversionedCommit UnversionedCommit(string tenantId = "_", Guid? guid = null)
            =>
                new UnversionedCommit(tenantId, guid ?? Guid.NewGuid(), Utils.PackEvents(Events(1)), Guid.NewGuid(),
                    DateTimeOffset.Now);
        

        public static List<object> Events(int count=4)
        {
            var f = new Fixture();
            return Enumerable.Range(1, count)
                .Select(i => i % 2 == 1 ? (object)f.Create<Event1>() : f.Create<Event2>())
                .ToList();          
        }

        public static Snapshot Snapshot(int ver,Guid entity,string tenant="_")
        => new Snapshot(ver,entity,tenant,Utils.PackSnapshot(new Fixture().Create<SomeMemento>()),DateTimeOffset.Now);

        public static Func<Commit, IEnumerable<object>> EventDeserializerWIthoutUpcasting()
        {
            return c=>c.GetEvents(ImmutableDictionary<Type, IMapEventDataToObject>.Empty);
        }
    }

  

}