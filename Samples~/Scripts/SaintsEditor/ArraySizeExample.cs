using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ArraySizeExample : SaintsMonoBehaviour
    {
        [ArraySize(3)]
        public string[] myStrings;

        [ArraySize(3)]
        public string[] myArr;

        [Serializable]
        public struct Nest
        {
            [ArraySize(3)] public string[] arr3;
        }

        [SaintsRow]
        public Nest nest;

        [SaintsRow]
        public Nest[] nests;
    }
}
