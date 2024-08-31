using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaInfoBoxExample : MonoBehaviour
    {
        [PlayaInfoBox("Please Note: label only works for <color=lime>UI Toolkit</color> <color=red>(not IMGUI)</color> in InfoBox.")]
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
    }
}
