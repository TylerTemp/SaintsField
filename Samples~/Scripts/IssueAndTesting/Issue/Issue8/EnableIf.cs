using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class EnableIf : EnableIfBase
    {
        [FieldEnableIf(nameof(boolV))] public int v;
    }
}
