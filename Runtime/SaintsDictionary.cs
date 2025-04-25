using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsDictionary<TKey, TValue>: SaintsDictionaryBase<TKey, TValue>
    {
        [SerializeField, Obsolete]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField, Obsolete]
        private List<TValue> _values = new List<TValue>();

        [SerializeField]
        private List<Wrap<TKey>> _saintsKeys = new List<Wrap<TKey>>();

        [SerializeField]
        private List<Wrap<TValue>> _saintsValues = new List<Wrap<TValue>>();

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropKeys => nameof(_saintsKeys);
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropValues => nameof(_saintsValues);
#endif
        protected override List<Wrap<TKey>> SerializedKeys => _saintsKeys;
        protected override List<Wrap<TValue>> SerializedValues => _saintsValues;

        public SaintsDictionary()
        {
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary);
            foreach (KeyValuePair<TKey, TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new Wrap<TKey>(kv.Key));
                _saintsValues.Add(new Wrap<TValue>(kv.Value));
            }
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            foreach (KeyValuePair<TKey, TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new Wrap<TKey>(kv.Key));
                _saintsValues.Add(new Wrap<TValue>(kv.Value));
            }
        }

#if UNITY_2021_2_OR_NEWER
        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            Dictionary = new Dictionary<TKey, TValue>(collection);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new Wrap<TKey>(kv.Key));
                _saintsValues.Add(new Wrap<TValue>(kv.Value));
            }
        }

        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(collection, comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new Wrap<TKey>(kv.Key));
                _saintsValues.Add(new Wrap<TValue>(kv.Value));
            }
        }
#endif

        public SaintsDictionary(IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _saintsKeys.Add(new Wrap<TKey>(kv.Key));
                _saintsValues.Add(new Wrap<TValue>(kv.Value));
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
                    _saintsKeys.Add(new Wrap<TKey>(key));
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
                    _saintsValues.Add(new Wrap<TValue>(value));
                }
                _values.Clear();
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }
#endif

#if UNITY_EDITOR
        protected override void OnBeforeSerializeProcesser()
        {
            MigrateKv();
            base.OnBeforeSerializeProcesser();
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
