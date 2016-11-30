using System;

namespace Itemify.Core.Item
{
    public class GodItem : ItemBase
    {
        public override Guid Id
        {
            get { return Guid.Empty; }
            set { throw new Exception("God has ID zero for eternity. You should now that ..."); }
        }

        public override string Name
        {
            get { return "GOD"; }
            set { throw new Exception("Are you serious? Nobady can do that ..."); }
        }
    }
}
