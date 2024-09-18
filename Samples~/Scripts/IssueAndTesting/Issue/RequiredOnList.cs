using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class RequiredOnList: MonoBehaviour
    {
        [Required] public Scriptable[] scriptableArray;
    }
}
