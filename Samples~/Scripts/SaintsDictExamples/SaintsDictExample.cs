using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsDictExamples
{
    public class SaintsDictExample : SaintsMonoBehaviour
    {
        public SaintsDictionary<string, GameObject> genDict;

        [ShowInInspector]
        private GameObject DebugKey(string key)
        {
            GameObject go = null;
            if(key != null && !genDict.TryGetValue(key, out go))
            {
                Debug.LogWarning($"Key {key} not found in dictionary");
                return null;
            }

            return go;
        }

        [Serializable]
        public struct Value
        {
            public string myStringField;
            public string myIntField;
        }

        public SaintsDictionary<int, Value> dict;

        public SaintsDictionary<string, GameObject[]> genDictValueArray;
        public SaintsDictionary<string, List<GameObject>> genDictValueList;

        [SaintsDictionary(numberOfItemsPerPage: 5)]
        public SaintsDictionary<int, string> kvPagin10 = new SaintsDictionary<int, string>
        {
            { 0, "0" },
            { 1, "1" },
            { 2, "2" },
            { 3, "3" },
            { 4, "4" },
            { 5, "5" },
            { 6, "6" },
            { 7, "7" },
            { 8, "8" },
            { 9, "9" },
        };

        [SaintsDictionary(keyWidth: "30%")] public SaintsDictionary<int, string> keyWidthControl;
        [SaintsDictionary(valueWidth: "120px")] public SaintsDictionary<int, string> valueWidthControl;

        private void Awake()
        {
            SaintsDictionary<int, Value> d = new SaintsDictionary<int, Value>();
            SaintsDictionary<int, Value> d2 = new SaintsDictionary<int, Value>(dict);
        }
    }
}
