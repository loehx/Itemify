using System.Collections.Generic;
using System.Threading.Tasks;

namespace Itemify.Shared.Utils
{
    public static class AsyncUtil
    {

        public static void Wait(this IEnumerable<Task> tasks)
        {
            Task.WaitAll(tasks.Array());
        }

        public static void Wait(this IEnumerable<Task> tasks, int millisecondsTimeout)
        {
            Task.WaitAll(tasks.Array(), millisecondsTimeout);
        }

        public static async Task WhenAll(this IEnumerable<Task> tasks)
        {
            await Task.WhenAll(tasks);
        }

        public static async Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }
    }
}
