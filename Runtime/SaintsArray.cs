using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public struct SaintsArray<T>: IWrapProp
        , IReadOnlyList<T>
        , ICollection
        , ICloneable
        // , IList  this is so fucked up. MS need this for index. Ignore this because IReadOnlyList solved it
        , IEnumerable
        , IStructuralComparable
    {
        [SerializeField]
        public T[] value;

#if UNITY_EDITOR
        public string EditorPropertyName => nameof(value);
#endif

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator T[](SaintsArray<T> saintsArray) => saintsArray.value;

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsArray<T>(T[] array) => new SaintsArray<T> { value = array };

        public override string ToString() => value.ToString();

        #region IReadOnlyList
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => value.Length;
        public T this[int index]
        {
            get => value[index];
            set => this.value[index] = value;  // ms just don't give any interface for this
        }

        #endregion

        #region ICollection

        public void CopyTo(Array array, int index) => value.CopyTo(array, index);
        public bool IsSynchronized => value.IsSynchronized;
        public object SyncRoot => value.SyncRoot;

        #endregion

        #region ICloneable
        public object Clone() => value.Clone();
        #endregion

        #region IStructuralComparable

        // array has this actually
        public int CompareTo(object other, IComparer comparer)
        {
            // ReSharper disable once PossibleNullReferenceException
            throw null;
        }
        #endregion

    }
}
