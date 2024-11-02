using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;

namespace SaintsField
{
    public class AdvancedDropdownList<T> : IAdvancedDropdownList
    {
        public string displayName { get; }

        private readonly T _typeValue;

        public object value => _typeValue;

        private List<AdvancedDropdownList<T>> _typeChildren;

        public IReadOnlyList<IAdvancedDropdownList> children =>
            _typeChildren.Select(each => (IAdvancedDropdownList)each).ToList();

        public bool disabled { get; }
        public string icon { get; }
        public bool isSeparator { get; }

        public void SetChildren(List<AdvancedDropdownList<T>> newChildren) => _typeChildren = newChildren;

        public AdvancedDropdownList()
        {
            displayName = "";
            _typeValue = default;
            _typeChildren = new List<AdvancedDropdownList<T>>();
            disabled = false;
            icon = null;
            isSeparator = false;
        }

        public AdvancedDropdownList(string displayName, bool disabled = false, string icon = null)
        {
            this.displayName = displayName;
            _typeValue = default;
            _typeChildren = new List<AdvancedDropdownList<T>>();
            this.disabled = disabled;
            this.icon = icon;
            isSeparator = false;
        }

        public AdvancedDropdownList(string displayName, T value, bool disabled = false, string icon = null, bool isSeparator = false)
        {
            this.displayName = displayName;
            _typeValue = value;
            _typeChildren = new List<AdvancedDropdownList<T>>();
            this.disabled = disabled;
            this.icon = icon;
            this.isSeparator = isSeparator;
        }

        public AdvancedDropdownList(string displayName, IEnumerable<AdvancedDropdownList<T>> children, bool disabled = false, string icon = null,
            bool isSeparator = false)
        {
            this.displayName = displayName;
            // this.value = value;
            _typeChildren = children.ToList();
            this.disabled = disabled;
            this.icon = icon;
            this.isSeparator = isSeparator;
        }

        public void Add(AdvancedDropdownList<T> child) => _typeChildren.Add(child);

        // this will parse "/"
        public void Add(string displayNames, T value, bool disabled = false, string icon = null)
        {
            AddByNames(this, new Queue<string>(displayNames.Split('/')), value, disabled, icon);
        }

        // this add a separator
        public void Add(string displayNames)
        {
            // ReSharper disable once MergeIntoLogicalPattern
            if (displayNames == "" || displayNames == "/")
            {
                AddSeparator();
                return;
            }

            string useNames = displayNames.EndsWith("/")
                ? displayNames
                : displayNames + "/";

            Add(useNames, default);

        }

        private static void AddByNames(AdvancedDropdownList<T> container, Queue<string> nameQuery, T value, bool disabled = false, string icon = null)
        {
            string curName = nameQuery.Dequeue();
            if (nameQuery.Count == 0)
            {
                container.Add(curName == ""? Separator(): new AdvancedDropdownList<T>(curName, value, disabled, icon));
                return;
            }
            IAdvancedDropdownList matchedChild = container.children.FirstOrDefault(each => each.displayName == curName);
            AdvancedDropdownList<T> targetChild;
            if (matchedChild != null)
            {
                targetChild = (AdvancedDropdownList<T>)matchedChild;
            }
            else
            {
                targetChild = new AdvancedDropdownList<T>(curName);
                container.Add(targetChild);
            }
            // ReSharper disable once TailRecursiveCall
            AddByNames(targetChild, nameQuery, value, disabled, icon);
        }

        public void AddSeparator() => _typeChildren.Add(Separator());

        public int ChildCount() => _typeChildren.Count(each => !each.isSeparator);
        public int SepCount() => _typeChildren.Count(each => each.isSeparator);

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

        public IEnumerator<IAdvancedDropdownList> GetEnumerator() => _typeChildren.Cast<IAdvancedDropdownList>().GetEnumerator();

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
        public int Count => _typeChildren.Count;

        public AdvancedDropdownList<T> this[int index] => _typeChildren[index];
        IAdvancedDropdownList IReadOnlyList<IAdvancedDropdownList>.this[int index] => _typeChildren[index];
    }
}
