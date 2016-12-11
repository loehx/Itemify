using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itemify.Core.Src.PostgreSql
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
