using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue284MultipleEditingExpandable : MonoBehaviour
    {
        [Expandable] public Scriptable so;
        [Expandable] public GameObject go;
    }
}
