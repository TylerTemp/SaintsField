using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class Issue223NativeHeader : SaintsMonoBehaviour
    {
        [Header("Title 1")]
        public int value1;

        [SepTitle("Title 2")]
        public int value2;

        [Header("Title 3")]
        [RichLabel("Value 3")]
        public int value3;

        [SepTitle("Title 4")]
        [RichLabel("Value 4")]
        public int value4;
    }
}
