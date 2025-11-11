using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class SaintsList<T>: IList<T>, ISerializationCallbackReceiver
    {
        [SerializeField, Obsolete]
        public List<T> value = new List<T>();

        [SerializeField] private List<SaintsWrap<T>> _saintsList = new List<SaintsWrap<T>>();
        [SerializeField] private int _saintsSerializedVersion;
        private const int SaintsSerializedVersionRuntime = 2;
        [SerializeField] private WrapType _wrapType;

        private List<T> _list = new List<T>();

        #region Serialization

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (_saintsSerializedVersion < 2)
            {
                _wrapType = RuntimeUtil.EditorWrapMigrateFrom1(_saintsList);
            }
#endif

#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if (_saintsSerializedVersion == 0)
            {
                _saintsSerializedVersion = 1;
                _saintsList.Clear();
#pragma warning disable CS0612 // Type or member is obsolete
                foreach (T oldValue in value)
#pragma warning restore CS0612 // Type or member is obsolete
                {
                    _saintsList.Add(new SaintsWrap<T>(_wrapType, oldValue));
                }

                _saintsSerializedVersion = SaintsSerializedVersionRuntime;
                // ReSharper disable once RedundantJumpStatement
                return;
            }
#endif

#if UNITY_EDITOR
            // do nothing
#else
            _saintsList.Clear();
            foreach (T v in _list)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, v));
            }

#endif
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
        }

#if UNITY_EDITOR
        private HashSet<SaintsWrap<T>> _editorWatchedKeys = new HashSet<SaintsWrap<T>>();
#endif
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (_saintsSerializedVersion == 0)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                _list = value;
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
            _list.Clear();

            int serCount = _saintsList.Count;
            for (int index = 0; index < serCount; index++)
            {
                T v = _saintsList[index].GetValue();
                _list.Add(v);
            }

#if UNITY_EDITOR
            // do nothing
#else
            _saintsList.Clear();
#endif
        }

        #endregion

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropertyName => nameof(_saintsList);
#endif

        public override string ToString()
        {
            return _list.ToString();
        }

        public SaintsList()
        {
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
            _wrapType = SaintsWrap<T>.GuessWrapType();
        }
        public SaintsList(IEnumerable<T> ie): this()
        {
            _list = new List<T>(ie);
#if UNITY_EDITOR
            foreach (T element in _list)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, element));
            }
#endif
        }
        public SaintsList(int capacity): this()
        {
            _list = new List<T>(capacity);
        }

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator List<T>(SaintsList<T> saintsList)
        {
            return saintsList._list;
        }

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsList<T>(List<T> lis)
        {
            return new SaintsList<T>(lis);
        }

        #region IList

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
#if UNITY_EDITOR
            _saintsList.Add(new SaintsWrap<T>(_wrapType, item));
#endif
            _list.Add(item);
        }

        public void Clear()
        {
#if UNITY_EDITOR
            _saintsList.Clear();
#endif
            _list.Clear();
        }

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            bool result = _list.Remove(item);

#if UNITY_EDITOR
            if (result)
            {
                int foundIndex = -1;

                for (int index = 0; index < _saintsList.Count; index++)
                {
                    T v = _saintsList[index].GetValue();
                    // ReSharper disable once InvertIf
                    if (EqualityComparer<T>.Default.Equals(v, item))
                    {
                        foundIndex = index;
                        break;
                    }
                }

                if (foundIndex != -1)
                {
                    _saintsList.RemoveAt(foundIndex);
                }
            }
#endif

            return result;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
#if UNITY_EDITOR
            _saintsList.Insert(index, new SaintsWrap<T>(_wrapType, item));
#endif
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
#if UNITY_EDITOR
            _saintsList.RemoveAt(index);
#endif
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                _list[index] = value;
#if UNITY_EDITOR
                _saintsList[index] = new SaintsWrap<T>(_wrapType, value);
#endif
            }
        }

        #endregion

        public void AddRange(IEnumerable<T> collection)
        {
#if UNITY_EDITOR
            foreach (T v in collection)
            {
                _list.Add(v);
                _saintsList.Add(new SaintsWrap<T>(_wrapType, v));
            }
#else
            _list.AddRange(collection);
#endif
        }
    }
}
