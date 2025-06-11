using System;
using System.Collections.Generic;
using System.Linq;
// #if SAINTSFIELD_JSON
// using Newtonsoft.Json;
// #endif
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictFillerExample : SaintsMonoBehaviour
    {
//         [Serializable]
//         public class ValueFillerDict : SaintsDictionaryBase<int, GameObject>
//         {
//             [SerializeField, NoLabel]
//             private List<Wrap<int>> _intKeys = new List<Wrap<int>>();
//
//             [SerializeField, NoLabel, GetComponentInChildren, ReadOnly]
//             private List<Wrap<GameObject>> _objValues = new List<Wrap<GameObject>>();
//
// #if UNITY_EDITOR
//             private static string EditorPropKeys => nameof(_intKeys);
//             private static string EditorPropValues => nameof(_objValues);
// #endif
//             protected override List<Wrap<int>> SerializedKeys => _intKeys;
//             protected override List<Wrap<GameObject>> SerializedValues => _objValues;
//         }

        // public ValueFillerDict valueFillerDict;

        // [SaintsDictionary("Slot", "Enemy", numberOfItemsPerPage: 5)]
        // public ValueFillerDict decValueFillerDict;

        // [LayoutStart("Buttons", ELayout.Horizontal)]

        // [Button]
        // private void AddRandom()
        // {
        //     int[] keys = Enumerable.Range(0, 100).Except(decValueFillerDict.Keys).ToArray();
        //     if (keys.Length == 0)
        //     {
        //         return;
        //     }
        //
        //     int key = keys[UnityEngine.Random.Range(0, keys.Length)];
        //     decValueFillerDict.Add(key, gameObject);
        // }

        // [Button]
        // private void DeleteRandom()
        // {
        //     int[] keys = decValueFillerDict.Keys.ToArray();
        //     if (keys.Length == 0)
        //     {
        //         return;
        //     }
        //     decValueFillerDict.Remove(keys[UnityEngine.Random.Range(0, keys.Length)]);
        // }

        [Serializable]
        public struct MyStruct
        {
            [NoLabel, AboveRichLabel]
            public string myStringField;
            [NoLabel, AboveRichLabel]
            public int myIntField;
        }

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
    }
}
