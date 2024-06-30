using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue41
{
    public class Issue41GetComp : MonoBehaviour
    {
        [GetComponent] [GetComponentInParent] public MCUnit mcUnit;
    }
}
