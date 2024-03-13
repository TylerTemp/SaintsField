using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class ShowHideExample: MonoBehaviour
    {
        public bool _bool1;
        public bool _bool2;
        public bool _bool3;
        public bool _bool4;

        [ShowIf(nameof(_bool1))]
        [ShowIf(nameof(_bool2))]
        [RichLabel("<color=red>show=1||2")]
        public string _showIf1Or2;


        [ShowIf(nameof(_bool1), nameof(_bool2))]
        [RichLabel("<color=green>show=1&&2")]
        public string _showIf1And2;

        [HideIf(nameof(_bool1))]
        [HideIf(nameof(_bool2))]
        [RichLabel("<color=blue>show=!1||!2")]
        public string _hideIf1Or2;


        [HideIf(nameof(_bool1), nameof(_bool2))]
        [RichLabel("<color=yellow>show=!(1&&2)=!1||!2")]
        public string _hideIf1And2;

        [ShowIf(nameof(_bool1))]
        [HideIf(nameof(_bool2))]
        [RichLabel("<color=magenta>show=1||!2")]
        public string _showIf1OrNot2;

        [ShowIf(nameof(_bool1), nameof(_bool2))]
        [ShowIf(nameof(_bool3), nameof(_bool4))]
        [RichLabel("<color=orange>show=(1&&2)||(3&&4)")]
        public string _showIf1234;

        [HideIf(nameof(_bool1), nameof(_bool2))]
        [HideIf(nameof(_bool3), nameof(_bool4))]
        [RichLabel("<color=pink>show=!(1&&2)||!(3&&4)")]
        public string _hideIf1234;
    }
}
