using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue75 : SaintsMonoBehaviour
    {
        [GetComponent, HideIf] public Issue75 i75Hide;
        [GetComponent, DisableIf] public Issue75 i75Disable;

        [Button]
        private void PrintDebug()
        {
            Debug.Log($"i75Hide={i75Hide}");
            Debug.Log($"i75Disable={i75Disable}");
        }
    }
}
