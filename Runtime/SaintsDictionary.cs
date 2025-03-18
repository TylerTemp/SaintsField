using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class SaintsDictionary<TKey, TValue>: SaintsDictionaryBase<TKey, TValue>
    {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField]
        private List<TValue> _values = new List<TValue>();

#if UNITY_EDITOR
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropKeys => nameof(_keys);
        // ReSharper disable once UnusedMember.Local
        private static string EditorPropValues => nameof(_values);
#endif
        protected override List<TKey> SerializedKeys => _keys;
        protected override List<TValue> SerializedValues => _values;

        public SaintsDictionary()
        {
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _keys.Add(kv.Key);
                _values.Add(kv.Value);
            }
        }

        public SaintsDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _keys.Add(kv.Key);
                _values.Add(kv.Value);
            }
        }

#if UNITY_2021_2_OR_NEWER
        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            Dictionary = new Dictionary<TKey, TValue>(collection);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _keys.Add(kv.Key);
                _values.Add(kv.Value);
            }
        }

        public SaintsDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(collection, comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _keys.Add(kv.Key);
                _values.Add(kv.Value);
            }
        }
#endif
        public SaintsDictionary(IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (KeyValuePair<TKey,TValue> kv in Dictionary)
            {
                _keys.Add(kv.Key);
                _values.Add(kv.Value);
            }
        }
    }
}
