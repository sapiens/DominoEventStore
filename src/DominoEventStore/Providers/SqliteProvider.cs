using System;
using SqlFu;

namespace DominoEventStore.Providers
{
    public class SqliteProvider : ASqlDbProvider
    {
        protected override string DuplicateCommmitMessage { get; } = "CommitId";

        protected override string DuplicateVersion { get; } = "Version";

        protected override string GetInitStorageSql(string schema)
        {
        
            return $@"

CREATE TABLE if not exists `{CommitsTable}`(
`Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
, `TenantId` TEXT NOT NULL
, `EntityId` TEXT NOT NULL
, `CommitId` TEXT NOT NULL 
, `EventData` TEXT NOT NULL
, `Timestamp` TEXT NOT NULL
, `Version` INTEGER NOT NULL 
, unique (`EntityId`,'CommitId')
               );
CREATE INDEX if not exists `IX_Commits_Cid` ON `{CommitsTable}` (`CommitId` ASC);
CREATE INDEX if not exists `IX_Commits_Ver` ON `{CommitsTable}` (`EntityId` ,`Version` );

CREATE TABLE if not exists `{SnapshotsTable}` 
( 
`Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `TenantId` TEXT NOT NULL, `EntityId` TEXT NOT NULL, `Version` INTEGER NOT NULL, `SerializedData` TEXT NOT NULL, `SnapshotDate` TEXT NOT NULL 
);
CREATE INDEX if not exists `IX_SNapshots_Ver` ON `{SnapshotsTable}` (`EntityId` ,`Version` );
CREATE TABLE if not exists `{BatchTable}`  ( `Name` TEXT NOT NULL, `Skip` INTEGER NOT NULL );
"
;
        }

        public SqliteProvider(IDbFactory db) : base(db)
        {
        }
    }
}