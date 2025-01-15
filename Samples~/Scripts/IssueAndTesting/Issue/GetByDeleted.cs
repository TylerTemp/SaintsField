using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class GetByDeleted : SaintsMonoBehaviour
    {
        [GetComponentInChildren(excludeSelf: true)]
        public GameObject[] children;
    }
}
