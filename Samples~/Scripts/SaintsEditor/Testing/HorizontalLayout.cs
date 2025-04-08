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

        public string ot;

        [LayoutStart("V", ELayout.TitleBox | ELayout.Vertical)]
        public string st;
        public MyStruct inOneStruct;

        [PlayaInfoBox("There!")]

        [LayoutStart("./H", ELayout.FoldoutBox | ELayout.Horizontal)]
        [LayoutStart("./G1", ELayout.TitleBox)]
        public string g11;
        public string g12;
        public MyStruct myStruct;
        public string g13;

        [LayoutStart("../G2", ELayout.TitleBox)]
        public string g21;
        [RichLabel("<color=lime><label/>")]
        public string g22;
        [RichLabel("$" + nameof(g23))]
        public string g23;

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
