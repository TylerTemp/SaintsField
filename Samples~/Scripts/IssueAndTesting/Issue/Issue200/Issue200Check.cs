using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue200
{
    public class Issue200Check: SaintsMonoBehaviour
    {
        [Button]
        private void CheckInit()
        {
            foreach (Issue200Se child in GetComponentsInChildren<Issue200Se>())
            {
                if (child.roUnity == null)
                {
                    Debug.LogWarning($"{child.name} has no ro2");
                }

                if (child.mainC == null)
                {
                    Debug.LogError($"{child.name} has no mainC");
                }
            }
        }
    }
}
