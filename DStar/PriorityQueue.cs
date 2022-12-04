using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        public List<T> list { get; private set; }

        public PriorityQueue()
        {
            list = new List<T>();
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            int i = 0;
            while (i < list.Count && item.CompareTo(list[i]) >= 0)
            {
                i++;
            }

            list.Insert(i, item);
        }

        public T Peek()
        {
            return list[0];
        }

        public T Poll()
        {
            T ret = list[0];
            list = list.Skip(1).ToList();
            return ret;
        }

        public void Remove(T item)
        {
            list.Remove(item);
        }

        public void Clear()
        {
            list = new List<T>();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }
    }
}
