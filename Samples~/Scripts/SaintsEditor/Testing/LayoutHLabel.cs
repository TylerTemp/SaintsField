using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutHLabel : SaintsMonoBehaviour
    {
        public string f1;

        [InfoBox("Test Info")]
        public string f2;

        [PropRange(0, 10)] public int pRange;

        [Serializable]
        public enum MyEnum
        {
            A,
            B,
            C
        }

        [EnumToggleButtons] public MyEnum myEnum;

        [Serializable]
        public struct MyStruct
        {
            public string myString;
        }

        public MyStruct myStruct;
    }
}
