using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ArraySizeExample : MonoBehaviour
    {
        // public int myInt;

        [ArraySize(3)]
        public string[] myArr;

        [Serializable]
        public struct Nest
        {
            [ArraySize(3)] public string[] arr3;
        }

        public Nest nest;
        public Nest[] nests;
    }
}
