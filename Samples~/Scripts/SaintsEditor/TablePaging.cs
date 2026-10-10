using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class TablePaging : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            [PropRange(0, 100)]
            public int myInt;
            public string myString;
        }

        [Table] public MyStruct[] numbers;
    }
}
