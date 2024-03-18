using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class SpriteToggle : SpriteToggleBase
    {
        [SerializeField, SpriteToggle(nameof(SpriteRenderer))] private Sprite sprite2;
    }
}
