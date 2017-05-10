using System;
using System.Collections.Generic;
using System.Linq;

namespace Itemify.Core
{
    public class ItemResolving
    {
        private List<string> children;
        private List<string> relations;

        internal ItemResolving()
        {
        }

        public bool Empty => children == null && relations == null;
        public IEnumerable<string> ChildrenTypes => children ?? Enumerable.Empty<string>();
        public IEnumerable<string> RelationsTypes => relations ?? Enumerable.Empty<string>();


        public ItemResolving ChildrenOfType(params string[] types)
        {
            if (children == null)
                children = new List<string>(types.Length);

            children.AddRange(types);

            return this;
        }

        public ItemResolving RelatedItemsOfType(params string[] types)
        {
            if (relations == null)
                relations = new List<string>(types.Length);

            relations.AddRange(types);

            return this;
        }


        public static ItemResolving Default => new ItemResolving();
    }
}