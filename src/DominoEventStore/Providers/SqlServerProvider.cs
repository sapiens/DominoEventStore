using System;
using SqlFu;

namespace DominoEventStore.Providers
{
    public class SqlServerProvider : ASqlDbProvider
    {
        public SqlServerProvider(IDbFactory db) : base(db)
        {
        }


        protected override string GetInitStorageSql(string schema)
        {
            schema = schema ?? "dbo";


            return $@"
IF not EXISTS(SELECT 1 FROM sys.Objects WHERE  Object_id = OBJECT_ID(N'{schema}.{CommitsTable}') or Object_id =OBJECT_ID(N'{schema}.{SnapshotsTable}')  AND Type = N'U')
begin
CREATE TABLE [{schema}].[{CommitsTable}](
                [Id][int] IDENTITY(1,1) NOT NULL,

                [TenantId] [varchar] (75) NOT NULL,

                [EntityId] [uniqueidentifier]
            NOT NULL,

                [CommitId] [uniqueidentifier]
            NOT NULL,

                [EventData] [nvarchar] (max) NOT NULL,
                [Timestamp] [datetimeoffset] (7) NOT NULL,
 
                [Version] [int] NOT NULL,
                CONSTRAINT[PK_Commits] PRIMARY KEY CLUSTERED
                (
                [Id] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY],
            CONSTRAINT[IX_Commits_Cid] UNIQUE NONCLUSTERED
            (
                [CommitId] ASC
            )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY],
            CONSTRAINT[IX_Commits_Ver] UNIQUE NONCLUSTERED
            (
                [EntityId] ASC,
                [Version] ASC
            )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                ) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY];
CREATE TABLE [{schema}].[{SnapshotsTable}](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenantId] [varchar](75) NOT NULL,
	[EntityId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[SerializedData] [nvarchar](max) NOT NULL,
	[SnapshotDate] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_SNapshots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
CREATE UNIQUE NONCLUSTERED INDEX [IX_SNapshots_Ver] ON [{schema}].[{SnapshotsTable}]
(
	[EntityId] ASC,
	[Version] ASC
)
;
CREATE TABLE [{schema}].[{BatchTable}](
	[Name] [varchar](50) NOT NULL,
	[Skip] [bigint] NOT NULL
) ON [PRIMARY]
end
"
;
        }
    }

   
 
}