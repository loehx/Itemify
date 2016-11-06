using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Itemify.Core.PostgreSql.Logging;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    internal class PostgreSqlDatabase : IDisposable
    {
        private readonly ISqlLog log;
        private NpgsqlConnection connection;


        public PostgreSqlDatabase(PostgreSqlConnectionContext context, ISqlLog log)
        {
            this.log = log;
            connection = context.Connection;
        }

        public int Execute(string sql)
        {
            return Execute(sql, new object[0]);
        }

        public int Execute(string sql, IEnumerable<object> values)
        {
            var parameters = values as object[] ?? values.ToArray();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.New(nameof(Execute));
            log.WriteSql(sql, parameters);

            try
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = sql;

                    if (parameters.Length > 0)
                        appendParameters(parameters, cmd);

                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception err)
            {
                log.Failed(err);
                throw err;
            }
            finally
            {
                log.Completed(stopwatch.ElapsedMilliseconds);
            }
        }

        public NpgsqlDataReader Query(string sql)
        {
            return Query(sql, new object[0]);
        }

        public NpgsqlDataReader Query(string sql, IEnumerable<object> values)
        {
            var parameters = values as object[] ?? values.ToArray();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.New(nameof(Query));
            log.WriteSql(sql, parameters);

            try
            {
                // TODO: Wrap this in using(...)
                var cmd = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };

                if (parameters.Length > 0)
                    appendParameters(parameters, cmd);

                return cmd.ExecuteReader();
            }
            catch (Exception err)
            {
                log.Failed(err);
                throw err;
            }
            finally
            {
                log.Completed(stopwatch.ElapsedMilliseconds);
            }
        }

        private static void appendParameters(object[] parameters, NpgsqlCommand cmd)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                cmd.Parameters.AddWithValue("@" + i, p);
            }

            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue(PrepareValue(parameter));
            }
        }

        public object QuerySingleValue(string sql)
        {
            return QuerySingleValue(sql, new object[0]);
        }

        public object QuerySingleValue(string sql, IEnumerable<object> values)
        {
            var parameters = values as object[] ?? values.ToArray();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            log.New(nameof(QuerySingleValue));
            log.WriteSql(sql, parameters);

            try
            {
                // TODO: Wrap this in using(...)
                var cmd = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = sql
                };

                if (parameters.Length > 0)
                    appendParameters(parameters, cmd);

                return cmd.ExecuteScalar();
            }
            catch (Exception err)
            {
                log.Failed(err);
                throw err;
            }
            finally
            {
                log.Completed(stopwatch.ElapsedMilliseconds);
            }
        }

        public static object PrepareValue(object value)
        {
            return value;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }
    }
}
