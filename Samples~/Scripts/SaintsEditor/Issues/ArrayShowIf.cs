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

        [PlayaHideIf(EMode.Edit, nameof(e1), EnumValues.A)]
        [PlayaHideIf(EMode.Edit, nameof(e2), EnumValues.B)]
        public string[] hideAndArray;

        [HideIf(EMode.Edit, nameof(e1), EnumValues.A)]
        [HideIf(EMode.Edit, nameof(e2), EnumValues.B)]
        public string hideAndElement;

        [PlayaHideIf(EMode.Edit, nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        public string[] hideOrArray;

        [HideIf(EMode.Edit, nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        [HideIf(nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        public string hideOrElement;

        [ShowIf(false)] public string showIfElement;
        [HideIf(false)] public string hideIfElement;
    }
}
