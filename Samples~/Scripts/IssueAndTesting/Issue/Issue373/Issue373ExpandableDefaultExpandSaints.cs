using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue373
{
    public class Issue373ExpandableDefaultExpandSaints : SaintsMonoBehaviour
    {
        [DefaultExpand, Expandable] public Transform e;
        [DefaultExpand,  // default expand the array
         FieldDefaultExpand,  // default expand items inside array
         Expandable] public Transform[] eArr;
    }
}
