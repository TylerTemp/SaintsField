using System.Collections.Generic;
using System.Linq;

namespace SaintsField.Editor.Linq
{
    public static class LinqExtend
    {
        public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> enumerable, int startIndex = 0)
            => enumerable.Select((source, index) => (value: source, index: index + startIndex));

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (T element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
