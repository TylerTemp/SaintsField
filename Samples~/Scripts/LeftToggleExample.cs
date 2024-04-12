using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LeftToggleExample: MonoBehaviour
    {
        [LeftToggle] public bool myToggle;
        // To use with `RichLabel` in IMGUI, you need to add 5 spaces ahead as a hack
        [LeftToggle, RichLabel("     <color=green><label />")] public bool richToggleImGui;
#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        // UIToolkit is cool
        [LeftToggle, RichLabel("<color=green><label />")] public bool richToggleUIToolkit;
#endif
        [LeftToggle, RichLabel(null)] public bool richToggle2;

        public bool normalToggle;

        [Serializable]
        public struct MyStruct
        {
            [LeftToggle, BelowRichLabel(nameof(myToggle), true)]
            public bool myToggle;

            [LeftToggle, BelowRichLabel(nameof(TogglesLabel), true)]
            public bool[] myToggles;

            public string TogglesLabel(bool _, int index) => myToggles[index].ToString();
        }

        public MyStruct myStruct;
        public MyStruct[] myStructs;

        [ReadOnly]
        [LeftToggle] public bool richToggleDisabled;
    }
}
