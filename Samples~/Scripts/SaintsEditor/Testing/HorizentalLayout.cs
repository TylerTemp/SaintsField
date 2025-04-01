using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class HorizentalLayout : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string ss;
            public int si;
        }

        public MyStruct outStruct;

        [LayoutStart("H", ELayout.TitleBox | ELayout.Horizontal)]

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
    }
}
