using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class SaintsArray<T>: IWrapProp
        , IList<T>
        , IReadOnlyList<T>
        , IList
        , ICollection
        , ICloneable
        , IStructuralComparable
        , IStructuralEquatable
        , ISerializationCallbackReceiver
    {
        // [SerializeField, Obsolete]
        // public T[] value = {};

        // ReSharper disable once InconsistentNaming
        [SerializeField] public List<SaintsWrap<T>> _saintsList = new List<SaintsWrap<T>>();
#pragma warning disable CS0414 // Field is assigned but its value is never used
        [SerializeField] private int _saintsSerializedVersion;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        private const int SaintsSerializedVersionRuntime = 2;
        [SerializeField] private WrapType _wrapType;
        private T[] _array = Array.Empty<T>();

#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        public static readonly string EditorPropertyName = nameof(_saintsList);
#endif

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator T[](SaintsArray<T> saintsArray) => saintsArray._array;

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static implicit operator SaintsArray<T>(T[] array) => new SaintsArray<T>(array);

        public override string ToString() => _array.ToString();

        public SaintsArray()
        {
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
            _wrapType = SaintsWrap<T>.GuessWrapType();
        }

        public SaintsArray(IEnumerable<T> ie): this()
        {
            _array = ie.ToArray();
#if UNITY_EDITOR
            foreach (T element in _array)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, element));
            }
#endif
        }

        public SaintsArray(int capacity): this()
        {
            _array = new T[capacity];
#if UNITY_EDITOR
            foreach (T element in _array)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, element));
            }
#endif
        }

        #region IReadOnlyList
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _array.Length;
        public T this[int index]
        {
            get => _array[index];
            set
            {
                _array[index] = value;
#if UNITY_EDITOR
                _saintsList[index] = new SaintsWrap<T>(_wrapType, value);
#endif
            }
        }

        public bool IsReadOnly => ((ICollection<T>)_array).IsReadOnly;

        public void Add(T item) => ((ICollection<T>)_array).Add(item);
        public void Clear()
        {
            ((ICollection<T>)_array).Clear();
#if UNITY_EDITOR
            SyncSerializedArray();
#endif
        }
        public bool Contains(T item) => ((ICollection<T>)_array).Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _array.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => ((IList<T>)_array).IndexOf(item);
        public void Insert(int index, T item) => ((IList<T>)_array).Insert(index, item);
        public bool Remove(T item) => ((ICollection<T>)_array).Remove(item);
        public void RemoveAt(int index) => ((IList<T>)_array).RemoveAt(index);

        bool IList.IsFixedSize => ((IList)_array).IsFixedSize;
        bool IList.IsReadOnly => ((IList)_array).IsReadOnly;

        object IList.this[int index]
        {
            get => ((IList)_array)[index];
            set
            {
                ((IList)_array)[index] = value;
#if UNITY_EDITOR
                SyncSerializedArray();
#endif
            }
        }

        int IList.Add(object value) => ((IList)_array).Add(value);
        bool IList.Contains(object value) => ((IList)_array).Contains(value);
        int IList.IndexOf(object value) => ((IList)_array).IndexOf(value);
        void IList.Insert(int index, object value) => ((IList)_array).Insert(index, value);
        void IList.Remove(object value) => ((IList)_array).Remove(value);

        #endregion

        #region ICollection

        public void CopyTo(Array array, int index) => _array.CopyTo(array, index);
        public void CopyTo(Array array, long index) => _array.CopyTo(array, index);
        public bool IsSynchronized => _array.IsSynchronized;
        public object SyncRoot => _array.SyncRoot;

        #endregion

        #region ICloneable
        public object Clone() => _array.Clone();
        #endregion

        #region IStructuralComparable

        public int CompareTo(object other, IComparer comparer) =>
            ((IStructuralComparable)_array).CompareTo(Unwrap(other), comparer);
        #endregion

        #region IStructuralEquatable

        public bool Equals(object other, IEqualityComparer comparer) =>
            ((IStructuralEquatable)_array).Equals(Unwrap(other), comparer);

        public int GetHashCode(IEqualityComparer comparer) =>
            ((IStructuralEquatable)_array).GetHashCode(comparer);

        #endregion

        // Microsoft is a shithole
        public int Length => _array.Length;
        public long LongLength => _array.LongLength;
        public int Rank => _array.Rank;

        public int GetLength(int dimension) => _array.GetLength(dimension);
        public long GetLongLength(int dimension) => _array.GetLongLength(dimension);
        public int GetLowerBound(int dimension) => _array.GetLowerBound(dimension);
        public int GetUpperBound(int dimension) => _array.GetUpperBound(dimension);
        public T GetValue(int index) => _array[index];
        public void SetValue(T value, int index) => this[index] = value;
        public void Initialize()
        {
            _array.Initialize();
#if UNITY_EDITOR
            SyncSerializedArray();
#endif
        }

        private static object Unwrap(object other) =>
            other is SaintsArray<T> saintsArray ? saintsArray._array : other;

#if UNITY_EDITOR
        private void SyncSerializedArray()
        {
            _saintsList.Clear();
            foreach (T value in _array)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, value));
            }
        }
#endif

        public void OnBeforeSerialize()
        {
// #if UNITY_EDITOR
//             // ReSharper disable once InvertIf
//             if (_saintsSerializedVersion < 2)
//             {
//                 _wrapType = SaintsWrap<T>.GuessWrapType();
//
//                 _saintsSerializedVersion = 2;
//                 _saintsList.Clear();
// #pragma warning disable CS0612 // Type or member is obsolete
//                 foreach (T oldValue in value)
// #pragma warning restore CS0612 // Type or member is obsolete
//                 {
//                     _saintsList.Add(new SaintsWrap<T>(_wrapType, oldValue));
//                 }
//
//                 // ReSharper disable once RedundantJumpStatement
//                 return;
//             }
// #endif

// #if UNITY_EDITOR
//             // do nothing
// #else
//             _saintsList.Clear();
//             foreach (T v in _array)
//             {
//                 _saintsList.Add(new SaintsWrap<T>(_wrapType, v));
//             }
//
// #endif
        }

#if UNITY_EDITOR
        private List<SaintsWrap<T>> _editorWatchedKeys = new List<SaintsWrap<T>>();
#endif
        public void OnAfterDeserialize()
        {
// #if UNITY_EDITOR
//             if (_saintsSerializedVersion < 2)
//             {
// #pragma warning disable CS0612 // Type or member is obsolete
//                 _array = value;
// #pragma warning restore CS0612 // Type or member is obsolete
//                 return;
//             }
// #endif

#if UNITY_EDITOR
            IEnumerable<SaintsWrap<T>> extraKeys = _saintsList.Except(_editorWatchedKeys);
            foreach (SaintsWrap<T> keyWrap in extraKeys)
            {
                // Debug.Log($"add key listener");
                keyWrap.EditorOnAfterDeserializeChanged.AddListener(OnAfterDeserializeProcess);
                _editorWatchedKeys.Add(keyWrap);
            }
#endif
            OnAfterDeserializeProcess();
        }

        private void OnAfterDeserializeProcess()
        {
            int serCount = _saintsList.Count;
            _array = new T[serCount];
            for (int index = 0; index < serCount; index++)
            {
                T v = _saintsList[index].GetValue();
                // Debug.Log($"OnAfterDeserializeProcess [{index}] null: {v==null}({_saintsList[index].wrapType}/{_saintsList[index].value})");
                _array[index] = v;
            }

#if UNITY_EDITOR
            // do nothing
#else
            // _saintsList.Clear();
#endif
        }

    }
}
