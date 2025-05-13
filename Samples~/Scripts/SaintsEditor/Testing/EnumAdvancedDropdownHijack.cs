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
            [RichLabel("0")]
            Zero,

            [RichLabel("Single/1")]
            One,
            [RichLabel("Plural/2")]
            Two,
            [RichLabel("Plural/3")]
            Three,
            [RichLabel("Few/4")]
            Four,
            [RichLabel("Few/5")]
            Five,
            [RichLabel("Few/6")]
            Sex,
            [RichLabel("Few/7")]
            Seven,
        }

        public EnumNormal enumNormalPure;

        [BelowRichLabel("<color=gray><field/>")]
        public EnumNormal enumNormalFall;

        public EnumNormal[] enumNormalPures;

        [BelowRichLabel("<color=gray><field/>")]
        public EnumNormal[] enumNormalFalls;

        [Serializable, Flags]
        public enum EnumFlags
        {
            Top = 1,
            Right = 1 << 1,
            Bottom = 1 << 2,
            Left = 1 << 3,

            [RichLabel("Group/TopLeft")]
            TopLeft = Top | Left,
            [RichLabel("Group/TopRight")]
            TopRight = Top | Right,
            [RichLabel("Group/BottomRight")]
            BottomRight = Bottom | Right,
            [RichLabel("Group/BottomLeft")]
            BottomLeft = Bottom | Left,
        }

        public EnumFlags enumFlagsPure;
        [BelowRichLabel("<color=gray><field=B/>")]
        [BelowRichLabel("<color=gray><field=B4/>")]
        public EnumFlags enumFlagsDec;

        public EnumFlags[] enumFlagsPures;
        [BelowRichLabel("<color=gray><field=B/>")]
        public EnumFlags[] enumFlagsDecs;
    }
}
