using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsArrayCustomExample : MonoBehaviour
    {
        [Serializable]
        public struct MyArr
        {
            [RichLabel(nameof(MyArrayRichLabel))]
            public int[] myArray;

            private string MyArrayRichLabel(int index) => $"<color=pink>[{(char)('A' + index)}]";
        }

        [SaintsArray("myArray")]
        public MyArr[] myArr;
    }
}
