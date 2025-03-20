using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public abstract class SaintsDictionaryBase<TKey, TValue>: IDictionary, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        protected abstract List<TKey> SerializedKeys { get; }
        protected abstract List<TValue> SerializedValues { get; }

        protected Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();
        private ICollection _keys;
        private ICollection _values;

        // [Conditional("UNITY_EDITOR")]
        // private void EditorSyncDictionaryToBackingField()
        // {
        //     foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
        //     {
        //         SerializedKeys.Add(kvp.Key);
        //         SerializedValues.Add(kvp.Value);
        //     }
        // }

        public void OnBeforeSerialize()
        {
            // Debug.Log($"OnBeforeSerialize keys={SerializedKeys.Count}, values={SerializedValues.Count}");
#if UNITY_EDITOR
            // if(SerializedKeys.Count != Dictionary.Count)
            // {
            //     EditorSyncDictionaryToBackingField();
            // }

            int keyCount = SerializedKeys.Count;
            int valueCount = SerializedValues.Count;
            if (keyCount < valueCount)
            {
                int addCount = valueCount - keyCount;
                for (int i = 0; i < addCount; i++)
                {
                    SerializedKeys.Add(default);
                }
                // Debug.Log($"Balance add {addCount} to keys");
            }
            else if (keyCount > valueCount)
            {
                int addCount = keyCount - valueCount;
                for (int i = 0; i < addCount; i++)
                {
                    SerializedValues.Add(default);
                }
                // Debug.Log($"Balance add {addCount} to values");
            }
#else
            SerializedKeys.Clear();
            SerializedValues.Clear();
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
            {
                SerializedKeys.Add(kvp.Key);
                SerializedValues.Add(kvp.Value);
            }
#endif

        }

        public void OnAfterDeserialize()
        {
            Dictionary.Clear();

            // Debug.Log($"OnAfterDeserialize keys={SerializedKeys.Count}, values={SerializedValues.Count}");
            // int keyCount = SerializedKeys.Count;
            // int valueCount = SerializedValues.Count;
            // if (SerializedKeys.Count != SerializedValues.Count)
            // {
            //     int useCount = Math.Max(SerializedKeys.Count, SerializedValues.Count);
            //
            // }

            for (var index = 0; index < SerializedKeys.Count; index++)
            {
                TKey key = SerializedKeys[index];
                TValue value = SerializedValues.Count > index ? SerializedValues[index] : default;
#if UNITY_EDITOR
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
                if (!RuntimeUtil.IsNull(key) && !Dictionary.ContainsKey(key))
                {
                    Dictionary.Add(key, value);
                }
#else
                Dictionary.Add(key, value);
#endif
                // Dictionary[_keys[index]] = _values[index];
            }

#if UNITY_EDITOR
            // do nothing
#else
            SerializedKeys.Clear();
            SerializedValues.Clear();
#endif
        }

        public void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
#if UNITY_EDITOR
            SerializedKeys.Add(key);
            SerializedValues.Add(value);
#endif
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public ICollection<TKey> Keys => Dictionary.Keys;

        ICollection IDictionary.Values => Dictionary.Values;

        ICollection IDictionary.Keys => Dictionary.Keys;

        public ICollection<TValue> Values => Dictionary.Values;

        public bool Contains(object key)
        {
            return Dictionary.ContainsKey((TKey) key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public void Remove(object key)
        {
            TKey tKey = (TKey)key;

            bool removed = Dictionary.Remove(tKey);
#if UNITY_EDITOR
            if(removed)
            {
                int keyIndex = SerializedKeys.IndexOf(tKey);
                SerializedValues.RemoveAt(keyIndex);
                SerializedKeys.RemoveAt(keyIndex);
            }
#endif
        }

        public virtual bool IsFixedSize => false;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
#if UNITY_EDITOR
            SerializedKeys.Add(item.Key);
            SerializedValues.Add(item.Value);
#endif
            Dictionary.Add(item.Key, item.Value);
        }

        public void Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
#if UNITY_EDITOR
            SerializedKeys.Clear();
            SerializedValues.Clear();
#endif
            Dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = Dictionary.Remove(item.Key);
#if UNITY_EDITOR
            if(result)
            {
                int keyIndex = SerializedKeys.IndexOf(item.Key);
                SerializedValues.RemoveAt(keyIndex);
                SerializedKeys.RemoveAt(keyIndex);
            }
#endif
            return result;
        }

        #region IDictionary
        public void CopyTo(Array array, int arrayIndex)
        {
            KeyValuePair<object, object>[] keyValuePairs = new KeyValuePair<object, object>[Dictionary.Count];
            int index = 0;
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                keyValuePairs[index] = new KeyValuePair<object, object>(kv.Key, kv.Value);
                index++;
            }

            keyValuePairs.CopyTo(array, arrayIndex);
        }

        public int Count => Dictionary.Count;
        public bool IsSynchronized => false;

        protected readonly object SyncRootObj = new object();

        public virtual object SyncRoot => SyncRootObj;
        public virtual bool IsReadOnly => false;

        public object this[object key]
        {
            get => Dictionary[(TKey)key];
            set
            {
                TKey tKey = (TKey)key;
                TValue tValue = (TValue)value;
                Dictionary[tKey] = tValue;
#if UNITY_EDITOR
                int index = SerializedKeys.IndexOf(tKey);
                if (index >= 0)
                {
                    SerializedValues[index] = tValue;
                }
                else
                {
                    SerializedKeys.Add(tKey);
                    SerializedValues.Add(tValue);
                }
#endif
            }
        }

        #endregion

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (Dictionary.Remove(key))
            {
#if UNITY_EDITOR
                int index = SerializedKeys.IndexOf(key);
                if (index >= 0)
                {
                    SerializedValues.RemoveAt(index);
                    SerializedKeys.RemoveAt(index);
                }
#endif
                return true;
            }
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
            if (Dictionary.ContainsKey(key))
            {
                return false;
            }

            Dictionary.Add(key, value);
#if UNITY_EDITOR
            SerializedKeys.Add(key);
            SerializedValues.Add(value);
#endif
            return true;
        }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                Dictionary[key] = value;
#if UNITY_EDITOR
                int index = SerializedKeys.IndexOf(key);
                if (index >= 0)
                {
                    SerializedValues[index] = value;
                }
                else
                {
                    SerializedKeys.Add(key);
                    SerializedValues.Add(value);
                }
#endif
            }
        }
    }
}
