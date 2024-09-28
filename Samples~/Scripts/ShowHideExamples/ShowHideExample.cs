using UnityEngine;

namespace SaintsField.Samples.Scripts.ShowHideExamples
{
    public class ShowHideExample : MonoBehaviour
    {
        public bool bool1;
        public bool bool2;
        public bool bool3;
        public bool bool4;

        [ShowIf(nameof(bool1))]
        [ShowIf(nameof(bool2))]
        [RichLabel("<color=red>show=1||2")]
        public string showIf1Or2;

        [ShowIf(nameof(bool1), nameof(bool2))]
        [RichLabel("<color=green>show=1&&2")]
        public string showIf1And2;

        [HideIf(nameof(bool1))]
        [HideIf(nameof(bool2))]
        [RichLabel("<color=Lime>show=!1||!2")]
        public string hideIf1Or2;

        [HideIf(nameof(bool1), nameof(bool2))]
        [RichLabel("<color=yellow>show=!(1||2)=!1&&!2")]
        public string hideIf1And2;

        [ShowIf(nameof(bool1))]
        [HideIf(nameof(bool2))]
        [RichLabel("<color=magenta>show=1||!2")]
        public string showIf1OrNot2;

        [ShowIf(nameof(bool1), nameof(bool2))]
        [ShowIf(nameof(bool3), nameof(bool4))]
        [RichLabel("<color=orange>show=(1&&2)||(3&&4)")]
        public string showIf1234;

        [HideIf(nameof(bool1), nameof(bool2))]
        [HideIf(nameof(bool3), nameof(bool4))]
        [RichLabel("<color=pink>show=!(1||2)||!(3||4)=(!1&&!2)||(!3&&!4)")]
        public string hideIf1234;
    }
}
