using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public partial class EnumDropdownDefaultFix : SaintsMonoBehaviour
    {
        [Serializable]
        public enum MyEnum
        {
            [InspectorName("null")]
            None,

            [InspectorName("Value/Content/1")]
            One,

            [InspectorName("Value/Sub/2")]
            Two,
        }

        public MyEnum defaultRender;
        [Dropdown(slashAsSub = false)]
        public MyEnum dropdownRender;

        [Serializable, Flags]
        public enum MyEnumFlags
        {
            [InspectorName("null")]
            None,

            [InspectorName("Value/Content/1")]
            One = 1,

            [InspectorName("Value/Sub/2")]
            Two = 1 << 1,

            [InspectorName("Value/Sub/3")]
            Three = 1 << 2,

            [InspectorName("all")]
            Full = None | One | Two | Three,
        }

        public MyEnumFlags defaultFlags;
        [Dropdown(slashAsSub: false)]
        public MyEnumFlags dropdownSingleFlags;
        [FlagsDropdown(slashAsSub: false), BelowText("<field/>")]
        public MyEnumFlags dropdownMultiFlags;

        [EnumToggleButtons, BelowText("<field/>")]
        public MyEnumFlags buttonMultiFlags;

        [Flags]
        public enum MyEnumFlagsLong: long
        {
            [InspectorName("null")]
            None,

            [InspectorName("Value/Content/1")]
            One = 1,

            [InspectorName("Value/Sub/2")]
            Two = 1 << 1,

            [InspectorName("Value/Sub/3")]
            Three = 1 << 2,

            [InspectorName("all")]
            Full = None | One | Two | Three,
        }

        [SaintsSerialized, NonSerialized, BelowText("<field/>")]
        public MyEnumFlagsLong defaultMultiFlagsLong;
        [SaintsSerialized, NonSerialized, EnumToggleButtons, BelowText("<field/>")]
        public MyEnumFlagsLong buttonMultiFlagsLong;

        [Flags]
        public enum MyEnumFlagsULong: ulong
        {
            [InspectorName("null")]
            None,

            [InspectorName("Value/Content/1")]
            One = 1,

            [InspectorName("Value/Sub/2")]
            Two = 1 << 1,

            [InspectorName("Value/Sub/3")]
            Three = 1 << 2,

            [InspectorName("all")]
            Full = None | One | Two | Three,
        }

        [SaintsSerialized, NonSerialized, BelowText("<field/>")]
        public MyEnumFlagsULong defaultMultiFlagsULong;
        [SaintsSerialized, NonSerialized, EnumToggleButtons, BelowText("<field/>")]
        public MyEnumFlagsULong buttonMultiFlagsULong;
    }
}
