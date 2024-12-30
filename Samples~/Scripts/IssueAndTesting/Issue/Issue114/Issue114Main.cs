using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue114
{
    public class Issue114Main : MonoBehaviour
    {
        [RequireType(typeof(IIssue114Interface))] public MonoBehaviour issue114Script;
    }
}
