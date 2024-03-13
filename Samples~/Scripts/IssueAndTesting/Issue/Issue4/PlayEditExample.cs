using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue4
{
    public class PlayEditExample: MonoBehaviour
    {
        [DisabledIfPlay] public string disabledIfPlay;
        [ShowIfPlay] public string showIfPlay;
    }
}
