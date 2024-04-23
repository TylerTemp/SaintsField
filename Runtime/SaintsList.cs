using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public struct SaintsList<T>: ISaintsArray, IList<T>
    {
        [SerializeField]
        public List<T> value;

#if UNITY_EDITOR
        public string EditorArrayPropertyName => nameof(value);
#endif

        public IEnumerator<T> GetEnumerator()
        {
            return value.GetEnumerator();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator List<T>(SaintsList<T> saintsArray)
        {
            return saintsArray.value;
        }

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsList<T>(List<T> array)
        {
            return new SaintsList<T> { value = array };
        }

        #region IList

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            value.Add(item);
        }

        public void Clear()
        {
            value.Clear();
        }

        public bool Contains(T item)
        {
            return value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            value.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return value.Remove(item);
        }

        public int Count => value.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            return value.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            value.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            value.RemoveAt(index);
        }

        public T this[int index]
        {
            get => value[index];
            set => this.value[index] = value;
        }

        #endregion
    }
}
