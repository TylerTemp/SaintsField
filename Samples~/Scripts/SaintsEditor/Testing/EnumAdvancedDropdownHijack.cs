using System;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class EnumAdvancedDropdownHijack : SaintsMonoBehaviour
    {
        [Serializable]
        public enum EnumNormal
        {
            Neg3,
            Neg2,
            Neg1,
            [FieldLabelText("0")]
            Zero,

            [FieldLabelText("Single/1")]
            One,
            [FieldLabelText("Plural/2")]
            Two,
            [FieldLabelText("Plural/3")]
            Three,
            [FieldLabelText("Few/4")]
            Four,
            [FieldLabelText("Few/5")]
            Five,
            [FieldLabelText("Few/6")]
            Sex,
            [FieldLabelText("Few/7")]
            Seven,
        }

        public EnumNormal enumNormalPure;

        [FieldBelowText("<color=gray><field/>")]
        public EnumNormal enumNormalFall;

        public EnumNormal[] enumNormalPures;

        [FieldBelowText("<color=gray><field/>")]
        public EnumNormal[] enumNormalFalls;

        [Serializable, Flags]
        public enum EnumFlags
        {
            Top = 1,
            Right = 1 << 1,
            Bottom = 1 << 2,
            Left = 1 << 3,

            [FieldLabelText("Group/TopLeft")]
            TopLeft = Top | Left,
            [FieldLabelText("Group/TopRight")]
            TopRight = Top | Right,
            [FieldLabelText("Group/BottomRight")]
            BottomRight = Bottom | Right,
            [FieldLabelText("Group/BottomLeft")]
            BottomLeft = Bottom | Left,
        }

        public EnumFlags enumFlagsPure;
        [FieldBelowText("<color=gray><field=B/>")]
        [FieldBelowText("<color=gray><field=B4/>")]
        public EnumFlags enumFlagsDec;

        public EnumFlags[] enumFlagsPures;
        [FieldBelowText("<color=gray><field=B/>")]
        public EnumFlags[] enumFlagsDecs;
    }
}
