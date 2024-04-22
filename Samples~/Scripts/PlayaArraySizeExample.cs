using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PlayaArraySizeExample : MonoBehaviour
    {
        [PlayaArraySize(3)] public int[] myArr3;

        [Serializable]
        public struct Nested
        {
            [PlayaArraySize(3)] public int[] nestedArr3;
        }

        [SaintsRow] public Nested nested;
        [SaintsRow] public Nested[] nestedArr;
    }
}
