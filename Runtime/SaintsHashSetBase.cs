using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SaintsField
{
    public abstract class SaintsHashSetBase<T>:
        // ICollection<T>,  --> ISet<T>
        // IEnumerable<T>,  --> ISet<T>
        // IEnumerable,  --> ISet<T>
        ISerializable,
        IDeserializationCallback,
        ISet<T>,
        IReadOnlyCollection<T>,

        ISerializationCallbackReceiver
    {
        protected abstract int SerializedCount();
        protected abstract void SerializedAdd(T key);
        protected abstract bool SerializedRemove(T key);
        protected abstract T SerializedGetAt(int index);
        protected abstract void SerializedClear();

        protected HashSet<T> HashSet = new HashSet<T>();

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // do nothing
#else
            SerializedClear();
            foreach (T value in HashSet)
            {
                SerializedAdd(value);
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            HashSet.Clear();

            int serCount = SerializedCount();
            for (int index = 0; index < serCount; index++)
            {
                T value = SerializedGetAt(index);
                HashSet.Add(value);
            }

#if UNITY_EDITOR
            // do nothing
#else
            SerializedClear();
#endif
        }

        #region HashSet Interface

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)HashSet).GetEnumerator();

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
                HashSet.Remove(v);
                SerializedRemove(v);
            }
#else
            HashSet.ExceptWith(other);
#endif
        }

        public void IntersectWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                if (HashSet.Contains(v))
                {
                    continue;
                }

                HashSet.Remove(v);
                while (SerializedRemove(v))
                {
                    // keep removing until not found
                }
            }
#else
            HashSet.IntersectWith(other);
#endif
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return HashSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return HashSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return HashSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return HashSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return HashSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return HashSet.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                if (HashSet.Add(v))
                {
                    SerializedAdd(v);
                }
                else
                {
                    HashSet.Remove(v);
                    while (SerializedRemove(v))
                    {
                        // keep removing until not found
                    }
                }
            }
#else
            HashSet.SymmetricExceptWith(other);
#endif
        }

        public void UnionWith(IEnumerable<T> other)
        {
#if UNITY_EDITOR
            foreach (T v in other)
            {
                if (HashSet.Add(v))
                {
                    SerializedAdd(v);
                }
            }
#else
            HashSet.UnionWith(other);
#endif
        }

        bool ISet<T>.Add(T item)
        {
            return Add(item);
        }

        public bool Add(T item)
        {
#if UNITY_EDITOR
            if(HashSet.Add(item))
            {
                SerializedAdd(item);
                return true;
            }

            return false;
#else
            return HashSet.Add(item);
#endif
        }

        public void Clear()
        {
            HashSet.Clear();
#if UNITY_EDITOR
            SerializedClear();
#endif
        }

        public bool Contains(T item)
        {
            return HashSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            HashSet.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if (HashSet.Remove(item))
            {
                while (SerializedRemove(item))
                {
                    // keep removing until not found
                }
                return true;
            }

            return false;
#else
            return HashSet.Remove(item);
#endif
        }

        int ICollection<T>.Count => HashSet.Count;

        public bool IsReadOnly => false;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            HashSet.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            HashSet.OnDeserialization(sender);
        }

        int IReadOnlyCollection<T>.Count => HashSet.Count;
        public int Count => HashSet.Count;
        #endregion
    }
}
