using System;
using System.Collections.Generic;

namespace TDPG.Templates.Pathfinding
{

    /// <summary>
    /// A generic Priority Queue implementation helper for A* pathfinding.
    /// <br/>
    /// This is a <b>Min-Queue</b>: items with the <i>lowest</i> priority value are Dequeued first.
    /// </summary>
    /// <typeparam name="TItem">The type of object stored (e.g., Vector3 coordinates).</typeparam>
    /// <typeparam name="TPriority">The numeric type used for sorting (e.g., float cost). Must be comparable.</typeparam>
    public class PriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<(TItem item, TPriority priority)> elements = new List<(TItem, TPriority)>();

        /// <summary>
        /// The number of elements currently in the queue.
        /// </summary>
        public int Count => elements.Count;

        /// <summary>
        /// Adds an item to the queue with an associated priority score.
        /// </summary>
        /// <param name="item">The data item to store.</param>
        /// <param name="priority">The cost/score. Lower values will be processed sooner.</param>
        public void Enqueue(TItem item, TPriority priority)
        {
            elements.Add((item, priority));
        }

        /// <summary>
        /// Removes and returns the item with the lowest priority value (best score).
        /// </summary>
        /// <returns>The item with the minimum priority key.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
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

        /// <summary>
        /// Checks if a specific item is already present in the queue.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>True if found, False otherwise.</returns>
        public bool Contains(TItem item)
        {
            return elements.Exists(e => EqualityComparer<TItem>.Default.Equals(e.item, item));
        }

        /// <summary>
        /// Removes all elements from the queue.
        /// </summary>
        public void Clear()
        {
            elements.Clear();
        }
    }
}