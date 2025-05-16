using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    [HeaderLabel("$" + nameof(value))]
    [HeaderLeftLabel("dynamic:")]
    public class HeaderLabelClassExample : SaintsMonoBehaviour
    {
        public string value;
    }
}
