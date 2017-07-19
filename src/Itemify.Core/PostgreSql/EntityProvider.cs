using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Itemify.Core.Exceptions;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Core.PostgreSql.Util;
using Itemify.Logging;
using Itemify.Shared.Utils;

namespace Itemify.Core.PostgreSql
{
    // TODO: Make internal
    public class EntityProvider
    {
        private PostgreSqlProvider postgreSql;
        private readonly ILogWriter log;
        private readonly Hashtable tables;
        private object syncRoot;

        public EntityProvider(PostgreSqlProvider postgreSql, ILogWriter log)
        {
            this.postgreSql = postgreSql;
            this.log = log;
            this.tables = new Hashtable();
            this.syncRoot = new object();
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

        public void InsertEntityRelations(string tableName, Guid guid,
            IEnumerable<KeyValuePair<Guid, string>> targetItems, string mappingTableName, bool overwrite)
        {
            tableName = resolveTable<ItemEntity>(tableName, guid == Guid.Empty);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);

            if (overwrite)
                postgreSql.Execute("DELETE FROM " + mappingTableName + " WHERE \"guid\" = @0 AND \"table_name\" LIKE @1", guid, tableName);

            var relations = targetItems.Select(k => new ItemRelationEntity()
            {
                Guid = guid,
                Table = tableName,
                TargetGuid = k.Key,
                TargetTable = resolveTable<ItemEntity>(k.Value, k.Key == Guid.Empty)
            });

            postgreSql.BulkInsert(mappingTableName, relations);
        }

        public int RemoveEntityRelations(string tableName, Guid guid, string mappingTableName, IEnumerable<string> targetTables)
        {
            tableName = resolveTable<ItemEntity>(tableName, guid == Guid.Empty);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);
            var parameters = new object[] { guid, tableName }
                .Concat(targetTables.Select(k => resolveTable<ItemEntity>(k)))
                .ToArray();

            return postgreSql.Execute("DELETE FROM " + mappingTableName +
                               $" WHERE \"guid\" = @0 AND \"table_name\" LIKE @1 AND \"t_table_name\" IN ({ parameters.Length.Times().Skip(2).Select(i => "@" + i).Join(", ") })", parameters);
        }

        public int RemoveEntityRelations(string tableName, Guid guid, string mappingTableName)
        {
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);
            tableName = resolveTable<ItemEntity>(tableName);

            var query = $"DELETE FROM {mappingTableName} WHERE (\"guid\" = @0 AND \"table_name\" LIKE @1) OR (\"t_guid\" = @0 AND \"t_table_name\" LIKE @1);";
            return postgreSql.Execute(query, guid, tableName);
        }

        public void DeleteEntity(string tableName, Guid guid)
        {
            tableName = resolveTable<ItemEntity>(tableName);

            var query = new StringBuilder();
            var parameter = new List<object>();

            query.AppendLine($"DELETE FROM {tableName} WHERE \"Guid\" = @0;");
            parameter.Add(guid);

            var affected = postgreSql.Execute(query.ToString(), guid);
            log.Describe($"Deleted {affected} item(s) successfully.");
        }

        public IEnumerable<ItemEntity> QueryEntityRelations(string tableName, Guid guid, string targetTableName, string mappingTableName, bool bidirectional)
        {
            tableName = resolveTable<ItemEntity>(tableName, guid == Guid.Empty);
            targetTableName = resolveTable<ItemEntity>(targetTableName);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);

            if (bidirectional)
            {
                // TODO: Put an INDEX on both guid+table_name and t_guid+t_table_name
                return postgreSql.Query<ItemEntity>($"SELECT * FROM {targetTableName} WHERE \"Guid\" IN (SELECT \"t_guid\" FROM {mappingTableName} WHERE (\"guid\" = @0 AND \"table_name\" LIKE @1) UNION SELECT \"guid\" FROM {mappingTableName} WHERE (\"t_guid\" = @0 AND \"t_table_name\" LIKE @1))", guid, tableName);
            }
            else
            {
                return postgreSql.Query<ItemEntity>($"SELECT * FROM {targetTableName} WHERE \"Guid\" IN (SELECT \"t_guid\" FROM {mappingTableName} WHERE \"guid\" = @0 AND \"table_name\" LIKE @1)", guid, tableName);
            }
        }

        public IEnumerable<ItemEntity> QueryEntityRelations(string tableName, Guid guid,
            string mappingTableName, bool bidirectional)
        {
            tableName = resolveTable<ItemEntity>(tableName, guid == Guid.Empty);
            mappingTableName = resolveTable<ItemRelationEntity>(mappingTableName);
            string[] targetTableNames = null;

            if (bidirectional)
            {
                targetTableNames = postgreSql.Query($"SELECT \"t_table_name\" FROM {mappingTableName} WHERE \"guid\" = @0 AND \"table_name\" LIKE @1" +
                                                    $" UNION SELECT \"table_name\" FROM {mappingTableName} WHERE \"t_guid\" = @0 AND \"t_table_name\" LIKE @1", guid, tableName)
                    .ToArray(k => (string)k[0]);
            }
            else
            {
                targetTableNames = postgreSql.Query($"SELECT \"t_table_name\" FROM {mappingTableName} WHERE \"guid\" = @0 AND \"table_name\" LIKE @1", guid, tableName)
                    .ToArray(k => (string)k[0]);
            }

            // TODO: Optimize performance
            return targetTableNames.SelectMany(t => QueryEntityRelations(tableName, guid, t,
                mappingTableName, bidirectional));
        }


        public ItemEntity QuerySingleEntity(string tableName, Guid guid)
        {
            tableName = resolveTable<ItemEntity>(tableName);
            return postgreSql.Query<ItemEntity>("SELECT * FROM " + tableName + " WHERE \"Guid\" = @0", guid).FirstOrDefault();
        }



        private string resolveTable<TSchema>(string tableName, bool ignoreTableExists = false)
           where TSchema : IEntityBase
        {
            tableName = postgreSql.ResolveTableName(tableName);

            if (ignoreTableExists) return tableName;

            lock (syncRoot)
            {
                var table = tables[tableName];
                if (table != null) return tableName; // Table already resolved
                tables[tableName] = true;

                if (!postgreSql.TableExists(tableName))
                {
                    Debug.Assert(!tableName.Contains("\"root\""));
                    log.Describe($"Create missing table: {tableName}");
                    postgreSql.CreateTable<TSchema>(tableName);
                }

            }

            return tableName;
        }

        public IEnumerable<ItemEntity> QueryEntitiesByName(string tableName, string pattern)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            pattern = pattern.Replace("_", "\\_"); // Disable PostgreSQL's: Single character wildcard (_)
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"Name\" ILIKE @0", pattern);
        }

        public IEnumerable<ItemEntity> QueryEntitiesByStringValue(string tableName, string pattern)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            pattern = pattern.Replace("_", "\\_"); // Disable PostgreSQL's: Single character wildcard (_)
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"ValueString\" ILIKE @0", pattern);
        }

        public IEnumerable<ItemEntity> QueryEntitiesByNumberValue(string tableName, double from, double to)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"ValueNumber\" >= @0 AND \"ValueNumber\" <= @1", from, to);
        }

        public IEnumerable<ItemEntity> QueryEntitiesByDateTimeValue(string tableName, DateTime from, DateTime to)
        {
            tableName = postgreSql.ResolveTableName(tableName);
            return postgreSql.Query<ItemEntity>($"SELECT * FROM {tableName} WHERE \"ValueDate\" >= @0 AND \"ValueDate\" <= @1", from, to);
        }

        public void Reset()
        {
            postgreSql.DropSchema();
            postgreSql.EnsureSchemaExists();
        }

        public IEnumerable<ItemEntity> QueryEntitiesByTableNames(IEnumerable<string> tableNames)
        {
            var sql = new StringBuilder();

            foreach (var tableName in tableNames)
            {
                if (tableName.IsEmpty()) throw new ArgumentException("List of table names contains an empty string.", nameof(tableNames));

                sql.AppendLine($"SELECT * FROM {postgreSql.ResolveTableName(tableName)}")
                    .AppendLine("UNION");
            }

            sql.Length -= "UNION".Length + 2;

            return postgreSql.Query<ItemEntity>(sql.ToString());
        }

        public int ReplaceEntityRelationSources(string mappingTable, Guid guid, string tableName, Guid newGuid, string newTableName, bool newItemIsVirtual)
        {
            mappingTable = resolveTable<ItemRelationEntity>(mappingTable);
            tableName = resolveTable<ItemEntity>(tableName);
            newTableName = resolveTable<ItemEntity>(newTableName, newItemIsVirtual);

            var query = $"UPDATE {mappingTable} SET \"guid\" = @0, \"table_name\" = @1 WHERE \"guid\" = @2 AND \"table_name\" LIKE @3";
            return postgreSql.Execute(query, newGuid, newTableName, guid, tableName);
        }
    }
}
