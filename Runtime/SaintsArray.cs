using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public struct SaintsArray<T>: ISaintsArray, IReadOnlyList<T>
    {
        [SerializeField, UIToolkit]
        public T[] value;

#if UNITY_EDITOR
        public string EditorArrayPropertyName => nameof(value);
#endif

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator T[](SaintsArray<T> saintsArray) => saintsArray.value;

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsArray<T>(T[] array) => new SaintsArray<T> { value = array };

        public override string ToString() => value.ToString();

        #region Array

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => value.Length;
        public T this[int index] => value[index];
        #endregion


    }
}
