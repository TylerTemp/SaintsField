using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue3
{
    public class Issue3Problem: MonoBehaviour
    {
        [RichLabel("<icon=star.png/>")] public string richIcon;
        [AboveRichLabel("<icon=star.png/>")]
        [AboveRichLabel("<icon=star.png/>")]
        public string richAboveIcon;
        [BelowRichLabel("<icon=star.png/>")]
        [BelowRichLabel("<icon=star.png/>")]
        public string richBelowIcon;

        [OverlayRichLabel("<icon=star.png/>")]
        public string richOverlayIcon;
        [PostFieldRichLabel("<icon=star.png/>")]
        public string richPostIcon;

        [PostFieldButton(nameof(ButtonCallback), "<icon=star.png/>")]
        [AboveButton(nameof(ButtonCallback), "<icon=star.png/>")]
        [BelowButton(nameof(ButtonCallback), "<icon=star.png/>")]
        public string buttons;

        [GameObjectActive]
        public GameObject go;

        [Rate(0, 5)] public int rate;

        private void ButtonCallback() => Debug.Log("Clicked");
    }
}
