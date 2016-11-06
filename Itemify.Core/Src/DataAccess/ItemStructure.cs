using System.Diagnostics;
using System.Text;
using Itemify.Core.Utils;

namespace Itemify.Core.DataAccess
{
    public class ItemStructure
    {
        public ItemStructure()
        {
        }

        public void CreateItemTable(string name)
        {
            var query = AssemblyUtil.GetResourceFileContent("create_item_table.sql", Encoding.UTF8);

            Debug.WriteLine(query);
        }
    }
}
