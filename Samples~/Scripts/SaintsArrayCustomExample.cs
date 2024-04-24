using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class SaintsArrayCustomExample : MonoBehaviour
    {
        [Serializable]
        public struct MyArr
        {
            [RichLabel(nameof(MyInnerRichLabel), true)]
            public int[] myArray;

            private string MyInnerRichLabel(object _, int index) => $"<color=pink>[{(char)('A' + index)}]";
        }

        [RichLabel(nameof(MyOuterLabel), true), SaintsArray("myArray")]
        public MyArr[] myArr;

        private string MyOuterLabel(object _, int index) => $"<color=Olive> {index}";
    }
}
