using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ArraySizeExample : MonoBehaviour
    {
        [ArraySize(3)] public string[] arr3;

        [Serializable]
        public struct Nest
        {
            [ArraySize(3)] public string[] arr3;
        }

        public Nest[] nests;
    }
}
