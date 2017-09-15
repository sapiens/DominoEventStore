using System;
using System.Data.Common;
using SqlFu;

namespace DominoEventStore.Providers
{
    public interface IConfigureSqlFu
    {
        IConfigureSqlFu IntegrateWith(SqlFuConfig config);
        IConfigureSqlFu WithMSSql(string cnx, Func<DbConnection> factory, string schema = null);

    }
}