using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue145AutoGetterOrder : SaintsMonoBehaviour
    {
        [GetComponentInChildren] public GameObject[] go;
    }
}
