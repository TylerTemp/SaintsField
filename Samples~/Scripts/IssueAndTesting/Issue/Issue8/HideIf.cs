using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class HideIf : HideIfBase
    {
        [FieldHideIf(nameof(boolV))] public int v;
    }
}
