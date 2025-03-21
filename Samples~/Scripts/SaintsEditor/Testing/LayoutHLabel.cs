using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class LayoutHLabel : SaintsMonoBehaviour
    {
        // [LayoutStart("H", ELayout.Horizontal | ELayout.TitleBox)]
        //
        public string f1;
        // public string f2;
        // // public string f3;
        // //
        // [LayoutStart("C", ELayout.Horizontal | ELayout.TitleBox)]
        [PropRange(0, 10)] public int pRange;

        [Serializable]
        public enum MyEnum
        {
            A,
            B,
            C
        }

        [EnumToggleButtons] public MyEnum myEnum;
    }
}
