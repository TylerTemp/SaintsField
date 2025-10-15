﻿using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LeftToggleExample: MonoBehaviour
    {
        [LeftToggle] public bool myToggle;
        // To use with `RichLabel` in IMGUI, you need to add 5 spaces ahead as a hack
        [LeftToggle, FieldRichLabel("<color=green><label />")] public bool richToggle;
        [LeftToggle, FieldRichLabel(null)] public bool richToggle2;

        public bool normalToggle;

        [Serializable]
        public struct MyStruct
        {
            [LeftToggle, FieldBelowText(nameof(myToggle), true)]
            public bool myToggle;

            [LeftToggle, FieldBelowText(nameof(TogglesLabel), true)]
            public bool[] myToggles;

            public string TogglesLabel(bool _, int index) => myToggles[index].ToString();
        }

        public MyStruct myStruct;
        public MyStruct[] myStructs;

        [ReadOnly]
        [LeftToggle] public bool richToggleDisabled;
    }
}
