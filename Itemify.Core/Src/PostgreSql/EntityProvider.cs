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
            tableName = resolveTable<ItemEntity>(tableName);
            return postgreSql.Insert(tableName, entity, true);
        }
        public void Update(string tableName, ItemEntity entity)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            var affected = postgreSql.Update(tableName, entity, true);
            if (affected == 0)
                throw new EntitityNotFoundException(entity.Guid.ToString(), tableName);
        }
        public Guid Insert(string tableName, ItemEntity entity)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            return postgreSql.Insert(tableName, entity, false);
        }

        public IEnumerable<Guid> Insert(string tableName, IEnumerable<ItemEntity> entities)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            return postgreSql.BulkInsert(tableName, entities);
        }

        public void InsertItemRelations(string tableName, Guid guid,
            IEnumerable<KeyValuePair<Guid, string>> targetItems, string mappingTableName, bool overwrite)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);

            if (overwrite)
                postgreSql.Execute("DELETE FROM " + mappingTableName + " WHERE \"guid\" = @0", guid);

            var relations = targetItems.Select(k => new ItemRelationEntity()
            {
                Guid = guid,
                Table = tableName,
                TargetGuid = k.Key,
                TargetTable = resolveTable<ItemEntity>(k.Value)
            });

            postgreSql.BulkInsert(mappingTableName, relations);
        }

        public void Delete(string tableName, Guid guid)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            postgreSql.Execute("DELETE FROM " + tableName + " WHERE \"guid\" = @0", guid);
        }

        public IEnumerable<ItemEntity> QueryItemsByRelation(string tableName, Guid guid, string targetTableName, string mappingTableName)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            targetTableName = resolveTable<ItemEntity>(targetTableName);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);

            return postgreSql.Query<ItemEntity>($"SELECT * FROM {targetTableName} WHERE \"Guid\" IN (SELECT \"t_guid\" FROM {mappingTableName} WHERE \"guid\" = @0 AND \"table_name\" LIKE @1)", guid, tableName);
        }

        public ItemEntity QuerySingleItem(string tableName, Guid guid)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            return postgreSql.Query<ItemEntity>("SELECT * FROM " + tableName + " WHERE \"Guid\" = @0", guid).FirstOrDefault();
        }



        private string resolveTable<TSchema>(string tableName)
           where TSchema : IEntityBase
        {
            tableName = postgreSql.ResolveTableName(tableName);

            if (!tables.Contains(tableName))
            {
                tables.Add(tableName);

                if (!postgreSql.TableExists(tableName))
                {
                    log.Describe($"Create missing table: {tableName}");
                    postgreSql.CreateTable<TSchema>(tableName);
                }
            }

            return tableName;
        }
    }
}
