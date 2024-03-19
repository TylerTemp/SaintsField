using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ButtonAddOnClick : ButtonAddOnClickBase
    {
        [SerializeField, ButtonAddOnClick(nameof(ButtonClick), nameof(buttonTarget))] protected string whatever;
    }
}
