using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SaintsField.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class SaintsList<T>: IList<T>, IReadOnlyList<T>, IList, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("value")]
        [SerializeField, Obsolete]
        public List<T> obsoleteValue = new List<T>();

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
                foreach (T oldValue in obsoleteValue)
#pragma warning restore CS0612 // Type or member is obsolete
                {
                    _saintsList.Add(new SaintsWrap<T>(_wrapType, oldValue));
                }

                _saintsSerializedVersion = SaintsSerializedVersionRuntime;
                // ReSharper disable once RedundantJumpStatement
                return;
            }
#endif

// #if UNITY_EDITOR
//             // do nothing
// #else
//             _saintsList.Clear();
//             foreach (T v in _list)
//             {
//                 _saintsList.Add(new SaintsWrap<T>(_wrapType, v));
//             }
// #endif
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
        }

#if UNITY_EDITOR
        private HashSet<SaintsWrap<T>> _editorWatchedKeys = new HashSet<SaintsWrap<T>>();
        private UnityEvent _editorOnAfterDeserializeChanged = new UnityEvent();

        public UnityEvent EditorOnAfterDeserializeChanged =>
            _editorOnAfterDeserializeChanged ??= new UnityEvent();
#endif
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (_saintsSerializedVersion == 0)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                _list = obsoleteValue;
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
            EditorOnAfterDeserializeChanged.Invoke();
#else
            _saintsList.Clear();
#endif
        }

        #endregion

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        public static string EditorPropertyName => nameof(_saintsList);
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

        public int Capacity
        {
            get => _list.Capacity;
            set => _list.Capacity = value;
        }

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator List<T>(SaintsList<T> saintsList)
        {
            return saintsList._list;
        }

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static implicit operator SaintsList<T>(List<T> lis)
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

        bool IList.IsFixedSize => ((IList)_list).IsFixedSize;
        bool IList.IsReadOnly => ((IList)_list).IsReadOnly;
        bool ICollection.IsSynchronized => ((ICollection)_list).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;

        object IList.this[int index]
        {
            get => ((IList)_list)[index];
            set
            {
                ((IList)_list)[index] = value;
#if UNITY_EDITOR
                SyncSerializedList();
#endif
            }
        }

        int IList.Add(object value)
        {
            int index = ((IList)_list).Add(value);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
            return index;
        }

        bool IList.Contains(object value) => ((IList)_list).Contains(value);

        int IList.IndexOf(object value) => ((IList)_list).IndexOf(value);

        void IList.Insert(int index, object value)
        {
            ((IList)_list).Insert(index, value);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        void IList.Remove(object value)
        {
#if UNITY_EDITOR
            int count = _list.Count;
#endif
            ((IList)_list).Remove(value);
#if UNITY_EDITOR
            if (_list.Count != count)
            {
                SyncSerializedList();
            }
#endif
        }

        void ICollection.CopyTo(Array array, int index) => ((ICollection)_list).CopyTo(array, index);

        #endregion

        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public ReadOnlyCollection<T> AsReadOnly() => _list.AsReadOnly();

        public int BinarySearch(T item) => _list.BinarySearch(item);
        public int BinarySearch(T item, IComparer<T> comparer) => _list.BinarySearch(item, comparer);
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) =>
            _list.BinarySearch(index, count, item, comparer);

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => _list.ConvertAll(converter);

        public void CopyTo(T[] array) => _list.CopyTo(array);
        public void CopyTo(int index, T[] array, int arrayIndex, int count) =>
            _list.CopyTo(index, array, arrayIndex, count);

        public bool Exists(Predicate<T> match) => _list.Exists(match);
        public T Find(Predicate<T> match) => _list.Find(match);
        public List<T> FindAll(Predicate<T> match) => _list.FindAll(match);
        public int FindIndex(Predicate<T> match) => _list.FindIndex(match);
        public int FindIndex(int startIndex, Predicate<T> match) => _list.FindIndex(startIndex, match);
        public int FindIndex(int startIndex, int count, Predicate<T> match) =>
            _list.FindIndex(startIndex, count, match);
        public T FindLast(Predicate<T> match) => _list.FindLast(match);
        public int FindLastIndex(Predicate<T> match) => _list.FindLastIndex(match);
        public int FindLastIndex(int startIndex, Predicate<T> match) => _list.FindLastIndex(startIndex, match);
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) =>
            _list.FindLastIndex(startIndex, count, match);
        public void ForEach(Action<T> action) => _list.ForEach(action);
        public List<T> GetRange(int index, int count) => _list.GetRange(index, count);
        public int IndexOf(T item, int index) => _list.IndexOf(item, index);
        public int IndexOf(T item, int index, int count) => _list.IndexOf(item, index, count);

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public int LastIndexOf(T item) => _list.LastIndexOf(item);
        public int LastIndexOf(T item, int index) => _list.LastIndexOf(item, index);
        public int LastIndexOf(T item, int index, int count) => _list.LastIndexOf(item, index, count);

        public int RemoveAll(Predicate<T> match)
        {
            int removed = _list.RemoveAll(match);
#if UNITY_EDITOR
            if (removed != 0)
            {
                SyncSerializedList();
            }
#endif
            return removed;
        }

        public void RemoveRange(int index, int count)
        {
            _list.RemoveRange(index, count);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public void Reverse()
        {
            _list.Reverse();
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public void Reverse(int index, int count)
        {
            _list.Reverse(index, count);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public void Sort()
        {
            _list.Sort();
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public void Sort(IComparer<T> comparer)
        {
            _list.Sort(comparer);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public void Sort(Comparison<T> comparison)
        {
            _list.Sort(comparison);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _list.Sort(index, count, comparer);
#if UNITY_EDITOR
            SyncSerializedList();
#endif
        }

        public T[] ToArray() => _list.ToArray();
        public void TrimExcess() => _list.TrimExcess();
        public bool TrueForAll(Predicate<T> match) => _list.TrueForAll(match);

#if UNITY_EDITOR
        private void SyncSerializedList()
        {
            _saintsList.Clear();
            foreach (T item in _list)
            {
                _saintsList.Add(new SaintsWrap<T>(_wrapType, item));
            }
        }
#endif
    }
}
