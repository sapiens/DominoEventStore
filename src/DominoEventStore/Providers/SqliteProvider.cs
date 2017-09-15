namespace DominoEventStore.Providers
{
    public class SqliteProvider : ASqlDbProvider
    {
        protected override string GetInitStorageSql(string schema)
        {
            return "";
        }

        public SqliteProvider(IEventStoreSqlFactory db) : base(db)
        {
        }
    }
}