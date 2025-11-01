using System;
using System.Collections.Generic;

namespace TDPG.Templates.Pathfinding
{

    public class PriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<(TItem item, TPriority priority)> elements = new List<(TItem, TPriority)>();

        public int Count => elements.Count;

        public void Enqueue(TItem item, TPriority priority)
        {
            elements.Add((item, priority));
        }

        public TItem Dequeue()
        {
            if (elements.Count == 0)
                throw new InvalidOperationException("PriorityQueue is empty.");

            int bestIndex = 0;
            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].priority.CompareTo(elements[bestIndex].priority) < 0)
                {
                    bestIndex = i;
                }
            }

            TItem bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public bool Contains(TItem item)
        {
            return elements.Exists(e => EqualityComparer<TItem>.Default.Equals(e.item, item));
        }

        public void Clear()
        {
            elements.Clear();
        }
    }
}