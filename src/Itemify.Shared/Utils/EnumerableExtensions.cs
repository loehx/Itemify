using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Itemify.Shared.Utils
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> func)
        {
            foreach (var item in source)
            {
                func(item);
            }
        }

        /// <summary>
        /// new int[] { 1, 2, 3, 4 }.ForEach(func) 
        /// Equals:
        ///  func(1, 2)
        ///  func(2, 3)
        ///  func(3, 4)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="func"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, T> func)
        {
            ForEach(source, 2, a => func(a[0], a[1]));
        }

        /// <summary>
        /// new int[] { 1, 2, 3, 4 }.ForEach(func) 
        /// Equals:
        ///  func(1, 2, 3)
        ///  func(2, 3, 4)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="func"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, T, T> func)
        {
            ForEach(source, 3, a => func(a[0], a[1], a[2]));
        } 

        public static void ForEach<T>(this IEnumerable<T> source, int chunkSize, Action<T[]> func)
        {
           var items = new T[chunkSize];
            var i = 0;
            var e = source.GetEnumerator();

            while (i < chunkSize && e.MoveNext())
            {
                items[i++] = e.Current;
            }

            func(items);

            while (e.MoveNext())
            {
                // Shift them by one position
                for (var j = 1; j < chunkSize; j++)
                {
                    items[j - 1] = items[j];
                }

                // Add current item to end of array
                items[chunkSize - 1] = e.Current;

                func(items);
            }
        } 

        public static IEnumerable<T> ByTheWay<T, T2>(this IEnumerable<T> source, Action<T, T2> action, T2 obj)
        {
            foreach (var value in source)
            {
                action(value, obj);
                yield return value;
            }
        }

        public static IEnumerable<T> ByTheWay<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var value in source)
            {
                action(value);
                yield return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T2> SelectList<T, T2>(this IEnumerable<T> source, Func<T, T2> factory)
        {
            return ToList(source, factory);
        }

        public static List<T> List<T>(this IEnumerable<T> source)
        {
            return source as List<T> ?? source.ToList();
        }

        public static T[] Array<T>(this IEnumerable<T> source)
        {
            return source as T[] ?? source.ToArray();
        }

        public static TResult[] ToArray<T, TResult>(this IEnumerable<T> source, Func<T, TResult> func)
        {
            var array = source as IReadOnlyList<T>;
            if (array != null)
            {
                var result = new TResult[array.Count];

                for (var i = 0; i < array.Count; i++)
                    result[i] = func(array[i]);
                
                return result;
            }

            return source.Select(func).ToArray();
        }

        public static List<TResult> ToList<T, TResult>(this IEnumerable<T> source, Func<T, TResult> factory)
        {
            var collection = source as IReadOnlyList<T>;
            if (collection != null)
            {
                var result = new List<TResult>(collection.Count);

                foreach (var item in collection)
                    result.Add(factory(item));
                
                return result;
            }

            return source.Select(factory).ToList();
        }

        public static IEnumerable<T> Times<T>(this int count, Func<int, T> factory)
        {
            return count.Times().Select(factory);
        }

        public static Task<T[]> TimesAsync<T>(this int count, Func<int, T> factory)
        {
            var tasks = count.Times().Select(async i =>
            {
                
                await Task.Yield();
                return factory(i);
            }).Array();

            return Task.WhenAll(tasks);
        }

        public static Task<T[]> TimesAsync<T>(this int count, Func<int, Task<T>> factory)
        {
            var tasks = count.Times().Select(async i =>
            {
                await Task.Yield();
                return await factory(i);
            }).Array();

            return Task.WhenAll(tasks);
        }

        public static IEnumerable<int> Times(this int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable<T> source)
        {
            return source;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, IEnumerable<T> target, Func<T, T, bool> pred, Func<T, int> hash)
        {
            return source.Except(target, new Itemify.Shared.Utils.EqualityComparer<T>(pred, hash));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, IEnumerable<T> target, Func<T, int> hash)
        {
            return source.Except(target, new Itemify.Shared.Utils.EqualityComparer<T>((a, b) => hash(a) == hash(b), hash));
        }

        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> source, IEnumerable<T> target, Func<T, T, bool> pred, Func<T, int> hash)
        {
            return source.Intersect(target, new Itemify.Shared.Utils.EqualityComparer<T>(pred, hash));
        }

        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> source, IEnumerable<T> target, Func<T, int> hash)
        {
            return source.Intersect(target, new Itemify.Shared.Utils.EqualityComparer<T>((a, b) => hash(a) == hash(b), hash));
        }

        public static Tuple<T, T> FirstLast<T>(this IEnumerable<T> source)
        {
            var first = true;
            var firstVal = default(T);
            var previous = default(T);
            foreach (var s in source)
            {
                previous = s;

                if (!first) continue;

                firstVal = s;
                first = false;
            }

            return !first ? new Tuple<T, T>(firstVal, previous) : null;
        }

        public static IEnumerable<IEnumerable<T>> Chunked<T>(this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return YieldChunkedElements(enumerator, batchSize - 1);
        }

        private static IEnumerable<T> YieldChunkedElements<T>(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (var i = 0; i < batchSize && source.MoveNext(); i++)
                yield return source.Current;
        }

        public static IEnumerable<IEnumerable<object>> SwapEnumerables(this IEnumerable<IEnumerable<object>> list, int columnCount)
        {
            var indexes = columnCount.Times();
            return list.Select(row => row.Zip(indexes, (o1, o2) => new { col = o2, val = o1 })).SelectMany(obj => obj.GroupBy(o => o.col)).Select(g => g.Select(o => o.val));
        }
    }
}
