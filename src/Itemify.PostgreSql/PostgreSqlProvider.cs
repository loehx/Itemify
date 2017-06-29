using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Itemify.Core.PostgreSql.Exceptions;
using Itemify.Core.PostgreSql.Util;
using Itemify.Logging;
using Itemify.Shared.Utils;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    public class PostgreSqlProvider : IDisposable
    {
        private readonly PostgreSqlConnectionPool connectionPool;
        private readonly ILogWriter log;
        private readonly string schema;

        public const string TABLE_PLACEHOLDER = "{table}";

        public string Schema => schema;

        public PostgreSqlProvider(PostgreSqlConnectionPool connectionPool, ILogWriter log, string schema = "public")
        {
            this.connectionPool = connectionPool;
            this.log = log.NewRegion(nameof(PostgreSqlDatabase));
            this.schema = schema;
        }

        public void EnsureSchemaExists()
        {
            Execute($"CREATE SCHEMA IF NOT EXISTS \"{Schema}\";");
        }

        public bool CreateTable<TSchema>(string tableName)
            where TSchema : IEntityBase
        {
            tableName = ResolveTableName(tableName);

            log.Describe($"{nameof(CreateTable)}<{typeof(TSchema).Name}>: {tableName}");

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            var type = typeof(TSchema);
            var columns = ReflectionUtil.GetColumnSchemas(type).ToArray();
            var sql = new StringBuilder(512);
            if (columns.Length == 0)
                throw new Exception("No columns defined by " + nameof(PostgreSqlColumnAttribute) + " in class: " + type.Name);

            sql.AppendFormat("CREATE TABLE ")
                .WriteLine(ResolveTableName(tableName))
                .WriteLine("(");

            foreach (var column in columns)
            {
                var columnName = '"' + column.Name + '"';
                var columnType = column.DataType;
                var columnNotNull = column.Nullable ? null : " NOT NULL";

                sql.WriteTabbedLine(1, $"{columnName} {columnType}{columnNotNull},");
            }

            var pk = columns.SingleOrDefault(k => k.PrimaryKey);
            if (pk != null)
            {
                sql.WriteTabbedLine(1, $"PRIMARY KEY(\"{pk.Name}\")");
            }
            else
            {
                sql.TrimEndLineBreaks()
                    .TrimEnd(',')
                    .NewLine();
            }

            sql.WriteLine(")");

            using (var db = getDb())
                return db.Execute(sql.ToString()) > 0;
        }

        public bool TableExists(string tableName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            var sql = new StringBuilder();

            tableName = ResolveTableName(tableName);

            var split = tableName.Split('.');
            if (split.Length != 2)
                throw new ArgumentException("A resolved table name must contain the schema and a dot.", nameof(tableName));

            var schema = split[0].Trim('"');
            var table = split[1].Trim('"');

            sql.AppendLine(@"SELECT EXISTS (SELECT 1");
            sql.AppendLine("    FROM   information_schema.tables");
            sql.AppendFormat("    WHERE  table_schema = '{0}'", schema).AppendLine();
            sql.AppendFormat("    AND    table_name = '{0}'", table).AppendLine();
            sql.AppendLine(");");

            using (var db = getDb())
                return true.Equals(db.QuerySingleValue(sql.ToString()));
        }

        public ICollection<string> GetTableNamesBySchema(string schema)
        {
            var result = new List<string>();
            var sql = new StringBuilder();

            sql.AppendLine(@"SELECT table_name");
            sql.AppendLine("    FROM   information_schema.tables");
            sql.AppendFormat("    WHERE  table_schema = '{0}'", schema).AppendLine();

            using (var db = getDb())
            using (var reader = db.Query(sql.ToString()))
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }

            return result;
        }

        public bool DropTable(string tableName)
        {
            tableName = ResolveTableName(tableName);

            var sql = "DROP TABLE " + ResolveTableName(tableName);
            using (var db = getDb())
                return db.Execute(sql) > 0;
        }

        public string ResolveTableName(string tableName)
        {
            if (tableName.StartsWith($"\"{Schema}\"."))
                return tableName;

            return $"\"{Schema}\".\"{tableName.ToCamelCase()}\"";
        }

        public void Insert(string tableName, IAnonymousEntity entity, bool merge = true)
        {
            Insert(tableName, entity, true, false, merge);
        }

        public Guid Insert(string tableName, IGloballyUniqueEntity entity, bool upsert = false, bool merge = true)
        {
            if (entity.Guid == Guid.Empty)
                throw new Exception("Cannot insert entity that with guid.");

            var guid = Insert(tableName, entity, true, upsert, merge);
            if (guid != null)
                entity.Guid = (Guid)guid;

            return entity.Guid;
        }

        public int Insert(string tableName, IDefaultEntity entity, bool upsert = false, bool merge = true)
        {
            var insertPrimaryKey = entity.Id != default(int);

            var id = Insert(tableName, entity, insertPrimaryKey, upsert, merge);
            if (id != null)
                entity.Id = (int)id;

            return entity.Id;
        }

        private object Insert(string tableName, IEntityBase entity, bool insertPrimaryKey, bool upsert, bool merge)
        {
            log.Describe($"{nameof(Insert)} into: {tableName}", new
            {
                insertPrimaryKey,
                upsert,
                merge,
                entity
            });

            var type = entity.GetType();
            var columns = ReflectionUtil.GetColumnSchemas(type);
            var columnNames = new List<string>(columns.Count);
            var valuePlaceholders = new List<string>(columns.Count);
            var values = new List<object>(columns.Count);
            var query = new StringBuilder(columns.Count * 32);
            var pos = 0;
            PostgreSqlColumnSchema pk = null;

            foreach (var column in columns)
            {
                // don't insert PK if specified
                if (column.PrimaryKey)
                {
                    pk = column;

                    if (!insertPrimaryKey)
                        continue;
                }

                var value = column.GetValue(entity);
                var defaultValue = value?.GetType().GetDefault();

                if (column.Nullable && (merge && value == defaultValue))
                    continue;

                columnNames.Add('"' + column.Name + '"');
                valuePlaceholders.Add("@" + pos++);
                values.Add(value);
            }

            query.Write($"INSERT INTO ")
                .WriteLine(ResolveTableName(tableName))
                .WriteTabbedLine(1, $"({string.Join(", ", columnNames)})")
                .WriteTabbedLine(1, $"VALUES ({string.Join(", ", valuePlaceholders)})");

            if (upsert && pk != null)
            {
                query.WriteTabbedLine(2, "ON CONFLICT (\"" + pk.Name + "\") DO UPDATE SET ");
                for (var i = 0; i < valuePlaceholders.Count; i++)
                {
                    query.WriteTabbed(3, columnNames[i])
                        .Write(" = ")
                        .Write(valuePlaceholders[i])
                        .WriteIf(i < valuePlaceholders.Count - 1, ",")
                        .NewLine();
                }
            }

            if (!insertPrimaryKey && pk != null)
            {
                query.WriteTabbedLine(2, "RETURNING \"" + pk.Name + '"');
            }

            using (var db = getDb())
                return db.QuerySingleValue(query.ToString(), values);
        }

        public int Update(string tableName, IEntityBase entity, bool merge)
        {
            log.Describe($"{nameof(Update)}: {tableName}", new
            {
                merge,
                entity
            });

            var type = entity.GetType();
            var columns = ReflectionUtil.GetColumnSchemas(type);
            var query = new StringBuilder(columns.Count * 32);
            var values = new List<object>(columns.Count);
            var pos = 0;
            PostgreSqlColumnSchema pk = null;

            query.Write($"UPDATE ")
                .WriteLine(ResolveTableName(tableName))
                .WriteTabbedLine(1, "SET");

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var value = columns[i].GetValue(entity);
                var defaultValue = value?.GetType().GetDefault();

                if (column.PrimaryKey)
                {
                    pk = column;
                    continue;
                }

                if (column.Nullable && (merge && value == defaultValue))
                    continue;

                values.Add(value);

                query.WriteTabbed(2, '"' + columns[i].Name + '"')
                    .Write(" = @" + pos++)
                    .Write(",")
                    .NewLine();
            }

            query.TrimEndLineBreaks()
                .TrimEnd(',');
            
            if (pk != null)
            {
                query.WriteTabbedLine(2, "WHERE \"" + pk.Name + "\" = @" + pos++);
                values.Add(pk.GetValue(entity));
            }
            
            query.TrimEndLineBreaks()
                .NewLine();

            using (var db = getDb())
                return db.Execute(query.ToString(), values);
        }


        public void BulkInsert(string tableName, IEnumerable<IAnonymousEntity> entities)
        {
            bulkInsert(tableName, entities.Array(), false);
        }

        public IEnumerable<Guid> BulkInsert(string tableName, IEnumerable<IGloballyUniqueEntity> entities)
        {
            var ids = bulkInsert(tableName, entities.Array(), true);
            return ids.Cast<Guid>();
        }

        public IEnumerable<int> BulkInsert(string tableName, IEnumerable<IDefaultEntity> entities)
        {
            var ids = bulkInsert(tableName, entities.Array(), false);
            return ids.Cast<int>();
        }

        private IReadOnlyList<object> bulkInsert(string tableName, IReadOnlyList<IEntityBase> entities, bool insertPrimaryKey)
        {
            if (entities.Count == 0)
                throw new ArgumentException("No entities to insert.");

            log.Describe($"{nameof(bulkInsert)} into: {tableName}", new
            {
                insertPrimaryKey,
                entities.Count
            });

            tableName = ResolveTableName(tableName);
            var type = entities[0].GetType();
            var columns = ReflectionUtil.GetColumnSchemas(type);
            var columnNames = new List<string>(columns.Count);
            var values = new List<object>(columns.Count * entities.Count);
            var query = new StringBuilder(columns.Count * 32);
            var pos = 0;
            PostgreSqlColumnSchema pk = null;

            foreach (var column in columns)
            {
                // don't insert PK if specified
                if (column.PrimaryKey)
                {
                    pk = column;

                    if (!insertPrimaryKey)
                        continue;
                }

                columnNames.Add('"' + column.Name + '"');
            }

            query.Write($"INSERT INTO ")
                .WriteLine(tableName)
                .WriteTabbedLine(1, $"({string.Join(", ", columnNames)})")
                .WriteTabbedLine(1, "VALUES");


            foreach (var entity in entities)
            {
                if (!type.GetTypeInfo().IsInstanceOfType(entity))
                    throw new Exception($"Mixed up entities in bulk insert. ({type.Name} != {entity.GetType().Name})");

                query.WriteTabbed(2, "(");

                foreach (var column in columns)
                {
                    // don't insert PK if specified
                    if (column.PrimaryKey && !insertPrimaryKey)
                        continue;

                    var value = column.GetValue(entity);
                    values.Add(value);

                    query.Append('@').Append(pos++).Append(',');
                }

                query.TrimEnd(',').Append("),").NewLine();
            }

            query.TrimEndLineBreaks()
                .TrimEnd(',')
                .NewLine();

            using (var db = getDb())
            {
                if (pk == null)
                {

                    var affected = db.Execute(query.ToString(), values);

                    return new object[0];
                }
                else
                {
                    var insertedIds = new List<object>();
                    query.WriteTabbedLine(1, $"RETURNING \"{pk.Name}\"");

                    using (var reader = db.Query(query.ToString(), values))
                    {
                        Debug.Assert(reader.VisibleFieldCount == 1);

                        while (reader.Read())
                        {
                            var id = reader.GetValue(0);
                            insertedIds.Add(id);

                            Debug.Assert(id != null);
                        }
                    }

                    return insertedIds;
                }
            }
        }

        public int Execute(string query, params object[] parameters)
        {
            using (var db = getDb())
                return db.Execute(query, parameters);
        }

        public IEnumerable<TEntity> Query<TEntity>(string query, params object[] parameters)
            where TEntity : IEntityBase, new()
        {
            return query<TEntity>(query, parameters);
        }

        private IEnumerable<TEntity> query<TEntity>(string query, params object[] parameters)
            where TEntity : IEntityBase, new()
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            Debug.Assert(parameters.Length == 0 || query.Contains("@" + (parameters.Length - 1)), "Query should contain exactly " + parameters.Length + " placeholders like @0, @1, ...");

            var type = typeof(TEntity);

            using (var db = getDb())
            using (var reader = db.Query(query, parameters))
            {
                var allColumns = ReflectionUtil.GetColumnSchemas(type);
                var columns = GetColumns(reader)
                    .Select(name => GetColumnSchema<TEntity>(name, allColumns))
                    .ToArray();
                var valueSets = GetValueSets(reader);

                foreach (var valueSet in valueSets)
                {
                    var result = new TEntity();

                    for (int i = 0; i < columns.Length; i++)
                    {
                        var column = allColumns[i];
                        var value = valueSet[i];

                        column.SetValue(result, value);
                    }

                    yield return result;
                }
            }
        }

        private PostgreSqlDatabase getDb()
        {
            return new PostgreSqlDatabase(connectionPool, log);
        }

        private static PostgreSqlColumnSchema GetColumnSchema<T>(string name, IReadOnlyList<PostgreSqlColumnSchema> allColumns)
        {
            var schema = allColumns.FirstOrDefault(c => c.Name.Equals(name));
            if (schema == null)
                throw new MissingPropertyException(typeof(T), name);

            return schema;
        }

        public IEnumerable<object[]> Query(string query, params object[] parameters)
        {
            return this.query(query, parameters);
        }

        private IEnumerable<object[]> query(string query, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            Debug.Assert(query.Contains("@" + (parameters.Length - 1)), "Query should contain exactly " + parameters.Length + " placeholders like @0, @1, ...");

            using (var db = getDb())
            using (var reader = db.Query(query, parameters))
            {
                foreach (var set in GetValueSets(reader))
                    yield return set;
            }
        }

        private static string[] GetColumns(NpgsqlDataReader reader)
        {
            Debug.Assert(reader.VisibleFieldCount > 0);
            var columns = new string[reader.VisibleFieldCount];

            for (var i = 0; i < reader.VisibleFieldCount; i++)
                columns[i] = reader.GetName(i);


            return columns;
        }

        private static IEnumerable<object[]> GetValueSets(NpgsqlDataReader reader)
        {
            while (reader.Read())
            {
                var values = new object[reader.VisibleFieldCount];
                var count = reader.GetValues(values);

                Debug.Assert(values.Length == reader.VisibleFieldCount);
                Debug.Assert(values.Length == count);

                yield return values;
            }
        }

        public void Dispose()
        {
            log?.Dispose();
        }

        public void TryDropTable(string table)
        {
            if (TableExists(table))
                DropTable(table);
        }
    }
}
