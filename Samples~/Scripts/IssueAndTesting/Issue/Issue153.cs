using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue153 : SaintsMonoBehaviour
    {
        [GetComponentInChildren(excludeSelf: true)]
        public GameObject[] childrenObjs;
    }
}
