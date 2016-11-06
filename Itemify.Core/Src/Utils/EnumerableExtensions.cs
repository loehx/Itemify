using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Itemify.Core.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Tuple<TA, TB>> InnerJoin<TA, TB>(this IEnumerable<TA> sourceA, IEnumerable<TB> sourceB)
        {
            using (var eA = sourceA.GetEnumerator())
            using (var eB = sourceB.GetEnumerator())
            {
                while (eA.MoveNext() && eB.MoveNext())
                {
                    yield return Tuple.Create(eA.Current, eB.Current);
                }
            }
        }

        public static IEnumerable<Tuple<TA, TB>> OuterJoin<TA, TB>(this IEnumerable<TA> sourceA, IEnumerable<TB> sourceB)
        {
            using (var eA = sourceA.GetEnumerator())
            using (var eB = sourceB.GetEnumerator())
            {
                var eAOk = false;
                var eBOk = false;

                while ((eAOk = eA.MoveNext()) && (eBOk = eB.MoveNext()))
                {
                    yield return Tuple.Create(
                        eAOk ? eA.Current : default(TA),
                        eBOk ? eB.Current : default(TB));
                }
            }
        }


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

        public static List<T2> ToList<T, T2>(this ICollection<T> source, Func<T, T2> factory)
        {
            var result = new List<T2>(source.Count);
            result.AddRange(source.Select(factory));
            return result;
        }

        public static List<T2> ToList<T, T2>(this IEnumerable<T> source, Func<T, T2> factory)
        {
            var collection = source as ICollection<T>;
            if (collection != null)
                return collection.ToList(factory);

            return source.Select(factory).ToList();
        }

        public static IEnumerable<T> Times<T>(this int count, Func<int, T> factory)
        {
            return count.Times().Select(factory);
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


        public static IEnumerable<Tuple<T, T>> GroupByTuples<T>(this IEnumerable<T> source)
        {
            var first = true;
            var previous = default(T);
            foreach (var entry in source)
            {
                if (first)
                {
                    previous = entry;
                    first = false;
                    continue;
                }

                yield return new Tuple<T, T>(previous, entry);

                previous = entry;
            }
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

        public static IEnumerable<int> EnumerateTo(this int start, int end)
        {
            for (; start <= end; start++)
            {
                yield return start;
            }
        }
    }
}
