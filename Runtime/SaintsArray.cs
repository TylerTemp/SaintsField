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
        , IReadOnlyList<T>
        , ICollection
        , ICloneable
        // , IList  this is so fucked up. MS need this for index. Ignore this because IReadOnlyList solved it
        // , IEnumerable  IReadOnlyList has this
        , IStructuralComparable
    {
        [SerializeField, Obsolete]
        public T[] value = {};

        [SerializeField]
        private List<SaintsWrap<T>> _saintsList = new List<SaintsWrap<T>>();
        [SerializeField] private int _saintsSerializedVersion;

        private T[] _array = Array.Empty<T>();

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropertyName => nameof(_saintsList);
#endif

        #region Serialization

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (_saintsSerializedVersion == 0)
            {
                _saintsSerializedVersion = 1;
                _saintsList.Clear();
#pragma warning disable CS0612 // Type or member is obsolete
                foreach (T oldValue in value)
#pragma warning restore CS0612 // Type or member is obsolete
                {
                    _saintsList.Add(new SaintsWrap<T>(oldValue));
                }

                return;
            }
#endif

#if UNITY_EDITOR
            // do nothing
#else
            _saintsList.Clear();
            foreach (T v in _array)
            {
                _saintsList.Add(new SaintsWrap<T>(v));
            }
#endif
        }

#if UNITY_EDITOR
        private HashSet<SaintsWrap<T>> _editorWatchedKeys = new HashSet<SaintsWrap<T>>();
#endif
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            IEnumerable<SaintsWrap<T>> extraKeys = _saintsList.Except(_editorWatchedKeys);
            foreach (SaintsWrap<T> keyWrap in extraKeys)
            {
                // Debug.Log($"add key listener");
                keyWrap.onAfterDeserializeChanged.AddListener(OnAfterDeserializeProcess);
                _editorWatchedKeys.Add(keyWrap);
            }
#endif
            OnAfterDeserializeProcess();
        }

        private void OnAfterDeserializeProcess()
        {
#if UNITY_EDITOR
            if (_saintsSerializedVersion == 0)
            {
                // _saintsSerializedVersion = 1;
#pragma warning disable CS0612 // Type or member is obsolete
                _array = value;
#pragma warning restore CS0612 // Type or member is obsolete
                return;
            }
#endif

            int serCount = _saintsList.Count;
            if (serCount != _array.Length)
            {
                _array = new T[serCount];
            }

            for (int index = 0; index < serCount; index++)
            {
                T v = _saintsList[index].Value;
                _array[index] = v;
            }

#if UNITY_EDITOR
            // do nothing
#else
            _saintsList.Clear();
#endif
        }

        #endregion


        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator T[](SaintsArray<T> saintsArray) => saintsArray._array;

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsArray<T>(T[] array) => new SaintsArray<T>(array);

        public override string ToString() => _array.ToString();

        public SaintsArray()
        {
        }

        public SaintsArray(IEnumerable<T> ie)
        {
            _array = ie.ToArray();
#if UNITY_EDITOR
            foreach (T element in _array)
            {
                _saintsList.Add(new SaintsWrap<T>(element));
            }
#endif
        }

        public SaintsArray(int capacity)
        {
            _array = new T[capacity];
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
                _saintsList[index] = new SaintsWrap<T>(value);
#endif
            }
        }

        #endregion

        #region ICollection

        public void CopyTo(Array array, int index) => _array.CopyTo(array, index);
        public bool IsSynchronized => _array.IsSynchronized;
        public object SyncRoot => _array.SyncRoot;

        #endregion

        #region ICloneable
        public object Clone() => _array.Clone();
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
