using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itemify.Core.Item;

namespace Itemify
{
    public static class ItemExtensions
    {
        public static T Wrap<T>(this IItem item)
        {
            return (T)Activator.CreateInstance(typeof(T), item);
        }

    }
}
