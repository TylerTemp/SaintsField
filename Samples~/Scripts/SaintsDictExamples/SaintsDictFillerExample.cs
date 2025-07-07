using System;
using System.Collections.Generic;
using System.Linq;
// #if SAINTSFIELD_JSON
// using Newtonsoft.Json;
// #endif
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictFillerExample : SaintsMonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        [Serializable]
        public class ValueFillerDict : SaintsDictionaryBase<int, GameObject>
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
            }

            [SerializeField, NoLabel]
            private List<SaintsWrap<int>> _intKeys = new List<SaintsWrap<int>>();

            [SerializeField, NoLabel, GetByXPath("scene:://*"), ReadOnly]
            private List<SaintsWrap<GameObject>> _objValues = new List<SaintsWrap<GameObject>>();

#if UNITY_EDITOR
            private static string EditorPropKeys => nameof(_intKeys);
            private static string EditorPropValues => nameof(_objValues);
#endif
            protected override int SerializedKeysCount()
            {
                return _intKeys.Count;
            }

            protected override void SerializedKeyAdd(int key)
            {
                _intKeys.Add(new SaintsWrap<int>{value = key});
            }

            protected override int SerializedKeyGetAt(int index)
            {
                return _intKeys[index].value;
            }

            protected override void SerializedKeysClear()
            {
                _intKeys.Clear();
            }

            protected override int SerializedValuesCount()
            {
                return _objValues.Count;
            }

            protected override void SerializedValueAdd(GameObject value)
            {
                _objValues.Add(new SaintsWrap<GameObject> { value = value });
            }

            protected override GameObject SerializedValueGetAt(int index)
            {
                return _objValues[index].value;
            }

            protected override void SerializedValuesClear()
            {
                _objValues.Clear();
            }

            protected override void SerializedSetKeyValue(int tKey, GameObject tValue)
            {
                int index = _intKeys.FindIndex(wrap => wrap.value.Equals(tKey));
                if (index >= 0)
                {
                    _objValues[index].value = tValue;
                }
                else
                {
                    _intKeys.Add(new SaintsWrap<int>{value = tKey});
                    _objValues.Add(new SaintsWrap<GameObject>{value = tValue});
                }
            }

            protected override void SerializedRemoveKeyValue(int key)
            {
                int index = _intKeys.FindIndex(wrap => wrap.value.Equals(key));
                if (index >= 0)
                {
                    _intKeys.RemoveAt(index);
                    _objValues.RemoveAt(index);
                }
            }
        }

        public ValueFillerDict valueFillerDict;

        [SaintsDictionary("Slot", "Enemy", numberOfItemsPerPage: 5)]
        public ValueFillerDict decValueFillerDict;

        [LayoutStart("Buttons", ELayout.Horizontal)]

        [Button]
        private void AddRandom()
        {
            int[] keys = Enumerable.Range(0, 100).Except(decValueFillerDict.Keys).ToArray();
            if (keys.Length == 0)
            {
                return;
            }

            int key = keys[UnityEngine.Random.Range(0, keys.Length)];
            decValueFillerDict.Add(key, gameObject);
        }

        [Button]
        private void DeleteRandom()
        {
            int[] keys = decValueFillerDict.Keys.ToArray();
            if (keys.Length == 0)
            {
                return;
            }
            decValueFillerDict.Remove(keys[UnityEngine.Random.Range(0, keys.Length)]);
        }

        // [Serializable]
        // public struct MyStruct
        // {
        //     [NoLabel, AboveRichLabel]
        //     public string myStringField;
        //     [NoLabel, AboveRichLabel]
        //     public int myIntField;
        // }

//         [Serializable]
//         public class MyConfig: SaintsDictionaryBase<int, MyStruct>
//         {
//             [SerializeField]
//             private List<Wrap<int>> _keys = new List<Wrap<int>>();
//
//             [SerializeField]
//             // [GetComponentInChildren]
//             private List<Wrap<MyStruct>> _values = new List<Wrap<MyStruct>>();
//
// #if UNITY_EDITOR
//             private static string EditorPropKeys => nameof(_keys);
//             private static string EditorPropValues => nameof(_values);
// #endif
//             protected override List<Wrap<int>> SerializedKeys => _keys;
//             protected override List<Wrap<MyStruct>> SerializedValues => _values;
//         }

        // public SaintsDictionary<int, MyStruct> basicType;
        // [Space(200)]
        // public MyConfig basicType;

// #if SAINTSFIELD_JSON
//         [Button]
//         private void SerializeTarget()
//         {
//             Debug.Log(JsonConvert.SerializeObject(basicType));
//         }
// #endif
#endif
    }
}
