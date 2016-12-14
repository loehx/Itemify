using System;
using System.Diagnostics;
using System.Globalization;
using Itemify.Core.PostgreSql.Util;

namespace Itemify.Core.PostgreSql.Logging
{
    public class ConsoleSqlLog : ISqlLog
    {
        public void New(string title)
        {
            Console.WriteLine(" ");
            Console.WriteLine("===== SQL LOG: " + title + " " + new string('=', 80 - title.Length));
            Console.WriteLine(" ");
        }

        public void WriteSql(string sql, object[] parameters)
        {
            Console.WriteLine(sql.Trim(' ', '\n'));
            Console.WriteLine(" ");

            if (parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    Console.WriteLine($"    [{i}] => {SqlUtil.ToSql(p)}");
                }
                Console.WriteLine(" ");
            }
        }

        public void Failed(Exception err)
        {
            Console.WriteLine(err.ToString());
        }

        public void Completed(long elapsedMilliseconds)
        {
            Console.WriteLine("--> took " + elapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture) + " ms.");
            Console.WriteLine(" ");
        }
    }
}