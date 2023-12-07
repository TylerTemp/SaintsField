using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;

namespace SaintsField
{
    public class DropdownList<T> : IDropdownList
    {
        // name, object, disabled, separator
        private readonly List<ValueTuple<string, object, bool, bool>> _values;

        public DropdownList() => _values = new List<ValueTuple<string, object, bool, bool>>();
        public DropdownList(IEnumerable<ValueTuple<string, T>> value) => _values = value.Select(each => (each.Item1, (object)each.Item2, false, false)).ToList();
        public DropdownList(IEnumerable<ValueTuple<string, T, bool>> value) => _values = value.Select(each => (each.Item1, (object)each.Item2, each.Item3, false)).ToList();

        public void Add(string displayName, T value) => _values.Add((displayName, value, false, false));
        public void Add(string displayName, T value, bool disabled) => _values.Add((displayName, value, disabled, false));
        public void Add((string, object, bool, bool) tuple) => _values.Add(tuple);

        public void AddSeparator(string separator="") => _values.Add((separator, default, default, true));

        public static (string, object, bool, bool) Separator(string separatorPath="") => (separatorPath, default, default, true);
        public static (string, object, bool, bool) Item(string name, T item) => (name, item, false, false);
        public static (string, object, bool, bool) Item(string name, T item, bool disabled) => (name, item, disabled, false);

        public void AddRange(IEnumerable<ValueTuple<string, T, bool, bool>> pairs)
        {
            foreach ((string, T, bool, bool) pair in pairs)
            {
                _values.Add(pair);
            }
        }
        public void AddRange(IEnumerable<ValueTuple<string, T>> pairs) =>
            AddRange(pairs.Select(each => (each.Item1, each.Item2, false, false)));
        public void AddRange(IEnumerable<ValueTuple<string, T, bool>> pairs) =>
            AddRange(pairs.Select(each => (each.Item1, each.Item2, each.Item3, false)));

        public IEnumerator<ValueTuple<string, object, bool, bool>> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // public static explicit operator DropdownList<object>(DropdownList<T> target)
        // {
        //     DropdownList<object> result = new DropdownList<object>();
        //     foreach (ValueTuple<string, object, bool> kvp in target)
        //     {
        //         result.Add((kvp.Item1, kvp.Item2, false));
        //     }
        //
        //     return result;
        // }
    }
}
