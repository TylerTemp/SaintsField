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
            [FieldRichLabel("0")]
            Zero,

            [FieldRichLabel("Single/1")]
            One,
            [FieldRichLabel("Plural/2")]
            Two,
            [FieldRichLabel("Plural/3")]
            Three,
            [FieldRichLabel("Few/4")]
            Four,
            [FieldRichLabel("Few/5")]
            Five,
            [FieldRichLabel("Few/6")]
            Sex,
            [FieldRichLabel("Few/7")]
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

            [FieldRichLabel("Group/TopLeft")]
            TopLeft = Top | Left,
            [FieldRichLabel("Group/TopRight")]
            TopRight = Top | Right,
            [FieldRichLabel("Group/BottomRight")]
            BottomRight = Bottom | Right,
            [FieldRichLabel("Group/BottomLeft")]
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
