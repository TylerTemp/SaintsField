using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public abstract class SaintsDictionaryBase<TKey, TValue>: IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        protected abstract List<TKey> SerializedKeys { get; }
        protected abstract List<TValue> SerializedValues { get; }

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        // [Conditional("UNITY_EDITOR")]
        // private void EditorSyncDictionaryToBackingField()
        // {
        //     foreach (KeyValuePair<TKey, TValue> kvp in _dictionary)
        //     {
        //         SerializedKeys.Add(kvp.Key);
        //         SerializedValues.Add(kvp.Value);
        //     }
        // }

        public void OnBeforeSerialize()
        {
            // Debug.Log($"OnBeforeSerialize keys={SerializedKeys.Count}, values={SerializedValues.Count}");
#if UNITY_EDITOR
            // if(SerializedKeys.Count != _dictionary.Count)
            // {
            //     EditorSyncDictionaryToBackingField();
            // }

            int keyCount = SerializedKeys.Count;
            int valueCount = SerializedValues.Count;
            if (keyCount < valueCount)
            {
                int addCount = valueCount - keyCount;
                foreach (int _ in Enumerable.Range(0, addCount))
                {
                    SerializedKeys.Add(default);
                }
                // Debug.Log($"Balance add {addCount} to keys");
            }
            else if (keyCount > valueCount)
            {
                int addCount = keyCount - valueCount;
                foreach (int _ in Enumerable.Range(0, addCount))
                {
                    SerializedValues.Add(default);
                }
                // Debug.Log($"Balance add {addCount} to values");
            }
#else
            SerializedKeys.Clear();
            SerializedValues.Clear();
            foreach (KeyValuePair<TKey, TValue> kvp in _dictionary)
            {
                SerializedKeys.Add(kvp.Key);
                SerializedValues.Add(kvp.Value);
            }
#endif

        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();

            // Debug.Log($"OnAfterDeserialize keys={SerializedKeys.Count}, values={SerializedValues.Count}");
            // int keyCount = SerializedKeys.Count;
            // int valueCount = SerializedValues.Count;
            // if (SerializedKeys.Count != SerializedValues.Count)
            // {
            //     int useCount = Math.Max(SerializedKeys.Count, SerializedValues.Count);
            //
            // }

            foreach (int index in Enumerable.Range(0, SerializedKeys.Count))
            {
                TKey key = SerializedKeys[index];
                TValue value = SerializedValues.Count > index ? SerializedValues[index] : default;
#if UNITY_EDITOR
                // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
                if (!RuntimeUtil.IsNull(key) && !_dictionary.ContainsKey(key))
                {
                    _dictionary.Add(key, value);
                }
#else
                _dictionary.Add(key, value);
#endif
                // _dictionary[_keys[index]] = _values[index];
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
            _dictionary.Add(key, value);
#if UNITY_EDITOR
            SerializedKeys.Add(key);
            SerializedValues.Add(value);
#endif
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
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
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
#if UNITY_EDITOR
            SerializedKeys.Clear();
            SerializedValues.Clear();
#endif
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool result = _dictionary.Remove(item.Key);
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

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
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
            if (_dictionary.ContainsKey(key))
            {
                return false;
            }

            _dictionary.Add(key, value);
#if UNITY_EDITOR
            SerializedKeys.Add(key);
            SerializedValues.Add(value);
#endif
            return true;
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value;
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
