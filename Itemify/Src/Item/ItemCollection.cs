using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Itemify.Item
{
    public class ItemCollection<T> : IList<T>
        where T: IItemicItem
    {
        private List<T> inner { get; }


        public ItemCollection()
        {
            inner = new List<T>();
        }

        public ItemCollection(IEnumerable<T> items)
        {
            inner = new List<T>(items);
        }

        public void Add(T item)
        {
            inner.Add(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            inner.AddRange(collection);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return inner.AsReadOnly();
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return inner.BinarySearch(index, count, item, comparer);
        }

        public int BinarySearch(T item)
        {
            return inner.BinarySearch(item);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return inner.BinarySearch(item, comparer);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public bool Contains(T item)
        {
            return inner.Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return inner.ConvertAll(converter);
        }

        public void CopyTo(T[] array)
        {
            inner.CopyTo(array);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            inner.CopyTo(index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            inner.CopyTo(array, arrayIndex);
        }

        public bool Exists(Predicate<T> match)
        {
            return inner.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return inner.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return inner.FindAll(match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return inner.FindIndex(match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return inner.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return inner.FindIndex(startIndex, count, match);
        }

        public T FindLast(Predicate<T> match)
        {
            return inner.FindLast(match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return inner.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return inner.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return inner.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            inner.ForEach(action);
        }

        public List<T> GetRange(int index, int count)
        {
            return inner.GetRange(index, count);
        }

        public int IndexOf(T item)
        {
            return inner.IndexOf(item);
        }

        public int IndexOf(T item, int index)
        {
            return inner.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return inner.IndexOf(item, index, count);
        }

        public void Insert(int index, T item)
        {
            inner.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            inner.InsertRange(index, collection);
        }

        public int LastIndexOf(T item)
        {
            return inner.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index)
        {
            return inner.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return inner.LastIndexOf(item, index, count);
        }

        public bool Remove(T item)
        {
            return inner.Remove(item);
        }

        public int RemoveAll(Predicate<T> match)
        {
            return inner.RemoveAll(match);
        }

        public void RemoveAt(int index)
        {
            inner.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            inner.RemoveRange(index, count);
        }

        public void Reverse()
        {
            inner.Reverse();
        }

        public void Reverse(int index, int count)
        {
            inner.Reverse(index, count);
        }

        public void Sort()
        {
            inner.Sort();
        }

        public void Sort(IComparer<T> comparer)
        {
            inner.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            inner.Sort(index, count, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            inner.Sort(comparison);
        }

        public T[] ToArray()
        {
            return inner.ToArray();
        }

        public void TrimExcess()
        {
            inner.TrimExcess();
        }

        public bool TrueForAll(Predicate<T> match)
        {
            return inner.TrueForAll(match);
        }

        public int Capacity
        {
            get { return inner.Capacity; }
            set { inner.Capacity = value; }
        }

        public int Count
        {
            get { return inner.Count; }
        }

        public T this[int index]
        {
            get { return inner[index]; }
            set { inner[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}