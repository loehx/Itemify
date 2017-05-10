using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itemify
{
    public static class ItemExtensions
    {
        public static T Wrap<T>(this Item item)
        {
            if (item == null)
                return default(T);

            try
            {
                return (T) Activator.CreateInstance(typeof(T), item);
            }
            catch (Exception err)
            {
                throw new Exception($"Could not wrap item with class: \"{typeof(T)}\"", err);
            }
        }
    }
}
