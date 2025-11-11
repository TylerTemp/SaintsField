using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsSerialization;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public class SaintsDictionary<TKey, TValue>: IDictionary, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SaintsWrap<TKey>> _saintsKeys = new List<SaintsWrap<TKey>>();
        [SerializeField] private WrapType _wrapTypeKey;

        [SerializeField]
        private List<SaintsWrap<TValue>> _saintsValues = new List<SaintsWrap<TValue>>();
        [SerializeField] private WrapType _wrapTypeValue;

        [SerializeField]
        protected int saintsSerializedVersion;

        // prev 1, now 2

        private const int SaintsSerializedVersionRuntime = 2;

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropKeys => nameof(_saintsKeys);
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropValues => nameof(_saintsValues);
#endif

        protected int SerializedKeysCount()
        {
            return _saintsKeys.Count;
        }

        protected void SerializedKeyAdd(TKey key)
        {
            _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, key));
        }

        protected TKey SerializedKeyGetAt(int index)
        {
            return _saintsKeys[index].GetValue();
        }

        protected void SerializedKeysClear()
        {
            _saintsKeys.Clear();
        }

        protected int SerializedValuesCount()
        {
            return _saintsValues.Count;
        }

        protected void SerializedValueAdd(TValue value)
        {
            _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, value));
        }

        protected TValue SerializedValueGetAt(int index)
        {
            return _saintsValues[index].GetValue();
        }

        protected void SerializedValuesClear()
        {
            _saintsValues.Clear();
        }

        protected void SerializedSetKeyValue(TKey tKey, TValue tValue)
        {
            int index = _saintsKeys.FindIndex(wrap => wrap.GetValue().Equals(tKey));
            if (index >= 0)
            {
                // Debug.Log($"serialized set value {tKey}:{tValue}");
                _saintsValues[index].SetValue(_wrapTypeKey, tValue);
            }
            else
            {
                // Debug.Log($"serialized add value {tKey}:{tValue}");
                _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, tKey));
                _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, tValue));
            }
        }

        protected void SerializedRemoveKeyValue(TKey key)
        {
            int index = _saintsKeys.FindIndex(wrap => wrap.GetValue().Equals(key));
            if (index >= 0)
            {
                _saintsKeys.RemoveAt(index);
                _saintsValues.RemoveAt(index);
            }
        }


        protected Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                EnsureOnAfterDeserializeOnce();
                return _dictionary;
            }
            set => _dictionary = value;
        }

        private ICollection _keys;
        private ICollection _values;

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (saintsSerializedVersion == 1)
            {
                // Debug.Assert(_wrapTypeKey == WrapType.Undefined, _wrapTypeKey);
                _wrapTypeKey = RuntimeUtil.EditorWrapMigrateFrom1(_saintsKeys);
                // Debug.Assert(_wrapTypeValue == WrapType.Undefined, _wrapTypeValue);
                _wrapTypeValue = RuntimeUtil.EditorWrapMigrateFrom1(_saintsValues);
            }
#endif

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

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (saintsSerializedVersion != SaintsSerializedVersionRuntime)
            {
                saintsSerializedVersion = SaintsSerializedVersionRuntime;
            }
        }

        private bool _onAfterDeserializeOnce = false;

        private void EnsureOnAfterDeserializeOnce()
        {
            if (!_onAfterDeserializeOnce)
            {
                OnAfterDeserialize();
            }
        }

#if UNITY_EDITOR
        private HashSet<SaintsWrap<TKey>> _editorWatchedKeys = new HashSet<SaintsWrap<TKey>>();
        private HashSet<SaintsWrap<TValue>> _editorWatchedValues = new HashSet<SaintsWrap<TValue>>();
#endif
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            IEnumerable<SaintsWrap<TKey>> extraKeys = _saintsKeys.Except(_editorWatchedKeys);
            IEnumerable<SaintsWrap<TValue>> extraValues = _saintsValues.Except(_editorWatchedValues);
            foreach (SaintsWrap<TKey> keyWrap in extraKeys)
            {
                // Debug.Log($"add key listener");
                keyWrap.EditorOnAfterDeserializeChanged.AddListener(OnAfterDeserializeProcess);
                _editorWatchedKeys.Add(keyWrap);
            }

            foreach (SaintsWrap<TValue> valueWrap in extraValues)
            {
                // Debug.Log($"add value listener");
                valueWrap.EditorOnAfterDeserializeChanged.AddListener(OnAfterDeserializeProcess);
                _editorWatchedValues.Add(valueWrap);
            }
#endif

            OnAfterDeserializeProcess();
        }

        private void OnAfterDeserializeProcess()
        {
            _onAfterDeserializeOnce = true;
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
                // if(!RuntimeUtil.IsNull(key))
                // {
                //     Dictionary[key] = value;
                // }
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

        public SaintsDictionary()
        {
            saintsSerializedVersion = SaintsSerializedVersionRuntime;
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary)
        {
            saintsSerializedVersion = SaintsSerializedVersionRuntime;

            _wrapTypeKey = SaintsWrap<TKey>.GuessWrapType();
            _wrapTypeValue = SaintsWrap<TValue>.GuessWrapType();

            Dictionary = new Dictionary<TKey, TValue>(dictionary);
            foreach (KeyValuePair<TKey, TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, kv.Value));
            }
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _wrapTypeKey = SaintsWrap<TKey>.GuessWrapType();
            _wrapTypeValue = SaintsWrap<TValue>.GuessWrapType();

            Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            foreach (KeyValuePair<TKey, TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, kv.Value));
            }
        }

#if UNITY_2021_2_OR_NEWER
        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _wrapTypeKey = SaintsWrap<TKey>.GuessWrapType();
            _wrapTypeValue = SaintsWrap<TValue>.GuessWrapType();

            Dictionary = new Dictionary<TKey, TValue>(collection);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, kv.Value));
            }
        }

        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer)
        {
            _wrapTypeKey = SaintsWrap<TKey>.GuessWrapType();
            _wrapTypeValue = SaintsWrap<TValue>.GuessWrapType();

            Dictionary = new Dictionary<TKey, TValue>(collection, comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, kv.Value));
            }
        }
#endif

        public SaintsDictionary(IEqualityComparer<TKey> comparer)
        {
            _wrapTypeKey = SaintsWrap<TKey>.GuessWrapType();
            _wrapTypeValue = SaintsWrap<TValue>.GuessWrapType();

            Dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(_wrapTypeKey, kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(_wrapTypeValue, kv.Value));
            }
        }

        #region Convert

        // Implicit conversion operator: Converts SaintsDictionary<,> to Dictionary<,>
        public static implicit operator Dictionary<TKey, TValue>(SaintsDictionary<TKey, TValue> saintsDict) => saintsDict.Dictionary;

        // Explicit conversion operator: Converts T[] to SaintsArray<T>
        public static explicit operator SaintsDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict) => new SaintsDictionary<TKey, TValue>(dict);

        #endregion
    }
}
