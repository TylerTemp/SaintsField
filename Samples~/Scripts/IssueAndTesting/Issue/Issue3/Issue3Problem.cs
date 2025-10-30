using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue3
{
    public class Issue3Problem: MonoBehaviour
    {
        [FieldLabelText("<icon=star.png/>")] public string richIcon;
        [FieldAboveText("<icon=star.png/>")]
        [FieldAboveText("<icon=star.png/>")]
        public string richAboveIcon;
        [FieldBelowText("<icon=star.png/>")]
        [FieldBelowText("<icon=star.png/>")]
        public string richBelowIcon;

        [OverlayText("<icon=star.png/>")]
        public string richOverlayIcon;
        [EndText("<icon=star.png/>")]
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
