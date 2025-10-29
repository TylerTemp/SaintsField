using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsList<T>: IWrapProp, IList<T>
    {
        [SerializeField]
        public List<SaintsWrap<T>> value = new List<SaintsWrap<T>>();

#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        public static readonly string EditorPropertyName = nameof(value);
#endif

        public override string ToString()
        {
            return value.ToString();
        }

        public SaintsList()
        {
        }
        public SaintsList(IEnumerable<T> ie)
        {
            foreach (T element in ie)
            {
                value.Add(new SaintsWrap<T>(element));
            }
        }
        public SaintsList(int capacity)
        {
            value = new List<SaintsWrap<T>>(capacity);
        }

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator List<T>(SaintsList<T> saintsArray)
        {
            return saintsArray.value.Select(each => each.Value).ToList();
        }

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsList<T>(List<T> lis)
        {
            return new SaintsList<T>(lis);
        }

        #region IList

        public IEnumerator<T> GetEnumerator() => value.Select(each => each.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item) => value.Add(new SaintsWrap<T>(item));

        public void Clear() => value.Clear();

        public bool Contains(T item) => value.Any(each => (object)each.Value == (object)item);

        public void CopyTo(T[] array, int arrayIndex) => value.Select(each => each.Value).ToArray().CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            bool found = false;
            int removeAtLess1 = -1;
            foreach (SaintsWrap<T> wrap in value)
            {
                if ((object)wrap.Value == (object)item)
                {
                    found = true;
                    break;
                }

                removeAtLess1 += 1;
            }

            if (!found)
            {
                return false;
            }

            value.RemoveAt(removeAtLess1 + 1);
            return true;
        }

        public int Count => value.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            int index = 0;
            foreach (SaintsWrap<T> saintsWrap in value)
            {
                if ((object)saintsWrap.Value == (object)item)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void Insert(int index, T item) => value.Insert(index, new SaintsWrap<T>(item));

        public void RemoveAt(int index) => value.RemoveAt(index);

        public T this[int index]
        {
            get => value[index].Value;
            set => this.value[index] = new SaintsWrap<T>(value);
        }

        #endregion

        public void AddRange(IEnumerable<T> collection) => value.AddRange(collection.Select(each => new SaintsWrap<T>(each)));
    }
}
