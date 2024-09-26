using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class ArrayShowIf : SaintsMonoBehaviour
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

        [PlayaShowIf(nameof(e1), EnumValues.A)]
        [PlayaShowIf(nameof(e2), EnumValues.B)]
        public string[] showOrArray;

        [ShowIf(nameof(e1), EnumValues.A)]
        [ShowIf(nameof(e2), EnumValues.B)]
        public string showOrElement;

        [PlayaShowIf(nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        public string[] showAndArray;

        [ShowIf(nameof(e1), EnumValues.A, nameof(e2), EnumValues.B)]
        public string showAndElement;
    }
}
