using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class SaintsHashSet<T>:
        // ICollection<T>,  --> ISet<T>
        // IEnumerable<T>,  --> ISet<T>
        // IEnumerable,  --> ISet<T>
        ISerializable,
        IDeserializationCallback,
        ISet<T>,
        IReadOnlyCollection<T>,

        ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SaintsWrap<T>> _saintsList = new List<SaintsWrap<T>>();
        [SerializeField] private int _saintsSerializedVersion;
        private const int SaintsSerializedVersionRuntime = 2;
        [SerializeField] private WrapType _wrapType;

        private HashSet<T> _hashSet;

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropertyName => nameof(_saintsList);
#endif

        #region Editor Interface

        protected int SerializedCount() => _saintsList.Count;

        protected void SerializedAdd(T key)
        {
            _saintsList.Add(new SaintsWrap<T>(_wrapType, key));
        }

        protected bool SerializedRemove(T key)
        {
            int index = _saintsList.FindIndex(each => EqualityComparer<T>.Default.Equals(each.GetValue(), key));
            if (index >= 0)
            {
                _saintsList.RemoveAt(index);
                return true;
            }

            return false;
        }

        protected T SerializedGetAt(int index) => _saintsList[index].GetValue();

        protected void SerializedClear() => _saintsList.Clear();
        #endregion

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if (_saintsSerializedVersion == 1)
            {
                _wrapType = RuntimeUtil.EditorWrapMigrateFrom1(_saintsList);
            }
#endif

#if UNITY_EDITOR
            // do nothing
#else
            SerializedClear();
            foreach (T value in _hashSet)
            {
                SerializedAdd(value);
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

        protected virtual void OnAfterDeserializeProcess()
        {
            _hashSet.Clear();

            int serCount = SerializedCount();
            for (int index = 0; index < serCount; index++)
            {
                T value = SerializedGetAt(index);
                _hashSet.Add(value);
            }

#if UNITY_EDITOR
            // do nothing
#else
            SerializedClear();
#endif
        }

        #region Constructor

        public SaintsHashSet()
        {
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
            _wrapType = SaintsWrap<T>.GuessWrapType();
            _hashSet = new HashSet<T>();
        }

#if UNITY_2021_2_OR_NEWER
        public SaintsHashSet(int capacity): this()
        {
            _hashSet = new HashSet<T>(capacity);
        }
#endif

        public SaintsHashSet(IEqualityComparer<T> comparer): this()
        {
            _hashSet = new HashSet<T>(comparer);
        }

        public SaintsHashSet(IEnumerable<T> collection): this()
        {
            _hashSet = new HashSet<T>(collection);
        }

        public SaintsHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer): this()
        {
            switch (collection)
            {
                case null:
                    throw new ArgumentNullException(nameof(collection));
                case SaintsHashSet<T> saintsSet:
                    _hashSet = new HashSet<T>(saintsSet._hashSet, comparer);
                    return;
                case HashSet<T> objSet:
                    _hashSet = new HashSet<T>(objSet, comparer);
                    return;
                case ICollection<T> objs:
                    _hashSet = new HashSet<T>(objs, comparer);
                    return;
                default:
                    _hashSet = new HashSet<T>(comparer);
                    _hashSet.UnionWith(collection);
                    return;
            }
        }

        // protected HashSet(SerializationInfo info, StreamingContext context) => HashSet.m_siInfo = info;
#if UNITY_2021_2_OR_NEWER
        public SaintsHashSet(int capacity, IEqualityComparer<T> comparer): this()
        {
            _hashSet = new HashSet<T>(capacity, comparer);
        }
#endif

        #endregion

        #region HashSet Interface

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_hashSet).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                _hashSet.Remove(v);
                SerializedRemove(v);
            }
#else
            _hashSet.ExceptWith(other);
#endif
        }

        public void IntersectWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                if (_hashSet.Contains(v))
                {
                    continue;
                }

                _hashSet.Remove(v);
                while (SerializedRemove(v))
                {
                    // keep removing until not found
                }
            }
#else
            _hashSet.IntersectWith(other);
#endif
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _hashSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _hashSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _hashSet.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                if (_hashSet.Add(v))
                {
                    SerializedAdd(v);
                }
                else
                {
                    _hashSet.Remove(v);
                    while (SerializedRemove(v))
                    {
                        // keep removing until not found
                    }
                }
            }
#else
            _hashSet.SymmetricExceptWith(other);
#endif
        }

        public void UnionWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                if (_hashSet.Add(v))
                {
                    SerializedAdd(v);
                }
            }
#else
            _hashSet.UnionWith(other);
#endif
        }

        bool ISet<T>.Add(T item)
        {
            return Add(item);
        }

        public bool Add(T item)
        {
#if UNITY_EDITOR
            if(_hashSet.Add(item))
            {
                SerializedAdd(item);
                return true;
            }

            return false;
#else
            return _hashSet.Add(item);
#endif
        }

        public void Clear()
        {
            _hashSet.Clear();
#if UNITY_EDITOR
            SerializedClear();
#endif
        }

        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _hashSet.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if (_hashSet.Remove(item))
            {
                while (SerializedRemove(item))
                {
                    // keep removing until not found
                }
                return true;
            }

            return false;
#else
            return _hashSet.Remove(item);
#endif
        }

        int ICollection<T>.Count => _hashSet.Count;

        public bool IsReadOnly => false;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _hashSet.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            _hashSet.OnDeserialization(sender);
        }

        int IReadOnlyCollection<T>.Count => _hashSet.Count;
        public int Count => _hashSet.Count;
        #endregion
    }
}
