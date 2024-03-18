using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ColorToggle : ColorToggleBase
    {
        [SerializeField, ColorToggle(nameof(Renderer))] private Color offColor;
    }
}
