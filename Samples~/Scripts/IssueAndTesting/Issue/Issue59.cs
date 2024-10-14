using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue59 : MonoBehaviour
    {
        [GetComponentInParents] public SpriteRenderer notInParents;
        [GetComponentInChildren] public SpriteRenderer notInChildren;
    }
}
