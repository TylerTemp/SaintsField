using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class TableExampleGeneric : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int myInt;
            public string myString;
            public GameObject myObject;
        }

        [Table]
        [OnArraySizeChanged(nameof(SizeChanged))]
        public List<MyStruct> myStructs;

        [Button]
        public void Add()
        {
            myStructs.Add(new MyStruct
            {
                myInt = UnityEngine.Random.Range(0, 100),
            });
        }

        private void SizeChanged(List<MyStruct> newLis) => Debug.Log(newLis.Count);
    }
}
