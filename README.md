# Domino Event Store

[![Appveyor stat](https://ci.appveyor.com/api/projects/status/github/sapiens/dominoeventstore?svg=true)](https://ci.appveyor.com/project/sapiens/dominoeventstore) [![NuGet](https://img.shields.io/nuget/v/DominoES.svg)](https://www.nuget.org/packages/DominoES)


An alternative to the good but somewhat outdated [NEventStore](https://github.com/NEventStore/NEventStore/wiki/Quick-Start), DominoES is inspired by Greg Young's [excellent book](https://leanpub.com/esversioning/read#leanpub-auto-weak-schema) and it's designed to be easy to use, lightweight and versatile.

It sits on top of an existing RDBMS (SqlServer, Sqlite), providing an event store for small to medium applications that are using Event Sourcing and don't want to use a separated, dedicated Event Store solution. Great to get you started with ES as it's easy to setup (just Nuget-it) and it takes at most 5 minutes to learn how to use it.

## Features

* Domino ES is a netstandard 2.0 library, therefore it runs on multiple platforms such as .Net Core  and .Net Framework 4.6+
* Multi-tenant support
* Bulk read model generation assistance
* Easy migrations and event data rewriting support (for those special cases)
* Easy to setup/use
* Great for small to medium non-distributed apps. 

## Beta Breaking Changes!

1.0.0-beta8 -> 1.0.0-beta9

* Changed how Snapshot data is serialiazed
* Due to using a different json lib, currently stored events might not deserialize properly
* Renamed the `Complete` method to `Commit`
* Netstandard 2.0

## Usage

Configuration is done inside the `Build` function. At a minimum you need to specify what db provider to use. DominoES uses Serilog for its logging, but you still have to specify sinks.

```csharp
//create and configure the event store singleton . Add it as singleton to your favourite DI Container
var eventStore=EventStore.WithLogger(/* Serilog instance */).Build(c =>
                    {
                        c.UseMSSql(SqlClientFactory.Instance.CreateConnection,ConnectionString);
                        //or
                         c.UseSqlite(SQLiteFactory.Instance.CreateConnection,ConnectionString);
                    }
                     );


//use it in your app services

//add events
await _store.Append(myEntityId,commitId,myEvents);

//commit events from more than one aggregate
  using (var t=_store.StartCommit(commitId))
            {
                t.Append(entity1,events1);
                t.Append(entity2,events2);
                await t.Complete().ConfigureFalse();
            }

//query events
var evs=await _store.GetEvents(myEntityId);

//advanced query
var evs=await _store.GetEvents(q=>q.WithCommitDate.OlderThan(myDate).IncludeSnapshots(false).OfEntity(myEntityId).FromBeginningUntilVersion(someAggregateVersion));

```

## Advanced Usage

## When domain events change

A normal occurrence in any domain, some business events do change over time. It's important to identify first if we're dealing with the same event with slightly changed structure, or the business semantics have changed. In case of the latter, just create a new specific event.

In case of the former, the traditional approach is to up-cast existing events at the app level, which introduces a new layer of complexity inside the app. Another method is to map directly the old event data to the new event. DominoES is designed around this technique, allowing you to keep the codebase maintainable. Focusing on data, instead of the event itself,it allows further advanced and edge scenarios, such as migrating an old event store.

```csharp

//define mapper, it will be treated as a singleton
 class MyMapper : AMapFromEventDataToObject<SomeEvent>
        {
            public override SomeEvent Map(dynamic oldData, SomeEvent newEvent, DateTimeOffset commitDate)
            {
                //change values of newEvent
                return newEvent;
            }
        }

//register it when configuring DominoES

 EventStore.Build(c =>
            {
                c.UseMSSql(SqlClientFactory.Instance.CreateConnection, SqlServerTests.ConnectionString);
                c.AddMapper(new MyMapper());
            });

```

## Read model generation

Sometimes you need to (re)generate read models from existing events. DominoES makes your life easier in this regard. I suggest to have a `ReadModelUpdater` kind of class consisting of `Handle(event)` methods.

```csharp

 public class ReadModelUpdater
    {
     
        public void Handle(TransactionDeleted ev)
        {
        }

        public void Handle(CashFlowEntryUndone ev)
        {

        }
 
         /** etc **/
    }

var updater=new ReadModelUpdater();
_store.Advanced.GenerateReadModel("balanceSheet",e=> updater.Handle(e));

```

## Store migration

As your app evolves, at one point you might need to move the existing events. One useful but **dangerous** (but _very handy_) feature is support for event rewriting. No, I don't mean changing the past, I mean rewriting the event data for technical or legal reasons. It's an edge case, a feature that shouldn't be used lightly, but nevertheless, if you need it, it will make your life easier.

```csharp

//moving from sqlite to sql server
 _dest = EventStore.Build(c =>
            {
                c.UseMSSql(SqlClientFactory.Instance.CreateConnection, SqlServerTests.ConnectionString);                
            });

            
    _src = EventStore.Build(c =>
                c.UseSqlite(SQLiteFactory.Instance.CreateConnection, SqliteTests.ConnectionString));

//optional event rewriter
  public class RewriteEvent : ARewriteEvent<Event1>
        {
            public override Event1 Rewrite(dynamic jsonData, Event1 deserializedEvent, DateTimeOffset commitDate)
            {
                deserializedEvent.Nr += 60;
                return deserializedEvent;
            }
        }

//do migration
 _src.Advanced.MigrateEventsTo(_dest, "migration",c=>c.AddConverters(new RewriteEvent()));

```

**Note**: Both model generation and store migration are long running processes. These features shouldn't be used inside your main app. Create a console app or cloud job (Azure function, AWS lambda etc) to use them.
