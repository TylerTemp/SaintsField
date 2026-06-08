using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    public class Dropdown<T> : IDropdown
    {
        public IReadOnlyList<string> absolutePathFragments { get; private set; }
        public string displayName { get; private set; }

        private T _typeValue;

        public object value => _typeValue;

        private List<Dropdown<T>> _typeChildren;

        public IReadOnlyList<IDropdown> children =>
            _typeChildren.Select(each => (IDropdown)each).ToList();

        public bool disabled { get; }
        public string icon { get; }
        public bool isSeparator { get; }

        public void SetChildren(List<Dropdown<T>> newChildren) => _typeChildren = newChildren;

        public Dropdown()
        {
            displayName = "";
            absolutePathFragments = new List<string> { "" };
            _typeValue = default;
            _typeChildren = new List<Dropdown<T>>();
            disabled = false;
            icon = null;
            isSeparator = false;
        }

        public Dropdown(string displayName, bool disabled = false, string icon = null)
        {
            this.displayName = displayName;
            absolutePathFragments = new List<string> { displayName };
            _typeValue = default;
            _typeChildren = new List<Dropdown<T>>();
            this.disabled = disabled;
            this.icon = icon;
            isSeparator = false;
        }

        public Dropdown(string displayName, T value, bool disabled = false, string icon = null, bool isSeparator = false)
        {
            this.displayName = displayName;
            absolutePathFragments = new List<string> { displayName };
            _typeValue = value;
            _typeChildren = new List<Dropdown<T>>();
            this.disabled = disabled;
            this.icon = icon;
            this.isSeparator = isSeparator;
        }

        public Dropdown(string displayName, IEnumerable<Dropdown<T>> children, bool disabled = false, string icon = null,
            bool isSeparator = false)
        {
            this.displayName = displayName;
            absolutePathFragments = new List<string> { displayName };
            // this.value = value;
            _typeChildren = children.ToList();
            this.disabled = disabled;
            this.icon = icon;
            this.isSeparator = isSeparator;
        }

        public void Add(Dropdown<T> child)
        {
            child.absolutePathFragments = absolutePathFragments.Append(child.displayName).ToArray();
            _typeChildren.Add(child);
        }

        // this will parse "/"
        public void Add(string displayNames, T value, bool disabled = false, string icon = null, ICollection<string> extraSearches=null)
        {
            AddByNames(this, new Queue<string>(RuntimeUtil.SeparatePath(displayNames)), value, disabled, icon, extraSearches);
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

        // ReSharper disable once MemberCanBePrivate.Global
        public static void AddByNames(Dropdown<T> container, Queue<string> nameQuery, T value, bool disabled = false, string icon = null, ICollection<string> extraSearches=null)
        {
            int curCount = nameQuery.Count;
            string curName = curCount == 0 ? "": nameQuery.Dequeue();
            int leftCount = nameQuery.Count;
            if (leftCount == 0)
            {
                container.Add(curName == ""? Separator(): new Dropdown<T>(curName, value, disabled, icon)
                {
                    ExtraSearches = extraSearches ?? new HashSet<string>(),
                });
                return;
            }

            IDropdown matchedChild = container.children.FirstOrDefault(each => each.displayName == curName);
            Dropdown<T> targetChild;
            if (matchedChild != null)
            {
                targetChild = (Dropdown<T>)matchedChild;
            }
            else
            {
                targetChild = new Dropdown<T>(curName);
                container.Add(targetChild);
            }
            // ReSharper disable once TailRecursiveCall
            AddByNames(targetChild, nameQuery, value, disabled, icon);
        }

        public void AddSeparator()
        {
            Dropdown<T> sep = Separator();
            sep.absolutePathFragments = new List<string>(absolutePathFragments);
            _typeChildren.Add(sep);
        }

        public int ChildCount() => _typeChildren.Count(each => !each.isSeparator);
        public int SepCount() => _typeChildren.Count(each => each.isSeparator);

        public static Dropdown<T> Separator() =>
            new Dropdown<T>("", (T)default, false, null, true);

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

        // public IEnumerator<Dropdown<T>> GetEnumerator() => children.GetEnumerator();

        public IEnumerator<IDropdown> GetEnumerator() => _typeChildren.Cast<IDropdown>().GetEnumerator();

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

        public IDropdown this[int index] => _typeChildren[index];

        public void SelfCompact()
        {
            foreach (Dropdown<T> child in _typeChildren)
            {
                child.IterCompact();
            }
        }

        public ICollection<string> ExtraSearches { get; set; } = new HashSet<string>();

        private void IterCompact()
        {
            if (isSeparator)
            {
                // Debug.Log($"skip separator");
                return;
            }

            if (_typeChildren.Count == 0)
            {
                // Debug.Log($"skip as typeChildren 0 for {displayName}");
                return;
            }

            foreach (Dropdown<T> child in _typeChildren)
            {
                child.IterCompact();
            }

            // merge single foldout, but not single item
            // ReSharper disable once InvertIf
            if (_typeChildren.Count == 1 && _typeChildren[0]._typeChildren.Count != 0)
            {
                // Debug.Log($"merge {this.displayName} with {_typeChildren[0].displayName}");
                Dropdown<T> child = _typeChildren[0];
                string myName = displayName;
                string childName = child.displayName;
                string childIcon = child.icon;
                displayName = $"{myName}/{(string.IsNullOrEmpty(childIcon) ? "" : $"<icon={childIcon}/>")}{childName}";
                // Debug.Log($"displayName={displayName}");

                if (child._typeChildren.Count > 0)
                {
                    _typeChildren = child._typeChildren.ToList();
                    // Debug.Log($"{displayName} set children to {string.Join("|", _typeChildren.Select(each => each.displayName))}");
                }
                else
                {
                    _typeValue = child._typeValue;
                    // Debug.Log($"{displayName} set value to {_typeValue}");
                    _typeChildren.Clear();
                }
                // Debug.Log($"again IterCompact {this.displayName}");
                // IterCompact();
            }
            // else
            // {
            //     Debug.Log($"not merge {this.displayName} with {string.Join("|", _typeChildren.Select(each => each.displayName))}");
            // }
        }
    }
}
