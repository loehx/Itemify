using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Itemify.Core.PostgreSql.Exceptions;
using Itemify.Core.PostgreSql.Logging;
using Itemify.Core.PostgreSql.Util;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    public interface IGloballyUniqueEntity : IEntityBase
    {
        Guid Guid { get; set; }
    }

    public interface IDefaultEntity : IEntityBase
    {
        int Id { get; set; }
    }

    public interface IAnonymousEntity : IEntityBase
    {
        // no id
    }

    public interface IEntityBase
    {
    }

    public class PostgreSqlProvider : IDisposable
    {
        private PostgreSqlConnectionContext context;
        private readonly PostgreSqlDatabase db;
        private readonly string schema;

        public const string TABLE_PLACEHOLDER = "{table}";

        public string Schema => schema;
        public int ConnectionId => context.ConnectionId;

        public PostgreSqlProvider(PostgreSqlConnectionPool connectionPool, ISqlLog log, string schema = "public")
        {
            this.schema = schema;
            context = connectionPool.GetContext();
            db = new PostgreSqlDatabase(context, log);
        }

        public void EnsureSchemaExists()
        {
            db.Execute($"CREATE SCHEMA IF NOT EXISTS \"{Schema}\";");
        }

        public bool CreateTable<TSchema>(string tableName)
            where TSchema : IEntityBase
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            var type = typeof(TSchema);
            var columns = ReflectionUtil.GetColumnSchemas(type).ToArray();
            var sql = new StringBuilder(512);
            if (columns.Length == 0)
                throw new Exception("No columns defined by " + nameof(PostgreSqlColumnAttribute) + " in class: " + type.Name);

            sql.AppendFormat("CREATE TABLE")
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

            return db.Execute(sql.ToString()) > 0;
        }

        public bool TableExists(string tableName)
        {
            var sql = @"SELECT EXISTS (SELECT 1
                   FROM   information_schema.tables 
                   WHERE  table_schema = '{0}'
                   AND    table_name = '{1}'
                );";

            sql = string.Format(sql, Schema, tableName);

            var result = db.QuerySingleValue(sql);
            return true.Equals(result);
        }

        public ICollection<string> GetTableNamesBySchema(string schema)
        {
            var result = new List<string>();
            var sql = @"SELECT table_name
                   FROM   information_schema.tables 
                   WHERE  table_schema = '{0}'";

            sql = string.Format(sql, schema);

            using (var reader = db.Query(sql))
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
            var sql = "DROP TABLE " + ResolveTableName(tableName);
            var affected = db.Execute(sql);
            return affected > 0;
        }

        public string ResolveTableName(string tableName)
        {
            if (tableName.StartsWith($"\"{Schema}\"."))
                return tableName;

            return $"\"{Schema}\".\"{tableName}\"";
        }

        public void Insert(string tableName, IAnonymousEntity entity)
        {
            Insert(tableName, entity, true, false);
        }

        public Guid Insert(string tableName, IGloballyUniqueEntity entity, bool upsert = false)
        {
            if (entity.Guid == Guid.Empty)
                entity.Guid = Guid.NewGuid();

            var guid = Insert(tableName, entity, true, upsert);
            if (guid != null)
                entity.Guid = (Guid)guid;

            return entity.Guid;
        }

        public int Insert(string tableName, IDefaultEntity entity, bool upsert = false)
        {
            var insertPrimaryKey = entity.Id != default(int);

            var id = Insert(tableName, entity, insertPrimaryKey, upsert);
            if (id != null)
                entity.Id = (int)id;

            return entity.Id;
        }

        private object Insert(string tableName, IEntityBase entity, bool insertPrimaryKey, bool upsert)
        {
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

                if (column.Nullable && value == null)
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

            var result = db.QuerySingleValue(query.ToString(), values);
            return result;
        }

        //public void Insert<TEntity>(string tableName, IEnumerable<TEntity> entities)
        //    where TEntity: IEntityBase
        //{
        //    const int bulkSize = 100; // TODO: Find out best bulk size for insertion
        //    var type = typeof(TEntity);
        //    var properties = type.GetProperties()
        //        .Where(k => k.GetCustomAttribute<PostgreSqlColumnAttribute>() != null)
        //        .ToArray();
        //    var columns = properties.Select(k => k.Name).ToArray();
        //    var query = new StringBuilder(1024);
        //    var valuePattern = "(" + string.Join(", ", 0.EnumerateTo(columns.Length - 1).Select((i) => "@{" + i + "}")) + ")\n";
        //    var allValues = new List<object>(columns.Length);

        //    foreach (var chunk in entities.Chunked(bulkSize))
        //    {
        //        query.Append($"INSERT INTO \"{Schema}\".\"{tableName}\" ([{string.Join("], [", columns)}]) VALUES\n");

        //        foreach (var entity in entities)
        //        {
        //            var values = properties.Select(k => k.GetValue(entity));
        //            query.AppendFormat(valuePattern, 0.EnumerateTo(columns.Length - 1).Select(i => i + allValues.Count));

        //            allValues.AddRange(values);
        //        }

        //        db.Execute(query.ToString(), allValues);
        //        query.Clear();
        //        allValues.Clear();
        //    }
        //}

        public void Execute(string query, params object[] parameters)
        {
            db.Execute(query, parameters);
        }

        public IEnumerable<TEntity> Query<TEntity>(string query, params object[] parameters)
            where TEntity : IEntityBase, new()
        {
            return query<TEntity>(query, parameters);
        }

        public IEnumerable<TEntity> Query<TEntity>(string query, Expression<Func<TEntity, bool>> predicate)
            where TEntity : IEntityBase, new()
        {
            return Enumerable.Empty<TEntity>();
        }

        private IEnumerable<TEntity> query<TEntity>(string query, params object[] parameters)
            where TEntity : IEntityBase, new()
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            Debug.Assert(query.Contains("@" + (parameters.Length - 1)), "Query should contain exactly " + parameters.Length + " placeholders like @0, @1, ...");

            var type = typeof(TEntity);

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

            StringCollection a;

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
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }
    }
}
