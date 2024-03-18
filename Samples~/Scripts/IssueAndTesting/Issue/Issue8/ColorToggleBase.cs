using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ColorToggleBase : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;

        protected Renderer Renderer => targetRenderer;

        [SerializeField, ColorToggle] private Color onColor;
    }
}
