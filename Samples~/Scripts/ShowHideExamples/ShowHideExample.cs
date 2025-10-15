using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowHideExample : MonoBehaviour
    {
        public bool bool1;
        public bool bool2;
        public bool bool3;
        public bool bool4;

        [FieldShowIf(nameof(bool1))]
        [FieldShowIf(nameof(bool2))]
        [FieldRichLabel("<color=red>show=1||2")]
        public string showIf1Or2;

        [FieldShowIf(nameof(bool1), nameof(bool2))]
        [FieldRichLabel("<color=green>show=1&&2")]
        public string showIf1And2;

        [FieldHideIf(nameof(bool1))]
        [FieldHideIf(nameof(bool2))]
        [FieldRichLabel("<color=Lime>show=!1||!2")]
        public string hideIf1Or2;

        [FieldHideIf(nameof(bool1), nameof(bool2))]
        [FieldRichLabel("<color=yellow>show=!(1||2)=!1&&!2")]
        public string hideIf1And2;

        [FieldShowIf(nameof(bool1))]
        [FieldHideIf(nameof(bool2))]
        [FieldRichLabel("<color=magenta>show=1||!2")]
        public string showIf1OrNot2;

        [FieldShowIf(nameof(bool1), nameof(bool2))]
        [FieldShowIf(nameof(bool3), nameof(bool4))]
        [FieldRichLabel("<color=orange>show=(1&&2)||(3&&4)")]
        public string showIf1234;

        [FieldHideIf(nameof(bool1), nameof(bool2))]
        [FieldHideIf(nameof(bool3), nameof(bool4))]
        [FieldRichLabel("<color=pink>show=!(1||2)||!(3||4)=(!1&&!2)||(!3&&!4)")]
        public string hideIf1234;
    }
}
