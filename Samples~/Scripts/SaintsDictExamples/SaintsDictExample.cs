using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictExample : SaintsMonoBehaviour
    {
        // public SaintsDictionary<string, GameObject> genDict;
        //
        // [Button]
        // private void DebugKey(string key)
        // {
        //     if(!genDict.TryGetValue(key, out GameObject go))
        //     {
        //         Debug.LogWarning($"Key {key} not found in dictionary");
        //         return;
        //     }
        //     Debug.Log(go);
        // }

        [Serializable]
        public struct Value
        {
            public string myStringField;
            public string myIntField;
        }

        public SaintsDictionary<int, Value> dict;

//         [Serializable]
//         public struct MyStruct
//         {
//             public string myStringField;
//             public int myIntField;
//         }
//
//         [Serializable]
//         public class MyConfig: SaintsDictionaryBase<int, MyStruct>
//         {
//             [SerializeField]
//             private List<int> _keys = new List<int>();
//
//             [SerializeField, SaintsRow(inline: true)]
//             // [GetComponentInChildren]
//             private List<MyStruct> _values = new List<MyStruct>();
//
// #if UNITY_EDITOR
//             private static string EditorPropKeys => nameof(_keys);
//             private static string EditorPropValues => nameof(_values);
// #endif
//             protected override List<int> SerializedKeys => _keys;
//             protected override List<MyStruct> SerializedValues => _values;
//         }
//
//         public MyConfig basicType;
    }
}
