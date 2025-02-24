using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictFillerExample : SaintsMonoBehaviour
    {
        [Serializable]
        public class ValueFillerDict : SaintsDictionaryBase<int, GameObject>
        {
            [SerializeField, NoLabel]
            private List<int> _intKeys = new List<int>();

            [SerializeField, NoLabel]
            // [GetComponentInChildren]
            private List<GameObject> _objValues = new List<GameObject>();

#if UNITY_EDITOR
            private static string EditorPropKeys => nameof(_intKeys);
            private static string EditorPropValues => nameof(_objValues);
#endif
            protected override List<int> SerializedKeys => _intKeys;
            protected override List<GameObject> SerializedValues => _objValues;
        }

        public ValueFillerDict valueFillerDict;
        [SaintsDictionary("键", "值", numberOfItemsPerPage: 2)]
        public ValueFillerDict decValueFillerDict;

        [LayoutStart("Buttons", ELayout.Horizontal)]

        [Button]
        private void AddRandom()
        {
            int[] keys = Enumerable.Range(0, 100).Except(valueFillerDict.Keys).ToArray();
            if (keys.Length == 0)
            {
                return;
            }

            int key = keys[UnityEngine.Random.Range(0, keys.Length)];
            valueFillerDict.Add(key, gameObject);
        }

        [Button]
        private void DeleteRandom()
        {
            int[] keys = valueFillerDict.Keys.ToArray();
            if (keys.Length == 0)
            {
                return;
            }
            valueFillerDict.Remove(keys[UnityEngine.Random.Range(0, keys.Length)]);
        }
    }
}
