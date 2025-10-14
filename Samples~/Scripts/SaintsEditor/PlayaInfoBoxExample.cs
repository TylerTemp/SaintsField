using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaInfoBoxExample : SaintsMonoBehaviour
    {
        [InfoBox("Please Note: special label like <icon=star.png/> only works for <color=lime>UI Toolkit</color> <color=red>(not IMGUI)</color> in InfoBox.")]
        [BelowInfoBox("$" + nameof(DynamicFromArray))]
        public string[] strings = {};

        public string dynamic;

        private string DynamicFromArray(string[] value) => value.Length > 0? string.Join("\n", value): "null";

        [InfoBox("MethodWithButton")]
        [Button("Click Me!")]
        [BelowInfoBox("GroupExample", groupBy: "group")]
        [BelowInfoBox("$" + nameof(dynamic), groupBy: "group")]
        public void MethodWithButton()
        {
        }

        [InfoBox("Method")]
        [BelowInfoBox("$" + nameof(dynamic))]
        public void Method()
        {
        }

        [InfoBox("Property")] public string V => "V";
    }
}
