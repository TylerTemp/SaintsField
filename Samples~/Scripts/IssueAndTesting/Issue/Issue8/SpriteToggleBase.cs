using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class SpriteToggleBase : MonoBehaviour
    {
        [field: SerializeField] private SpriteRenderer spriteRenderer;

        protected SpriteRenderer SpriteRenderer => spriteRenderer;

        [SerializeField, SpriteToggle] private Sprite sprite1;

    }
}
