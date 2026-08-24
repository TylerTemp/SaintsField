using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PlayaArraySizeExample : MonoBehaviour
    {
        [ArraySize(3)] public int[] myArr3;

        [Serializable]
        public struct Nested
        {
            [ArraySize(3)] public int[] nestedArr3;
        }

        [SaintsRow] public Nested nested;
        [SaintsRow] public Nested[] nestedArr;
    }
}
