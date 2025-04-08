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

        [LayoutStart("装备", ELayout.TitleBox | ELayout.Vertical)]
        [LayoutStart("./头部", ELayout.TitleBox)]
        public string st;
        [LayoutCloseHere]
        public MyStruct inOneStruct;

        [LayoutStart("./上身装备", ELayout.TitleBox)]

        [PlayaInfoBox("注意：左手可以为空，右手不能为空", EMessageType.Warning)]

        [LayoutStart("./Horizontal", ELayout.Horizontal)]

        [LayoutStart("./左手", ELayout.TitleBox)]
        public string g11;
        public string g12;
        public MyStruct myStruct;
        public string g13;

        [LayoutStart("../右手", ELayout.TitleBox)]
        public string g21;
        [RichLabel("<color=lime><label/>")]
        public string g22;
        [RichLabel("$" + nameof(g23))]
        public string g23;

        public bool toggle;

        // [LayoutEnd]
        // [PlayaInfoBox("Buttons!")]
        // [LayoutStart("Buttons", ELayout.Horizontal)]
        // [Button]
        // public void B1(string strV, bool bv, Vector2 v2)
        // {
        //
        // }
        //
        // [Button]
        // public void B2(MyStruct structV, Vector4 v4, Bounds bounds, Rect rectValue)
        // {
        //
        // }


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
