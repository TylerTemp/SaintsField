using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class HorizontalLayoutLight: SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string ss;
            public int si;
        }

        public string ot;

        [LayoutStart("Equipment", ELayout.TitleBox)]

        [LayoutStart("./Head")]
        [PlayaSeparator("Head", EAlign.Center)]
        public string st;
        [LayoutCloseHere]
        public MyStruct inOneStruct;

        [LayoutStart("./Upper Body")]

        [PlayaInfoBox("Note：left hand can be empty, but not right hand", EMessageType.Warning)]

        [LayoutStart("./Horizontal", ELayout.Horizontal)]

        [LayoutStart("./Left Hand")]
        [PlayaSeparator("Left Hand", EAlign.Center)]
        public string g11;
        public string g12;
        public MyStruct myStruct;
        public string g13;

        [LayoutStart("../Right Hand")]
        [PlayaSeparator("Right Hand", EAlign.Center)]
        public string g21;
        [RichLabel("<color=lime><label/>")]
        public string g22;
        [RichLabel("$" + nameof(g23))]
        public string g23;

        public bool toggle;
    }
}
