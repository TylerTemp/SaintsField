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
        private static string EditorPropKeys => nameof(_keys);
        private static string EditorPropValues => nameof(_values);
#endif
        protected override List<TKey> SerializedKeys => _keys;
        protected override List<TValue> SerializedValues => _values;
    }
}
