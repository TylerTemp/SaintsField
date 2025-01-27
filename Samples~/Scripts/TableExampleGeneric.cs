using System;
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
        public MyStruct[] myStructs;
    }
}
