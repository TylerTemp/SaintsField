using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ArraySizeExample : MonoBehaviour
    {
        // public int myInt;

        [ArraySize(2)]
        public string[] myArr;

        // public string myStr;
        //
        // [Serializable]
        // public struct Nest
        // {
        //     [ArraySize(3)] public string[] arr3;
        // }
        //
        // public Nest[] nests;
    }
}
