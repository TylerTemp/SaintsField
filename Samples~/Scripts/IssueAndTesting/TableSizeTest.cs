using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class TableSizeTest : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public int myInt;
            public string myString;
            public GameObject myObject;
        }

        [Table]
        [ArraySize(3)]
        public List<MyStruct> fixed3;

        [Table]
        [ArraySize(1, 3)]
        public MyStruct[] min1Max3;

        [MinValue(0)]
        public int intValue;
        [Table]
        [ArraySize(nameof(intValue))]
        public MyStruct[] dynamicFixed;

        public Vector2Int v2Value;
        [Table]
        [ArraySize(nameof(v2Value))]
        public MyStruct[] dynamicRange;
    }
}
