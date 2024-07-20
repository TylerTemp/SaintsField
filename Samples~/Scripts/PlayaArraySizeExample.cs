using System;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts
{
    public class PlayaArraySizeExample : SaintsMonoBehaviour
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
