using System;
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsDictionary<TKey, TValue>: SaintsDictionaryBase<TKey, TValue>
    {
        [Serializable]
        public class SaintsWrap<T> : BaseWrap<T>
        {
            [SerializeField] public T value;
            public override T Value { get => value; set => this.value = value; }

#if UNITY_EDITOR
            // ReSharper disable once StaticMemberInGenericType
            public static readonly string EditorPropertyName = nameof(value);
#endif

            public SaintsWrap(T v)
            {
                value = v;
            }
        }

        [SerializeField, Obsolete]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField, Obsolete]
        private List<TValue> _values = new List<TValue>();

        [SerializeField]
        private List<SaintsWrap<TKey>> _saintsKeys = new List<SaintsWrap<TKey>>();

        [SerializeField]
        private List<SaintsWrap<TValue>> _saintsValues = new List<SaintsWrap<TValue>>();


        protected override int SerializedKeysCount()
        {
            return _saintsKeys.Count;
        }

        protected override void SerializedKeyAdd(TKey key)
        {
            _saintsKeys.Add(new SaintsWrap<TKey>(key));
        }

        protected override TKey SerializedKeyGetAt(int index)
        {
            return _saintsKeys[index].value;
        }

        protected override void SerializedKeysClear()
        {
            _saintsKeys.Clear();
        }

        protected override int SerializedValuesCount()
        {
            return _saintsValues.Count;
        }

        protected override void SerializedValueAdd(TValue value)
        {
            _saintsValues.Add(new SaintsWrap<TValue>(value));
        }

        protected override TValue SerializedValueGetAt(int index)
        {
            return _saintsValues[index].value;
        }

        protected override void SerializedValuesClear()
        {
            _saintsValues.Clear();
        }

        protected override void SerializedSetKeyValue(TKey tKey, TValue tValue)
        {
            int index = _saintsKeys.FindIndex(wrap => wrap.value.Equals(tKey));
            if (index >= 0)
            {
                _saintsValues[index].value = tValue;
            }
            else
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(tKey));
                _saintsValues.Add(new SaintsWrap<TValue>(tValue));
            }
        }

        protected override void SerializedRemoveKeyValue(TKey key)
        {
            int index = _saintsKeys.FindIndex(wrap => wrap.value.Equals(key));
            if (index >= 0)
            {
                _saintsKeys.RemoveAt(index);
                _saintsValues.RemoveAt(index);
            }
        }

        protected override void OnBeforeSerializeProcesser()
        {
#if UNITY_EDITOR
            MigrateKv();
#endif
            base.OnBeforeSerializeProcesser();
        }

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropKeys => nameof(_saintsKeys);
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropValues => nameof(_saintsValues);
#endif

        public SaintsDictionary()
        {
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary);
            foreach (KeyValuePair<TKey, TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(kv.Value));
            }
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            foreach (KeyValuePair<TKey, TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(kv.Value));
            }
        }

#if UNITY_2021_2_OR_NEWER
        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            Dictionary = new Dictionary<TKey, TValue>(collection);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(kv.Value));
            }
        }

        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(collection, comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(kv.Value));
            }
        }
#endif

        public SaintsDictionary(IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new SaintsWrap<TKey>(kv.Key));
                _saintsValues.Add(new SaintsWrap<TValue>(kv.Value));
            }
        }

#if UNITY_EDITOR
        private void MigrateKv()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (_saintsKeys.Count == 0 && _keys.Count != 0)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"saints dictionary migrate keys {_keys.Count}");
#endif
                foreach (TKey key in _keys)
                {
                    _saintsKeys.Add(new SaintsWrap<TKey>(key));
                }

                _keys.Clear();
            }

            if (_saintsValues.Count == 0 && _values.Count != 0)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"saints dictionary migrate values {_values.Count}");
#endif
                foreach (TValue value in _values)
                {
                    _saintsValues.Add(new SaintsWrap<TValue>(value));
                }
                _values.Clear();
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }
#endif


#if UNITY_EDITOR
        protected override void OnAfterDeserializeProcess()
        {
            MigrateKv();
            base.OnAfterDeserializeProcess();
        }
#endif
    }
}
