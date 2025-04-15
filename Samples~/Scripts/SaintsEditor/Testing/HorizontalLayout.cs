using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class HorizontalLayout : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string ss;
            public int si;
        }

        // [PlayaSeparator("上身装备", EAlign.Center)]
        public string ot;

        [LayoutStart("Equipment", ELayout.TitleBox | ELayout.Vertical)]
        [LayoutStart("./Head", ELayout.TitleBox)]
        public string st;
        [LayoutCloseHere]
        public MyStruct inOneStruct;

        [LayoutStart("./Upper Body", ELayout.TitleBox)]

        [PlayaInfoBox("Note：left hand can be empty, but not right hand", EMessageType.Warning)]

        [LayoutStart("./Horizontal", ELayout.Horizontal)]

        [LayoutStart("./Left Hand", ELayout.TitleBox)]
        public string g11;
        public string g12;
        public MyStruct myStruct;
        public string g13;

        [LayoutStart("../Right Hand", ELayout.TitleBox)]
        public string g21;
        [RichLabel("<color=lime><label/>")]
        public string g22;
        [RichLabel("$" + nameof(g23))]
        public string[] g23;

        public bool toggle;

        [LayoutEnd]
        [PlayaInfoBox("Buttons!")]
        [LayoutStart("Buttons", ELayout.Horizontal)]
        [Button]
        public void B1(string strV, bool bv, Vector2 v2)
        {

        }

        [Button]
        public void B2(MyStruct structV, Vector4 v4, Bounds bounds, Rect rectValue)
        {

        }

        [LayoutStart("V", ELayout.TitleBox)]
        [Expandable] public Scriptable soV;
        [AnimatorState] public AnimatorStateBase animBaseV;

        [LayoutStart("Horizontal", ELayout.TitleBox | ELayout.Horizontal)]
        [Expandable] public Scriptable so;
        [AnimatorState] public AnimatorStateBase animBase;
        // [LayoutStart("Tab", ELayout.TitleBox)]
        // // public string tab;
        //
        // // [LayoutStart("./1", ELayout.TitleBox)]
        // // public string tab1Sub1;
        // public string tab1Sub2;
        // [LayoutTerminateHere]
        // public string tab1Sub3;
        //
        // [Button]
        // public void AFunction() {}
        // [Button]
        // public void BFunction() {}
    }
}
