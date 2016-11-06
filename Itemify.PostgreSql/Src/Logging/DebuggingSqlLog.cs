using System;
using System.Diagnostics;
using System.Globalization;
using Itemify.Core.PostgreSql.Util;

namespace Itemify.Core.PostgreSql.Logging
{
    public class DebuggingSqlLog : ISqlLog
    {
        public void New(string title)
        {
            Debug.WriteLine(" ");
            Debug.WriteLine("===== SQL LOG: " + title + " " + new string('=', 80 - title.Length));
            Debug.WriteLine(" ");
        }

        public void WriteSql(string sql, object[] parameters)
        {
            Debug.WriteLine(sql.Trim(' ', '\n'));
            Debug.WriteLine(" ");

            if (parameters.Length > 0)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    Debug.WriteLine($"    [{i}] => {SqlUtil.ToSql(p)}");
                }
                Debug.WriteLine(" ");
            }
        }

        public void Failed(Exception err)
        {
            Debug.WriteLine(err.ToString());
        }

        public void Completed(long elapsedMilliseconds)
        {
            Debug.WriteLine("--> took " + elapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture) + " ms.");
            Debug.WriteLine(" ");
        }
    }
}