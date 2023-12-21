using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;

namespace SaintsField
{
    public class AdvancedDropdownList<T> : IAdvancedDropdownList
    {
        // name, value, disabled, icon, separator
        private readonly List<ValueTuple<string, object, List<object>, bool, string, bool>> _values;

        public AdvancedDropdownList() => _values = new List<ValueTuple<string, object, List<object>, bool, string, bool>>();

        public AdvancedDropdownList(IEnumerable<ValueTuple<string, T>> value) =>
            _values = value.Select(each => (each.Item1, (object)each.Item2, new List<object>(), false, (string)null, false)).ToList();

        public AdvancedDropdownList(IEnumerable<ValueTuple<string, T, bool>> value) => _values =
            value.Select(each => (each.Item1, (object)each.Item2, new List<object>(), each.Item3, (string)null, false)).ToList();

        public void Add(string displayName, T value) => _values.Add((displayName, value, new List<object>(), false, null, false));

        public void Add(string displayName, T value, bool disabled) =>
            _values.Add((displayName, value, new List<object>(), disabled, null, false));
        public void Add(string displayName, T value, bool disabled, string icon) =>
            _values.Add((displayName, value, new List<object>(), disabled, icon, false));

        public void Add(string displayName, object value, List<object> children, bool disabled, string icon, bool isSep) => _values.Add((
            displayName, value, children, disabled, icon, isSep));

        public void Add((string, object, List<object>, bool, string, bool) tuple) => _values.Add(tuple);

        public void AddSeparator(string separator = "") => _values.Add((separator, default, new List<object>(),  default, default, true));

        public static (string, object, bool, string, bool) Separator(string separatorPath = "") =>
            (separatorPath, default, default, null, true);

        // public static (string, object, bool, string, bool) Item(string name, T item) => (name, item, false, null, false);
        //
        // public static (string, object, bool, string, bool) Item(string name, T item, bool disabled) =>
        //     (name, item, disabled, null, false);

        // public void AddRange(IEnumerable<ValueTuple<string, T, bool, string, bool>> pairs)
        // {
        //     foreach ((string, T, bool, string, bool) pair in pairs)
        //     {
        //         _values.Add(pair);
        //     }
        // }
        //
        // public void AddRange(IEnumerable<ValueTuple<string, T>> pairs) =>
        //     AddRange(pairs.Select(each => (each.Item1, each.Item2, false, (string)null, false)));
        //
        // public void AddRange(IEnumerable<ValueTuple<string, T, bool>> pairs) =>
        //     AddRange(pairs.Select(each => (each.Item1, each.Item2, each.Item3, (string)null, false)));

        public IEnumerator<ValueTuple<string, object, List<object>, bool, string, bool>> GetEnumerator() => _values.GetEnumerator();

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
        public int Count => _values.Count;

        public (string, object, List<object>, bool, string, bool) this[int index] => _values[index];
    }
}
