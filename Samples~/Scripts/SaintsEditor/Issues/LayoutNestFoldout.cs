using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class LayoutNestFoldout : SaintsMonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string structString;
        }

        [Serializable, Flags]
        public enum MyEnum
        {
            EnumValue1,
            EnumValue2,
            EnumValue3,
            EnumValue4,
        }

        public string[] plainStrings;
        public MyStruct plainStruct;
        [EnumToggleButtons]
        public MyEnum plainEnum;

        [LayoutStart("Layout1", ELayout.FoldoutBox)]

        public string[] l1Strings;
        public MyStruct l1Struct;
        [EnumToggleButtons]
        public MyEnum l1Enum;

        [LayoutStart("./Layout2", ELayout.FoldoutBox)]

        public string[] l2Strings;
        public MyStruct l2Struct;
        [EnumToggleButtons]
        public MyEnum l2Enum;

        [EnumToggleButtons, FieldLabelText("<color=red><label/>")]
        public MyEnum l2EnumLabel;
    }
}
