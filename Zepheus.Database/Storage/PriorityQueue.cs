using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zepheus.Database.Storage
{
    public class PriorityQueue<TValue> : PriorityQueue<TValue, int> { }

    public class PriorityQueue<TValue, TPriority> where TPriority : IComparable
    {
        private SortedDictionary<TPriority, Queue<TValue>> dict = new SortedDictionary<TPriority, Queue<TValue>>();

        public int Count { get; private set; }
        public bool Empty { get { return Count == 0; } }

        public void Enqueue(TValue val)
        {
            Enqueue(val, default(TPriority));
        }

        public void Enqueue(TValue val, TPriority pri)
        {
            ++Count;
            if (!dict.ContainsKey(pri)) dict[pri] = new Queue<TValue>();
            dict[pri].Enqueue(val);
        }

        public TValue Dequeue()
        {
            --Count;
            var item = dict.Last();
            if (item.Value.Count == 1) dict.Remove(item.Key);
            return item.Value.Dequeue();
        }
    }
}
