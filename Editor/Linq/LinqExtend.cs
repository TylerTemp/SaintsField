using System.Collections.Generic;
using System.Linq;

namespace SaintsField.Editor.Linq
{
    public static class LinqExtend
    {
        public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> enumerable, int startIndex = 0)
            => enumerable.Select((source, index) => (value: source, index: index + startIndex));
    }
}
