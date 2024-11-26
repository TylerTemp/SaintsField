using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue103 : SaintsMonoBehaviour
    {
        [GetComponentInChildren(includeInactive: false), PostFieldButton(nameof(NativeGetComponentInChildren), "R")]
        public Transform[] children;

        private void NativeGetComponentInChildren()
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>(includeInactive: false))
            {
                Debug.Log(child);
            }
        }
    }
}
