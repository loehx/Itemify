using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Exceptions;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Shared.Logging;

namespace Itemify.Core.PostgreSql
{
    internal class EntityProvider
    {
        private PostgreSqlProvider postgreSql;
        private readonly ILogWriter log;
        private readonly SortedSet<string> tables = new SortedSet<string>();

        internal EntityProvider(PostgreSqlProvider postgreSql, ILogWriter log)
        {
            this.postgreSql = postgreSql;
            this.log = log;
        }

        public Guid Upsert(string tableName, ItemEntity entity)
        {
            tableName = resolveTable(tableName);
            return postgreSql.Insert(tableName, entity, true);
        }
        public void Update(string tableName, ItemEntity entity)
        {
            tableName = resolveTable(tableName);
            var affected = postgreSql.Update(tableName, entity, true);
            if (affected == 0)
                throw new EntitityNotFoundException(entity.Guid.ToString(), tableName);
        }
        public Guid Insert(string tableName, ItemEntity entity)
        {
            tableName = resolveTable(tableName);
            return postgreSql.Insert(tableName, entity, false);
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
                    log.Describe($"Create missing table: {tableName}");
                    postgreSql.CreateTable<ItemEntity>(tableName);
                }
            }

            return tableName;
        }
    }
}
