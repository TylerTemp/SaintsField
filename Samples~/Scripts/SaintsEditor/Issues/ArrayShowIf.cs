using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class ArrayHideIf : SaintsMonoBehaviour
    {
        [Serializable]
        public enum EnumValues
        {
            A,
            B,
            C,
        }

        public EnumValues e1;
        public EnumValues e2;

        [HideIf(EMode.Edit, nameof(e1), EnumValues.A)]
        [HideIf(EMode.Edit, nameof(e2), EnumValues.B)]
        public string[] hideAndArray;

        [FieldHideIf(EMode.Edit, nameof(e1), EnumValues.A)]
        [FieldHideIf(EMode.Edit, nameof(e2), EnumValues.B)]
        public string hideAndElement;

        [HideIf(EMode.Edit, nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        public string[] hideOrArray;

        [FieldHideIf(EMode.Edit, nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        [FieldHideIf(nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        public string hideOrElement;

        [FieldShowIf(false)] public string showIfElement;
        [FieldHideIf(false)] public string hideIfElement;
    }
}
