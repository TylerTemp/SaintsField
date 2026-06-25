namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.LabelTextForSaintsFallback
{
    public class LabelTextForSaintsFallbackTest : SaintsMonoBehaviour
    {
        [LabelText("<color=green>fall")]
        [FieldInfoBox("Let SaintsField Drawer Fall to raw")]
        public string str;

        [LabelText("<color=red>fall third")]
        [FieldInfoBox("Let SaintsField Drawer Fall to other drawer")]
        public CustomType customType;
    }
}
