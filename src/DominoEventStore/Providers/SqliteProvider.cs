using System;

namespace DominoEventStore.Providers
{
    public class SqliteProvider : ASqlDbProvider
    {
        protected override string GetInitStorageSql(string schema)
        {
          //  schema = schema.IsNullOrEmpty()? "main":schema;


            return $@"

CREATE TABLE if not exists `{schema}`.`{CommitsTable}`(
`Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
, `TenantId` TEXT NOT NULL
, `EntityId` TEXT NOT NULL
, `CommitId` TEXT NOT NULL UNIQUE
, `EventData` TEXT NOT NULL
, `Timestamp` TEXT NOT NULL
, `Version` INTEGER NOT NULL 
               );
CREATE INDEX `IX_Commits_Cid` ON `{schema}`.`{CommitsTable}` (`CommitId` ASC);
CREATE INDEX `IX_Commits_Ver` ON `{schema}`.`{CommitsTable}` (`EntityId` ,`Version` );

CREATE TABLE if not exists `{schema}`.`{SnapshotsTable}` 
( 
`Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `TenantId` TEXT NOT NULL, `EntityId` TEXT NOT NULL, `Version` INTEGER NOT NULL, `SerializedData` TEXT NOT NULL, `SnapshotDate` TEXT NOT NULL 
);
CREATE INDEX `IX_SNapshots_Ver` ON `{schema}`.`{SnapshotsTable}` (`EntityId` ,`Version` );
CREATE TABLE if not exists `{schema}`.`{BatchTable}`  ( `Name` TEXT NOT NULL, `Skip` INTEGER NOT NULL );
"
;
        }

        public SqliteProvider(IEventStoreSqlFactory db) : base(db)
        {
        }
    }
}