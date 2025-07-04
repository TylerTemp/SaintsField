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
        protected abstract int SerializedKeysCount();
        protected abstract void SerializedKeyAdd(TKey key);
        protected abstract TKey SerializedKeyGetAt(int index);
        protected abstract void SerializedKeysClear();
        protected abstract int SerializedValuesCount();
        protected abstract void SerializedValueAdd(TValue value);
        protected abstract TValue SerializedValueGetAt(int index);
        protected abstract void SerializedValuesClear();

        protected abstract void SerializedSetKeyValue(TKey tKey, TValue tValue);
        protected abstract void SerializedRemoveKeyValue(TKey key);

        protected Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();
        private ICollection _keys;
        private ICollection _values;

        public void OnBeforeSerialize()
        {
            OnBeforeSerializeProcesser();
        }

        protected virtual void OnBeforeSerializeProcesser()
        {
#if UNITY_EDITOR
            int keyCount = SerializedKeysCount();
            int valueCount = SerializedValuesCount();
            if (keyCount < valueCount)
            {
                int addCount = valueCount - keyCount;
                for (int i = 0; i < addCount; i++)
                {
                    SerializedKeyAdd(default);
                }
                // Debug.Log($"Balance add {addCount} to keys");
            }
            else if (keyCount > valueCount)
            {
                int addCount = keyCount - valueCount;
                for (int i = 0; i < addCount; i++)
                {
                    SerializedValueAdd(default);
                }
                // Debug.Log($"Balance add {addCount} to values");
            }
#else
            SerializedKeysClear();
            SerializedValuesClear();
            foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
            {
                SerializedKeyAdd(kvp.Key);
                SerializedValueAdd(kvp.Value);
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            OnAfterDeserializeProcess();
        }

        protected virtual void OnAfterDeserializeProcess()
        {
            Dictionary.Clear();

            int keyCount = SerializedKeysCount();
            for (int index = 0; index < keyCount; index++)
            {
                TKey key = SerializedKeyGetAt(index);
                TValue value = SerializedValuesCount() > index ? SerializedValueGetAt(index) : default;
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
            SerializedKeysClear();
            SerializedValuesClear();
#endif
        }

        public void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
#if UNITY_EDITOR
            SerializedKeyAdd(key);
            SerializedValueAdd(value);
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
                SerializedRemoveKeyValue(tKey);
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
            SerializedKeyAdd(item.Key);
            SerializedValueAdd(item.Value);
#endif
            Dictionary.Add(item.Key, item.Value);
        }

        public void Add(object key, object value)
        {
            if (key is TKey tKey && value is TValue tValue)
            {
                Add(new KeyValuePair<TKey, TValue>(tKey, tValue));
            }

            throw new NotSupportedException($"Unsupported key type {key.GetType()} and value type {value?.GetType()}");
        }

        public void Clear()
        {
#if UNITY_EDITOR
            SerializedKeysClear();
            SerializedValuesClear();
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
                SerializedRemoveKeyValue(item.Key);
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
                SerializedSetKeyValue(tKey, tValue);
#endif
            }
        }

        #endregion

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        #region Dictionary Extension


        public TValue GetValueOrDefault(TKey key)
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            return default;
        }

        public TValue GetValueOrDefault(TKey key, TValue defaultValue)
        {
            return TryGetValue(key, out TValue value)
                ? value
                : defaultValue;
        }

        public bool Remove(TKey key)
        {
            if (Dictionary.Remove(key))
            {
#if UNITY_EDITOR
                SerializedRemoveKeyValue(key);
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
            SerializedKeyAdd(key);
            SerializedValueAdd(value);
#endif
            return true;
        }

        public TValue GetOrAddNonNull(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            TValue createdValue = valueFactory.Invoke(key);
            Add(key, createdValue);
            return createdValue;
        }

        public TValue GetValueOrAdd(
            TKey key,
            Func<TKey, TValue> valueProvider)
        {
            if(TryGetValue(key, out TValue value))
            {
                return value;
            }

            TValue createdValue = valueProvider.Invoke(key);
            Add(key, createdValue);
            return createdValue;
        }

        // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Global
        public bool TryRemove(TKey key, out TValue value)
        {
            if(TryGetValue(key, out TValue foundValue))
            {
                value = foundValue;
                return Remove(key);
            }
            value = default;
            return false;
        }
        #endregion

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                Dictionary[key] = value;
#if UNITY_EDITOR
                SerializedSetKeyValue(key, value);
#endif
            }
        }
    }
}
