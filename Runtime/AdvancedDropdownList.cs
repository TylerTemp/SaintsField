using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;

namespace SaintsField
{
    public class AdvancedDropdownList<T> : IAdvancedDropdownList
    {
        public string displayName { get; set; }

        public T typeValue;

        public object value => typeValue;

        public List<AdvancedDropdownList<T>> typeChildren;

        public IReadOnlyList<IAdvancedDropdownList> children =>
            typeChildren.Select(each => (IAdvancedDropdownList)each).ToList();

        public bool disabled { get; set; }
        public string icon { get; set; }
        public bool isSeparator { get; set; }

        public AdvancedDropdownList(string displayName, T value, bool disabled = false, string icon = null,
            bool isSeparator = false)
        {
            this.displayName = displayName;
            this.typeValue = value;
            this.typeChildren = new List<AdvancedDropdownList<T>>();
            this.disabled = disabled;
            this.icon = icon;
            this.isSeparator = isSeparator;
        }

        public AdvancedDropdownList(string displayName, IEnumerable<AdvancedDropdownList<T>> children, bool disabled = false, string icon = null,
            bool isSeparator = false)
        {
            this.displayName = displayName;
            // this.value = value;
            typeChildren = children.ToList();
            this.disabled = disabled;
            this.icon = icon;
            this.isSeparator = isSeparator;
        }

        public void Add(AdvancedDropdownList<T> child) => typeChildren.Add(child);

        public void AddSeparator() => typeChildren.Add(Separator());

        public static AdvancedDropdownList<T> Separator() =>
            new AdvancedDropdownList<T>("", (T)default, false, null, true);

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

        // public IEnumerator<AdvancedDropdownList<T>> GetEnumerator() => children.GetEnumerator();

        public IEnumerator<IAdvancedDropdownList> GetEnumerator()
        {
            foreach (AdvancedDropdownList<T> child in typeChildren)
            {
                yield return child;
            }
        }

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
        public int Count => typeChildren.Count;

        public AdvancedDropdownList<T> this[int index] => typeChildren[index];
        IAdvancedDropdownList IReadOnlyList<IAdvancedDropdownList>.this[int index] => typeChildren[index];
    }
}
