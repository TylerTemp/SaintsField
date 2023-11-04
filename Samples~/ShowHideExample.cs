using UnityEngine;

namespace SaintsField.Samples
{
    public class ShowHideExample: MonoBehaviour
    {
        [SerializeField] private bool _bool1;
        [SerializeField] private bool _bool2;
        [SerializeField] private bool _bool3;
        [SerializeField] private bool _bool4;

        [SerializeField]
        [ShowIf(nameof(_bool1))]
        [ShowIf(nameof(_bool2))]
        [RichLabel("simple: show=1||2 / <label />")]
        private string _showIf1Or2;


        [SerializeField]
        [ShowIf(nameof(_bool1), nameof(_bool2))]
        [RichLabel("simple: show=1&&2 / <label />")]
        private string _showIf1And2;

        [SerializeField]
        [HideIf(nameof(_bool1))]
        [HideIf(nameof(_bool2))]
        [RichLabel("simple: show=!1||!2 / <label />")]
        private string _hideIf1Or2;


        [SerializeField]
        [HideIf(nameof(_bool1), nameof(_bool2))]
        [RichLabel("simple: show=!1&&!2 / <label />")]
        private string _hideIf1And2;

        [SerializeField]
        [ShowIf(nameof(_bool1))]
        [HideIf(nameof(_bool2))]
        [RichLabel("simple: show=1||!2 / <label />")]
        private string _showIf1OrNot2;

        [SerializeField]
        [ShowIf(nameof(_bool1), nameof(_bool2))]
        [ShowIf(nameof(_bool3), nameof(_bool4))]
        [RichLabel("complex: show=(1&&2)||(3&&4) / <label />")]
        private string _showIf1234;

        [SerializeField]
        [HideIf(nameof(_bool1), nameof(_bool2))]
        [HideIf(nameof(_bool3), nameof(_bool4))]
        [RichLabel("complex: show=(!1&&!2)||(!3&&!4)")]
        private string _hideIf1234;
    }
}
