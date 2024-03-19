using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue8
{
    public class ExpandableBase : MonoBehaviour
    {
        [Expandable, GetScriptableObject] public Scriptable scriptable;
    }
}
