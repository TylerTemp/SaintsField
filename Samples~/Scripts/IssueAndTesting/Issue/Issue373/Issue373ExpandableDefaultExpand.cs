using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue373
{
    public class Issue373ExpandableDefaultExpand : MonoBehaviour
    {
        [FieldDefaultExpand, Expandable] public Transform e;
        [FieldDefaultExpand, Expandable] public Transform[] eArr;
    }
}
