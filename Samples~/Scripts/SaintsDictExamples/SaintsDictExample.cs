using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictExample : SaintsMonoBehaviour
    {
        public SaintsDictionary<string, GameObject> genDict;

        [Button]
        private void DebugKey(string key)
        {
            if(!genDict.TryGetValue(key, out GameObject go))
            {
                Debug.LogWarning($"Key {key} not found in dictionary");
                return;
            }
            Debug.Log(go);
        }

        [Serializable]
        public struct Value
        {
            public string myStringField;
            public string myIntField;
        }

        public SaintsDictionary<int, Value> dict;

        private void Awake()
        {

            SaintsDictionary<int, Value> d = new SaintsDictionary<int, Value>();
            SaintsDictionary<int, Value> d2 = new SaintsDictionary<int, Value>(dict);
        }
    }
}
