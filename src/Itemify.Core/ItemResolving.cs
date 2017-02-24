using System;
using System.Collections.Generic;
using System.Linq;

namespace Itemify.Core
{
    public class ItemResolving
    {
        private List<Enum> children;
        private List<Enum> relations;

        internal ItemResolving()
        {
        }

        public bool Empty => children == null && relations == null;
        public IEnumerable<Enum> ChildrenTypes => children ?? Enumerable.Empty<Enum>();
        public IEnumerable<Enum> RelationsTypes => relations ?? Enumerable.Empty<Enum>();


        public ItemResolving ChildrenOfType(params Enum[] types)
        {
            if (children == null)
                children = new List<Enum>(types.Length);

            children.AddRange(types);

            return this;
        }

        public ItemResolving RelatedItemsOfType(params Enum[] types)
        {
            if (relations == null)
                relations = new List<Enum>(types.Length);

            relations.AddRange(types);

            return this;
        }


        public static ItemResolving Default => new ItemResolving();
    }
}