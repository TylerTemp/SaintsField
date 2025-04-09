using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class NoContLayout : SaintsMonoBehaviour
    {
        [Layout("1", ELayout.TitleBox)] public string l1;

        public string lOut;

        [Layout("1")] public string l2;

        [LayoutStart("Start", ELayout.TitleBox)]
        public string s1;
        public string s2;
    }
}
