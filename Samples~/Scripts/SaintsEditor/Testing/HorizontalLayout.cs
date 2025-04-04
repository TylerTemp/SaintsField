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

        // public MyStruct outStruct;

        [Layout("V", ELayout.TitleBox | ELayout.Vertical)]
        public MyStruct inOneStruct;

        [LayoutStart("H", ELayout.FoldoutBox | ELayout.Horizontal)]
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
        [LayoutStart("Buttons", ELayout.Horizontal)]
        [Button]
        public void B1(string strV, bool bv, Vector2 v2)
        {

        }

        [Button]
        public void B2(MyStruct structV, Vector4 v4, Bounds bounds, Rect rectValue)
        {

        }
    }
}
