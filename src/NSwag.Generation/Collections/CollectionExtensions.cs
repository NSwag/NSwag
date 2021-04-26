using System;
using System.Collections.Generic;
using System.Linq;

namespace NSwag.Generation.Collections
{
    internal static class CollectionExtensions
    {
        /// <summary>Returns the only element of a sequence that satisfies a specified condition or a creates a new object and adds it to the collection if no such element exists; this method throws an exception if more than one element satisfies the condition.</summary>
        /// <returns>The single element of the input sequence that satisfies the condition, or a new TSource instance if no such element is found.</returns>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static TSource SingleOrNew<TSource>(this ICollection<TSource> source, Func<TSource, bool> predicate) where TSource : new()
        {
            var item = source.SingleOrDefault(predicate);
            if (item == null)
            {
                item = new TSource();
                source.Add(item);
            }

            return item;
        }
    }
}