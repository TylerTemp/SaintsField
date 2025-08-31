using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderLabelExample : SaintsMonoBehaviour
    {
        [HeaderLeftLabel("Fixed Text")]
        [HeaderLabel]  // dynamic text
        public string label;
    }
}
