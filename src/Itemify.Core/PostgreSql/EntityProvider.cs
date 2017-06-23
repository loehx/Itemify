using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Itemify.Core.Exceptions;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Logging;
using Itemify.Shared.Utils;

namespace Itemify.Core.PostgreSql
{
    // TODO: Make internal
    public class EntityProvider
    {
        private PostgreSqlProvider postgreSql;
        private readonly ILogWriter log;
        private readonly SortedSet<string> tables = new SortedSet<string>();

        public EntityProvider(PostgreSqlProvider postgreSql, ILogWriter log)
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
                postgreSql.Execute("DELETE FROM " + mappingTableName + " WHERE \"guid\" = @0 AND \"table_name\" LIKE @1", guid, tableName);

            var relations = targetItems.Select(k => new ItemRelationEntity()
            {
                Guid = guid,
                Table = tableName,
                TargetGuid = k.Key,
                TargetTable = resolveTable<ItemEntity>(k.Value)
            });

            postgreSql.BulkInsert(mappingTableName, relations);
        }

        public void DeleteItemRelations(string tableName, Guid guid, string mappingTableName, IEnumerable<string> targetTables)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);
            var parameters = new object[] { guid, tableName }
                .Concat(targetTables.Select(resolveTable<ItemEntity>))
                .ToArray();

            postgreSql.Execute("DELETE FROM " + mappingTableName +
                               $" WHERE \"guid\" = @0 AND \"table_name\" LIKE @1 AND \"t_table_name\" IN ({ parameters.Length.Times().Skip(2).Select(i => "@" + i).Join(", ") })", parameters);
        }

        public void Delete(string tableName, Guid guid)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            var affected = postgreSql.Execute($"DELETE FROM {tableName} WHERE \"Guid\" = @0", guid);
            log.Describe($"Deleted {affected} item(s) successfully.");
        }

        public IEnumerable<ItemEntity> QueryItemsByRelation(string tableName, Guid guid, string targetTableName, string mappingTableName, bool bidirectional)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            targetTableName = resolveTable<ItemEntity>(targetTableName);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);

            if (bidirectional)
            {
                // TODO: Put an INDEX on both guid+table_name and t_guid+t_table_name
                return postgreSql.Query<ItemEntity>($"SELECT * FROM {targetTableName} WHERE \"Guid\" IN (SELECT \"t_guid\" FROM {mappingTableName} WHERE \"guid\" = @0 AND \"table_name\" LIKE @1 UNION SELECT \"guid\" FROM {mappingTableName} WHERE \"t_guid\" = @0 AND \"t_table_name\" LIKE @1)", guid, tableName);
            }
            else
            {
                return postgreSql.Query<ItemEntity>($"SELECT * FROM {targetTableName} WHERE \"Guid\" IN (SELECT \"t_guid\" FROM {mappingTableName} WHERE \"guid\" = @0 AND \"table_name\" LIKE @1)", guid, tableName);
            }
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

        public IEnumerable<ItemEntity> QueryItemsByStringValue(string tableName, string pattern)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            pattern = pattern.Replace("_", "\\_"); // Disable PostgreSQL's: Single character wildcard (_)
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"ValueString\" ILIKE @0", pattern);
        }

        public IEnumerable<ItemEntity> QueryItemsByNumberValue(string tableName, double from, double to)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"ValueNumber\" >= @0 AND \"ValueNumber\" <= @1", from, to);
        }

        public IEnumerable<ItemEntity> QueryItemsByDateTimeValue(string tableName, DateTime from, DateTime to)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"ValueDate\" >= @0 AND \"ValueDate\" <= @1", from, to);
        }
    }
}
