using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaInfoBoxExample : SaintsMonoBehaviour
    {
        [PlayaInfoBox("Please Note: special label like <icon=star.png/> only works for <color=lime>UI Toolkit</color> <color=red>(not IMGUI)</color> in InfoBox.")]
        [PlayaBelowInfoBox("$" + nameof(DynamicFromArray))]
        public string[] strings = {};

        public string dynamic;

        private string DynamicFromArray(string[] value) => value.Length > 0? string.Join("\n", value): "null";

        [PlayaInfoBox("MethodWithButton")]
        [Button("Click Me!")]
        [PlayaBelowInfoBox("GroupExample", groupBy: "group")]
        [PlayaBelowInfoBox("$" + nameof(dynamic), groupBy: "group")]
        public void MethodWithButton()
        {
        }

        [PlayaInfoBox("Method")]
        [PlayaBelowInfoBox("$" + nameof(dynamic))]
        public void Method()
        {
        }

        [PlayaInfoBox("Property")] public string V => "V";
    }
}
