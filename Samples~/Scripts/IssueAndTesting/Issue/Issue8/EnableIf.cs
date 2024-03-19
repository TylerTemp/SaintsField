using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class EnableIf : EnableIfBase
    {
        [EnableIf(nameof(boolV))] public int v;
    }
}
