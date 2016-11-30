using System;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.ItemAccess
{
    public class ItemProvider
    {
        private PostgreSqlProvider postgreSql;

        internal ItemProvider(PostgreSqlProvider postgreSql)
        {
            this.postgreSql = postgreSql;
        }


    }
}
