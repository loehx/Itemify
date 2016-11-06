using System;

namespace Itemify.Core.PostgreSql.Logging
{
    public interface ISqlLog
    {
        void New(string title);
        void WriteSql(string sql, object[] parameters);
        void Failed(Exception err);
        void Completed(long elapsedMilliseconds);
    }
}