using System;

namespace Itemify.Core.PostgreSql
{
    class EntityProviderLog
    {
        private Action<string> log;

        public EntityProviderLog(Action<string> log)
        {
            this.log = log;
        }

        public void CreateTable(string tableName)
        {
            log("Create table: '" + tableName + "'");
        }
    }
}
