using System.Collections.Concurrent;

namespace GenPhoto.Extensions
{
    internal static class QueueExtensions
    {
        public static void EnqueueRange<T>(this ConcurrentQueue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}
