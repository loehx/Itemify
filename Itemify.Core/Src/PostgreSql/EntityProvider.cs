using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.ItemAccess.Entities;

namespace Itemify.Core.PostgreSql
{
    internal class EntityProvider
    {
        private PostgreSqlProvider postgreSql;
        private readonly EntityProviderLog log;
        private readonly SortedSet<string> tables = new SortedSet<string>();

        internal EntityProvider(PostgreSqlProvider postgreSql, EntityProviderLog log)
        {
            this.postgreSql = postgreSql;
            this.log = log;
        }

        public Guid Upsert(string tableName, ItemEntity entity)
        {
            tableName = resolveTable(tableName);
            return postgreSql.Insert(tableName, entity, true);
        }

        public void Delete(string tableName, Guid guid)
        {
            tableName = resolveTable(tableName);
            postgreSql.Execute("DELETE FROM " + tableName + " WHERE \"guid\" = @0", guid);
        }

        public IEnumerable<ItemEntity> QueryMulti(string tableName, Guid guid)
        {
            tableName = resolveTable(tableName);
            return postgreSql.Query<ItemEntity>("SELECT * FROM " + tableName + " WHERE \"Guid\" = @0", guid);
        }

        public ItemEntity QuerySingle(string tableName, Guid guid)
        {
            tableName = resolveTable(tableName);
            return postgreSql.Query<ItemEntity>("SELECT * FROM " + tableName + " WHERE \"Guid\" = @0", guid).FirstOrDefault();
        }


        private string resolveTable(string tableName)
        {
            tableName = postgreSql.ResolveTableName(tableName);

            if (!tables.Contains(tableName))
            {
                tables.Add(tableName);

                if (!postgreSql.TableExists(tableName))
                {
                    log.CreateTable(tableName);
                    postgreSql.CreateTable<ItemEntity>(tableName);
                }
            }

            return tableName;
        }

    }
}
