using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Itemify.Shared.Logging;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    internal class PostgreSqlDatabase : IDisposable
    {
        private readonly ILogWriter log;
        private NpgsqlConnection connection;


        public PostgreSqlDatabase(PostgreSqlConnectionContext context, ILogWriter log)
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

            log.Describe(nameof(Execute));
            log.Describe(sql, parameters);

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
                log.Describe(err);
                throw err;
            }
            finally
            {
                log.Describe("Execute complete");
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

            log.Describe(nameof(Query));
            log.Describe(sql, parameters);

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
                log.Describe(err);
                throw err;
            }
            finally
            {
                log.Describe("Query complete");
            }
        }

        private static void appendParameters(object[] parameters, NpgsqlCommand cmd)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                cmd.Parameters.AddWithValue("@" + i, p ?? DBNull.Value);
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

            log.Describe(nameof(QuerySingleValue));
            log.Describe(sql, parameters);

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

                var result = cmd.ExecuteScalar();

                log.Describe("Query single value successful.", result);

                return result;
            }
            catch (Exception err)
            {
                log.Describe(err);
                throw err;
            }
            finally
            {
                log.Describe("QuerySingleValue complete");
            }
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
