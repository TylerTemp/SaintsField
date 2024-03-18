using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class MaterialToggle : MaterialToggleBase
    {
        [MaterialToggle(nameof(TargetRenderer))] public Material mat2;
    }
}
