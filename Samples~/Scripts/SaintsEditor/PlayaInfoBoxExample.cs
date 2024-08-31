using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaInfoBoxExample : MonoBehaviour
    {
        [PlayaInfoBox("Array <color=green>Info</color> with long long long long long long long long long content")]
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
