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
        // , IEnumerable
        , IStructuralComparable
        , ISerializationCallbackReceiver
    {
        [SerializeField, Obsolete]
        public T[] value = {};

        [SerializeField] private List<SaintsWrap<T>> _saintsList = new List<SaintsWrap<T>>();
        [SerializeField] private int _saintsSerializedVersion;
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
        public static explicit operator SaintsArray<T>(T[] array) => new SaintsArray<T>(array);

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
            set => _array[index] = value;  // ms just don't give any interface for this
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

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if (_saintsSerializedVersion < 2)
            {
                _wrapType = SaintsWrap<T>.GuessWrapType();

                _saintsSerializedVersion = 2;
                _saintsList.Clear();
#pragma warning disable CS0612 // Type or member is obsolete
                foreach (T oldValue in value)
#pragma warning restore CS0612 // Type or member is obsolete
                {
                    _saintsList.Add(new SaintsWrap<T>(_wrapType, oldValue));
                }

                // ReSharper disable once RedundantJumpStatement
                return;
            }
#endif

#if UNITY_EDITOR
            // do nothing
#else
            _saintsList.Clear();
            foreach (T v in _array)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, v));
            }

#endif
        }

#if UNITY_EDITOR
        private HashSet<SaintsWrap<T>> _editorWatchedKeys = new HashSet<SaintsWrap<T>>();
#endif
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (_saintsSerializedVersion < 2)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                _array = value;
#pragma warning restore CS0612 // Type or member is obsolete
                return;
            }
#endif

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
            _array = Enumerable.Range(0, serCount).Select(_ => default(T)).ToArray();
            for (int index = 0; index < serCount; index++)
            {
                T v = _saintsList[index].GetValue();
                _array[index] = v;
            }

#if UNITY_EDITOR
            // do nothing
#else
            _saintsList.Clear();
#endif
        }

    }
}
