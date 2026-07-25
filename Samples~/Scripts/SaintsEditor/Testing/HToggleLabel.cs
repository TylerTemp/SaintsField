using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class HToggleLabel : SaintsMonoBehaviour
    {
        [LeftToggle]
        public bool oLeftBool;

        [NoLabel] public bool oNoLabelBool;
        [NoLabel, LeftToggle] public bool oNoLabelLeftToggle;

        [LabelText("LabelTextBool!")] public bool oLabelTextBool;
        [LabelText("LabelTextLeftBool!"), LeftToggle] public bool oLabelTextLeftToggle;

        [LayoutStart("H", ELayout.Horizontal)]

        [LeftToggle]
        public bool leftBool;

        [NoLabel]
        public bool noLabelBool;

        [NoLabel, LeftToggle]
        public bool noLabelLeftBool;

        public bool defaultLabelBool;
    }
}
